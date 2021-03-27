using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        bool isStart = false;
        bool isRun = false;

        int camera = 0;

        private List<int> cameras = new List<int>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    using (VideoCapture videoCapture = new VideoCapture()) 
                    {
                        if(videoCapture.Open(i))
                        {
                            cameras.Add(i);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            if(cameras.Count <= 1)
            {
                ChangeButton.Enabled = false;
            }
            Start();
        }

        private async void Start()
        {
            isStart = true;
            isRun = true;
            await Task.Run(() =>
            {
                using (VideoCapture videoCapture = new VideoCapture())
                {
                    videoCapture.Open(camera);
                    while (isStart)
                    {
                        using (Mat src = new Mat())
                        {
                            if (videoCapture.Read(src))
                            {
                                using (Stream stream = src.Resize(new OpenCvSharp.Size(MovieBox.Width, MovieBox.Height)).ToMemoryStream())
                                {
                                    Image img = Image.FromStream(stream);
                                    MovieBox.Image = new Bitmap(img);

                                    Task.Run(() =>
                                    {
                                        BarcodeReader reader = new BarcodeReader();
                                        Result result = reader.Decode(new Bitmap(img));
                                        if (result != null)
                                        {
                                            CodeBox.Image = img;
                                            Invoke(new MethodInvoker(() => { TextBox.Text = result.Text; }));
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                isRun = false;
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isStart = false;
        }

        private void ChangeButton_Click(object sender, EventArgs e)
        {
            isStart = false;

            // 終了待ちして再開
            while (isRun) { };

            int index = cameras.IndexOf(camera) + 1;
            if(index == cameras.Count)
            {
                index = 0;
            }
            camera = cameras[index];
            Start();
        }
    }
}

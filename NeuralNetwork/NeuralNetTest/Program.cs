using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetwork;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace NeuralNetTest
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        static void Main(string[] args)
        {
            int outnodes = 10;
            NeuralNet net = new NeuralNet(false, "SkyNet", 784, 2, 16, outnodes, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" });

            Console.WriteLine("Loading data to memory...");

            net.LoadToMemory();
            int t = 0;
            while (true)
            {
                Console.WriteLine("Test Nr {0}", t);
                Console.WriteLine("#############################################");
                string dir = @"A:\Media\dfjdh\Visual Studio 2017\Projects\NeuralNetwork\NeuralNetTest\bin\Debug\mnist_jpgfiles\test\mnist_6_559.jpg";
                var arr = net.RunImage(dir);
                for (int i = 0; i < arr.Length; i++) Console.WriteLine("Output " + i + ": " + arr[i]);
                Console.WriteLine();
                var fl = net.Cost(arr, new float[] { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 });
                Console.WriteLine("Cost: {0}, Grade: {1}", fl, net.Grade(net.GetRating(fl)));
                Console.WriteLine("#############################################");
                t++;
                Console.ReadKey();
            }
            

            //16384
            //Bitmap bit = new Bitmap("rsz_turnberry_004.jpg");
            //var bite = bit.GetPixel(1, 1).B;
            //Console.WriteLine(bite.ToString());
        }
        private static Image RezizeImage(Image img, int maxWidth, int maxHeight)
        {
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                Double xRatio = (double)img.Width / maxWidth;
                Double yRatio = (double)img.Height / maxHeight;
                Double ratio = Math.Max(xRatio, yRatio);
                int nnx = (int)Math.Floor(img.Width / ratio);
                int nny = (int)Math.Floor(img.Height / ratio);
                Bitmap cpy = new Bitmap(nnx, nny, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                        new Rectangle(0, 0, nnx, nny),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }

        }
    }
}

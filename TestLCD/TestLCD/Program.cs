using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using WiringPi;
using System.Drawing;
using ImageTransformLib;

namespace TestLCD
{
    class Program
    {
        static void Main(string[] args)
        {
            int w = 84;
            int h = 48;
            
            // use form to test on desktop
            //ILcd lcd = new TestFormLCD(w, h);

            ILcd lcd = new PCD8544Lcd(w, h);


            lcd.Connect();

            Console.WriteLine("Turning on led");
            lcd.SetLed(true);


            Random rnd = new Random();
            Bitmap bmp = new Bitmap(84, 48, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            UnsafeBitmap ubmp = new UnsafeBitmap(bmp);
            Graphics g = Graphics.FromImage(ubmp.Bitmap);

            List<Ball> balls = new List<Ball>();

            for (int i = 0; i < 10; i++)
            {
                Ball b = new Ball()
                {
                    Size = new Size(10, 10),
                    Direction = new PointF((float)rnd.NextDouble() * 4 - 2, (float)rnd.NextDouble() * 4 - 2)
                };
                balls.Add(b);
            }

            while (true)
            {
                g.Clear(Color.White);

                using (System.Drawing.Drawing2D.LinearGradientBrush b = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(0, 0), new Point(84, 48), Color.White, Color.Black))
                {
                    g.FillRectangle(b, new Rectangle(0, 0, w, h));
                }
                foreach (var b in balls)
                {
                    DrawBall(g, b);
                    UpdateBallPosition(lcd, b);
                }

                using (Font f = new Font(SystemFonts.DefaultFont.FontFamily, 20))
                {
                    g.DrawString("Test", f, Brushes.Black, new PointF(0, 0));
                }

                ubmp.LockBitmap();
                lcd.DrawBitmap(ubmp);
                ubmp.UnlockBitmap();

                lcd.Refresh();
                System.Threading.Thread.Sleep(25);
            }

            lcd.SetLed(false);


            lcd.Dispose();

        }

        private static void DrawBall(Graphics g, Ball b)
        {
            using (Brush p = new SolidBrush(Color.Black))
                g.FillEllipse(p, new Rectangle((int)b.Position.X, (int)b.Position.Y, b.Size.Width, b.Size.Height));
        }

        private static void UpdateBallPosition(ILcd lcd, Ball b)
        {
            b.Position = new PointF(b.Position.X + b.Direction.X, b.Position.Y + b.Direction.Y);
            if (b.Position.X < 0)
            {
                b.Position = new PointF(0, b.Position.Y);
                b.Direction = new PointF(-b.Direction.X, b.Direction.Y);
            }
            if (b.Position.Y < 0)
            {
                b.Position = new PointF(b.Position.X, 0);
                b.Direction = new PointF(b.Direction.X, -b.Direction.Y);
            }

            if (b.Position.X + b.Size.Width >= lcd.Width)
            {
                b.Position = new PointF(lcd.Width - b.Size.Width - 1, b.Position.Y);
                b.Direction = new PointF(-b.Direction.X, b.Direction.Y);
            }

            if (b.Position.Y + b.Size.Height >= lcd.Height)
            {
                b.Position = new PointF(b.Position.X, lcd.Height - b.Size.Height - 1);
                b.Direction = new PointF(b.Direction.X, -b.Direction.Y);
            }
        }

        private class Ball
        {
            public PointF Position { get; set; }
            public Size Size { get; set; }

            public PointF Direction { get; set; }
        }

    }



}

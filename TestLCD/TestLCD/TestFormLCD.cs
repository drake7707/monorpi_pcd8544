using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace TestLCD
{
    public class TestFormLCD : Form, ILcd
    {
        private Bitmap bmp;

        public new int Width { get; set; }
        public new int Height { get; set; }
        public TestFormLCD(int w, int h)
        {
            this.Width = w;
            this.Height = h;
            bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }
            this.BackgroundImage = bmp;
            this.BackgroundImageLayout = ImageLayout.Center;

            Dither = true;
            DitherPattern = DitheringTypeEnum.Pattern4;

            this.Show();
        }


        public void Clear()
        {

        }

        public void Connect()
        {
          
        }

        public bool Dither { get; set; }
        public DitheringTypeEnum DitherPattern { get; set; }
       
        private const byte MONOCHROME_DITHER_LEVEL = 1;
        private float DitherFunction(float input, int x, int y, DitheringTypeEnum pattern, byte levels)
        {
            byte c = 0;
            double mask = 0;
            switch ((int)pattern)
            {
                case 1: mask = ((x ^ y * 149) * 1234 & 511) / 511.0f; break;
                case 2: mask = (((x + c * 17) ^ y * 149) * 1234 & 511) / 511.0f; break;
                case 3: mask = ((x + y * 237) * 119 & 255) / 255.0f; break;
                case 4: mask = (((x + c * 67) + y * 236) * 119 & 255) / 255.0f; break;
                case 5: mask = 0.5f; break;
                default: return input;
            }
            return ((float)Math.Floor(levels * input + mask) / levels);
        }

        public unsafe void DrawBitmap(ImageTransformLib.UnsafeBitmap ubmp)
        {


            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    var c = ubmp.GetPixel(i, j);
                    var greyscale = (c.R + c.G + c.B) / 3;
                    byte val = (byte)(DitherFunction(greyscale / 255f, i, j, DitherPattern, MONOCHROME_DITHER_LEVEL) * 255f);
                   

                    if (val > 128)
                        this[i, j] = false;
                    else
                        this[i, j] = true;
                }
            }
        }

        public void DrawBitmap(System.Drawing.Bitmap bmp)
        {

        }

        public void Reset()
        {

        }

        public new void Refresh()
        {
            this.BackgroundImage = null;
            this.BackgroundImage = bmp;
            Application.DoEvents();
        }

        public void SetContrast(byte contrast)
        {

        }

        public void SetLed(bool on)
        {

        }

        public void SetPosition(int x, int y)
        {

        }

        public bool this[int x, int y]
        {
            get
            {
                var c = bmp.GetPixel(x, y);
                return c.R != 0 || c.G != 0 || c.B != 0;
            }
            set
            {
                bmp.SetPixel(x, y, value ? Color.Black : Color.White);
            }
        }
    }
}

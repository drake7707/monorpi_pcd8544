using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WiringPi;
using ImageTransformLib;
using System.Drawing;

namespace TestLCD
{
    public class PCD8544Lcd : IDisposable, TestLCD.ILcd
    {
        private int pinDC = 4;
        private int pinRST = 5;
        private int pinLED = 1;
        private int pinSCE = 10;
        private int pinSCLK = 14;
        private int pinDIN = 12;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private byte[] displayData;

        private const byte BITS_PER_BYTE = 8;

        public PCD8544Lcd(int lcdWidth, int lcdHeight)
        {
            this.Width = lcdWidth;
            this.Height = lcdHeight;
            displayData = new byte[lcdWidth * lcdHeight / BITS_PER_BYTE];

            Dither = true;
            DitherPattern = DitheringTypeEnum.Pattern4;


        }

        public void Connect()
        {
            int result = WiringPi.Init.WiringPiSetup();
            if (result == -1)
            {
                Console.WriteLine("WiringPi init failed!");
                return;
            }

            WiringPi.SPI.wiringPiSPISetup(0, 4000000);
            GPIO.pinMode(pinDC, WiringPi.GPIO.GPIOpinmode.Output);
            GPIO.pinMode(pinRST, WiringPi.GPIO.GPIOpinmode.Output);
            GPIO.pinMode(pinLED, WiringPi.GPIO.GPIOpinmode.Output);
            SetLed(false);

            Reset();
        }

        public bool this[int x, int y]
        {
            get
            {
                if ((x >= 0) && (x < Width) && (y >= 0) && (y < Height))
                {
                    byte shift = (byte)(y % BITS_PER_BYTE);
                    return (displayData[x + (y / BITS_PER_BYTE) * Width] & (1 << shift)) != 0;
                }
                else
                    throw new ArgumentOutOfRangeException();
            }
            set
            {
                if ((x >= 0) && (x < Width) && (y >= 0) && (y < Height))
                {
                    byte shift = (byte)(y % BITS_PER_BYTE);

                    if (value) // If black, set the bit.
                        displayData[x + (y / BITS_PER_BYTE) * Width] |= (byte)(1 << shift);
                    else   // If white clear the bit.
                        displayData[x + (y / BITS_PER_BYTE) * Width] &= (byte)~(1 << shift);
                }
                else
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void DrawBitmap(Bitmap bmp)
        {
            UnsafeBitmap ubmp = new UnsafeBitmap(bmp);
            ubmp.LockBitmap();
            try
            {
                DrawBitmap(ubmp);
            }
            finally
            {
                ubmp.UnlockBitmap();
            }
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


        public void SetLed(bool on)
        {
            GPIO.digitalWrite(pinLED, on ? 1 : 0);
        }

        private void LCDWrite(bool isData, byte[] data)
        {
            GPIO.digitalWrite(pinDC, isData ? 1 : 0);
            SPIWrite(data);
        }

        public void SetPosition(int x, int y)
        {
            GPIO.digitalWrite(pinDC, 0);
            SPIWrite((byte)(x + 128), (byte)(y + 64));
        }

        public void Clear()
        {
            SetPosition(0, 0);
            for (int i = 0; i < displayData.Length; i++)
                displayData[i] = 0;
            Refresh();
            SetContrast(0xAA);
        }

        public void Refresh()
        {
            SetPosition(0, 0);
            LCDWrite(true, displayData);
        }

        public void SetContrast(byte contrast)
        {
            if (0x80 <= contrast && contrast < 0xFF)
            {
                GPIO.digitalWrite(pinDC, 0);
                SPIWrite(0x21, 0x14, contrast, 0x20, 0x0c);
            }

        }

        private void SPIWrite(params byte[] bytes)
        {
            unsafe
            {
                fixed (byte* bptr = bytes)
                {
                    SPI.wiringPiSPIDataRW(0, bptr, bytes.Length);
                }
            }

        }
        public void Reset()
        {
            GPIO.digitalWrite(pinRST, 0);
            Thread.Sleep(200);
            GPIO.digitalWrite(pinRST, 1);
            Clear();
            SetContrast(0xAA);
        }

        public void Dispose()
        {

        }



    }

}

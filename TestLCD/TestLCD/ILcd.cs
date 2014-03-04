using System;
namespace TestLCD
{
    interface ILcd
    {
         
        void Clear();
        void Connect();
        void Dispose();
        unsafe void DrawBitmap(ImageTransformLib.UnsafeBitmap ubmp);
        void DrawBitmap(System.Drawing.Bitmap bmp);
        int Height { get; }
        void Refresh();
        void Reset();
        void SetContrast(byte contrast);
        void SetLed(bool on);
        void SetPosition(int x, int y);
        bool this[int x, int y] { get; set; }
        int Width { get; }

        bool Dither { get; set; }
        DitheringTypeEnum DitherPattern { get; set; }
    }

    public enum DitheringTypeEnum
    {
        Pattern1 = 1,
        Pattern2 = 2,
        Pattern3 = 3,
        Pattern4 = 4,
        Pattern5 = 5
    }

}

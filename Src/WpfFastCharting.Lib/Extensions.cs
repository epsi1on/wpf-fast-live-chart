using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfFastCharting.Lib
{
    public static class Extensions
    {
        public static void DrawLine(this WriteableBitmap bmp, Point p1, Point p2, Color cl, int thickness = 1)
        {
            var x1 = (int)p1.X;
            var x2 = (int)p2.X;

            var y1 = (int)p1.Y;
            var y2 = (int)p2.Y;

            var dx = x2 - x1;
            var dy = y2 - y1;

            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;

            if (dx < 1 && dy < 1)
                return;//bug in draw line which do crash, also line will be a dot, so ignore it

            bmp.DrawLine(x1, y1, x2, y2, cl);
            //bmp.DrawLineAa(x1, y1, x2, y2, cl, thickness);
        }

        public static void DrawLineD(this WriteableBitmap bmp, double xd1,double yd1, double xd2,double yd2, Color cl, int thickness = 1)
        {
            var x1 = (int)xd1;
            var y1 = (int)yd1;

            var x2= (int)xd2;
            var y2= (int)yd2;

            var dx = x2 - x1;
            var dy = y2 - y1;

            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;

            if (dx < 1 && dy < 1)
                return;//bug in draw line which do crash, also line will be a dot, so ignore it

            bmp.DrawLine(x1, y1, x2, y2, cl);
            //bmp.DrawLineAa(x1, y1, x2, y2, cl, thickness);
        }

        public static void DrawLine(this WriteableBitmap bmp, Point p1, Point p2, int thickness = 1)
        {
            DrawLine(bmp, p1, p2, Colors.Black, thickness);
        }

        public static void DrawLineDotted(this WriteableBitmap bmp, Point p1, Point p2, Color cl)
        {
            bmp.DrawLineDotted((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, 3, 3, cl);
        }


        public static void DrawLineDotted(this WriteableBitmap bmp, double x1, double y1, double x2,double y2, Color cl)
        {
            bmp.DrawLineDotted((int)x1, (int)y1, (int)x2, (int)y2, 3, 3, cl);
        }

        public static void DrawString(this WriteableBitmap bmp, Point p1, string text, WriteableBitmap rbmp,Dictionary<char,Rect> dic)
        {
            var x = p1.X;
            var y = p1.Y;

            for (var i = 0; i < text.Length; i++)
            {
                var r = dic[text[i]];

                var taget = new Rect(x, y, r.Width, r.Height);

                bmp.Blit(taget, rbmp, r, WriteableBitmapExtensions.BlendMode.Alpha);
                x += r.Width;

            }
        }
    }

    
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfFastCharting.Lib
{
    public class WriteableBitmapFontManager
    {
        public void Init(Typeface typeFace,double fontSize,Color color)
        {
            Characters = BitmapFactory.New(160, 160);

            var wi = 16;
            var hi = 16;
            var size = 10;

            char[] Chars = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,;:?!-_~#""'&()[]|`\/@°+=*$£€<>%".ToCharArray();

            var rows = (int)Math.Sqrt(Chars.Length) + 1;
            var cols = (int)Math.Sqrt(Chars.Length);

            var rnd = new RenderTargetBitmap(rows * hi, cols * wi, 96, 96, PixelFormats.Pbgra32);

            for (var i = 0; i < Chars.Length; i++)
            {
                var txt = Chars[i].ToString();

                var row = i % cols;
                var col = i / cols;

                var x = row * wi;
                var y = col * hi;

                {
                    var bi = rnd;
                    FormattedText text = new FormattedText(txt, new CultureInfo("en-us"), FlowDirection.LeftToRight, typeFace, fontSize, Brushes.Black);

                    DrawingVisual drawingVisual = new DrawingVisual();
                    DrawingContext drawingContext = drawingVisual.RenderOpen();

                    drawingContext.DrawText(text, new Point(x, y));
                    drawingContext.Close();

                    var rect = drawingVisual.ContentBounds;

                    bi.Render(drawingVisual);

                    //var yyy = rect.Y;
                    //yyy -= hi;
                    //rect.Height = yyy;

                    chars[Chars[i]] = rect;
                }

                
            }

            Characters = BitmapFactory.ConvertToPbgra32Format(rnd);

            /** /
            {
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(Characters));

                using (var fs = System.IO.File.OpenWrite(@"c:\\temp\\ff.png"))
                    enc.Save(fs);

            }/**/
        }

        WriteableBitmap Characters;

        private Dictionary<char, Rect> chars = new Dictionary<char, Rect>();

        public void DrawString(WriteableBitmap bmp, Point p1, string text)
        {
            WriteableBitmap rbmp = Characters;
            Dictionary<char, Rect> dic = chars;


            var x = p1.X;
            var y = p1.Y;

            for (var i = 0; i < text.Length; i++)
            {
                var r = dic[text[i]];

                var taget = new Rect(x, y, r.Width, r.Height);

                var delta = 16 - r.Height;

                taget.Y += delta;

                bmp.Blit(taget, rbmp, r, WriteableBitmapExtensions.BlendMode.Alpha);
                x += r.Width;

            }
        }

        public void DrawStringVertical(WriteableBitmap bmp, Point p1, string text)
        {
            WriteableBitmap rbmp = Characters;
            Dictionary<char, Rect> dic = chars;


            var x = p1.X;
            var y = p1.Y;

            for (var i = 0; i < text.Length; i++)
            {
                var r = dic[text[i]];

                var taget = new Rect(x, y, r.Width, r.Height);

                var delta = 16 - r.Height;

                taget.Y += delta;

                bmp.Blit(taget, rbmp, r, WriteableBitmapExtensions.BlendMode.Alpha);
                x += r.Width;

            }
        }
    }
}

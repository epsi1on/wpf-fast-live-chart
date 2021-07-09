using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WpfFastCharting.Lib
{
    public class MatrixTransformation
    {
        public double m11, m12, m21, m22, dx, dy;

        public double _xMin, _yMin, _xMax, _yMax, _w, _h;

        public Thickness t;

        public Point Transform(Point p)
        {
            var x = m11 * p.X + m12 * p.Y + dx;
            var y = m21 * p.X + m22 * p.Y + dy;

            return new Point(x, y);
        }

        public Point Transform(double X,double Y)
        {
            var x = m11 * X + m12 * Y + dx;
            var y = m21 * X + m22 * Y + dy;

            return new Point(x, y);
        }

        public void Transform(double x,double y,out double xo,out double yo)
        {
            xo = m11 * x + m12 * y + dx;
            yo = m21 * x + m22 * y + dy;
        }

        public Point TransformBack(Point p)
        {
            var det = m11 * m22 - m12 * m21;

            var m11p = m22 / det;
            var m12p = -m12 / det;
            var m21p = -m21 / det;
            var m22p = m11 / det;


            var x = -dx * m22 + dy * m12 - m12 * p.Y + m22 * p.X;
            var y = dx * m21 - dy * m11 + m11 * p.Y - m21 * p.X;

            return new Point(x / det, y / det);
        }

        public void TransformBack(double x, double y, out double xi, out double yi)
        {
            var det = m11 * m22 - m12 * m21;

            var m11p = m22 / det;
            var m12p = -m12 / det;
            var m21p = -m21 / det;
            var m22p = m11 / det;


            xi = -dx * m22 + dy * m12 - m12 * y + m22 * x;
            yi = dx * m21 - dy * m11 + m11 * y - m21 * x;
        }

        public static MatrixTransformation FromWindow(double xMin, double yMin, double xMax, double yMax, double w, double h)
        {
            var buf = new MatrixTransformation();

            buf.dx = (-w * xMin) / (xMax - xMin);
            buf.dy = (h * yMax) / (yMax - yMin);

            buf.m11 = w / (xMax - xMin);
            buf.m22 = -h / (yMax - yMin);


            buf._xMin = xMin;
            buf._yMin = yMin;
            buf._xMax = xMax;
            buf._yMax = yMax;
            buf._h = h;
            buf._w = w;

            return buf;
        }

        public static MatrixTransformation FromWindow(double xMin, double yMin, double xMax, double yMax, double w, double h,Thickness t)
        {
            var ml = t.Left;
            var mr = t.Right;
            var mt = t.Bottom;
            var mb = t.Top;

            var buf = new MatrixTransformation();

            buf.m11 = (-ml - mr + w) / (xMax - xMin);
            buf.m12 = buf.m21 = 0;
            buf.m22 = (-h + mb + mt) / (yMax - yMin);

            buf.dx = (ml * xMax + mr * xMin - w * xMin) / (xMax - xMin);
            buf.dy = (h * yMax - mb * yMin - mt * yMax) / (yMax - yMin);

            buf._xMin = xMin;
            buf._yMin = yMin;
            buf._xMax = xMax;
            buf._yMax = yMax;
            buf.t = t;

            return buf;
        }
    }
}

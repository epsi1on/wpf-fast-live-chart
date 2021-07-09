using System;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfFastCharting.Lib;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var x0 = 70;
            var x1 = 100;

            var y0 = 50;
            var y1 = 60;

            var w = 500;
            var h = 400;

            var tr = MatrixTransformation.FromWindow(x0, y0, x1, y1, w, h);

            var p0 = new Point(x0, y0);
            var p1 = new Point(x1, y1);

            var p0t = tr.Transform(p0);
            var p1t = tr.Transform(p1);

            var p0r = tr.TransformBack(p0t);
            var p1r = tr.TransformBack(p1t);

        }
    }
}

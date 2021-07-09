using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfFastCharting.Lib;

namespace ConsoleApp1
{
    class Program
    {

        public class WindosPointChartMapper : IChartPointMapper
        {
            public void Map(object obj, out double x, out double y)
            {
                var t = (Point)obj;

                x = t.X;
                y = t.Y;
            }

        }


        [STAThread]
        static void Main(string[] args)
        {
            var ctrl = new LineChart();

            var wnd = new Window();

            wnd.Content = ctrl;



            var rnd = new Random();

            var sp = System.Diagnostics.Stopwatch.StartNew();

            ObservableCollection<System.Windows.Point> src;

            ctrl.Source = src = new ObservableCollection<Point>();

            ctrl.PointMapper = new WindosPointChartMapper();// TranslateDelegate;

            var cnt = 0;

            Task.Run(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    Thread.Sleep(10);

                    var y = rnd.NextDouble() * 100 + 100;

                    y = cnt++ % 30;

                    y = 100 * Math.Sin(3000 * cnt);

                    src.Add(new Point(sp.ElapsedMilliseconds, y));
                }
            });

            wnd.ShowDialog();

        }

        static void TranslateDelegate(object obj, out double x, out double y)
        {
            var t = (Point)obj;

            x = t.X;
            y = t.Y;
        }
    }
}

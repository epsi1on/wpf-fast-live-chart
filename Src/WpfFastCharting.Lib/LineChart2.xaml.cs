using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfFastCharting.Lib
{

    internal interface ILockable
    {
        void Lock();
        void Unlock();
        bool IsLocked();
    }

    internal class Locker : IDisposable
    {
        public ILockable Target;

        public Locker(ILockable tar)
        {
            Target = tar;
            tar.Lock();
        }

        public void Dispose()
        {
            Target.Unlock();
        }
    }


    /// <summary>
    /// Interaction logic for LineChart2.xaml
    /// </summary>
    public partial class LineChart : UserControl,ILockable
    {
        public LineChart()
        {
            InitializeComponent();

            this.SizeChanged += (a, b) => RecreateCanvas();
            this.Loaded += (a, b) => RecreateCanvas();
            textWriter = new WriteableBitmapFontManager();
            var fc = new Typeface(this.FontFamily, FontStyles.Normal, FontWeights.Normal, new FontStretch());

            textWriter.Init(fc, 12, Colors.Black);

            
        }

        WriteableBitmapFontManager textWriter;


        double w, h;

        private void RecreateCanvas()
        {
            w = this.ActualWidth;
            h = this.ActualHeight;

            var max = int.MaxValue;
            var min = 100;

            w = Math.Min(w, max);
            w = Math.Max(w, min);

            h = Math.Min(h, max);
            h = Math.Max(h, min);

            Canvas = BitmapFactory.New((int)w, (int)h);
            MainImg.Source = Canvas;

            CanvasRender = new RenderTargetBitmap(Canvas.PixelWidth, Canvas.PixelHeight, Canvas.DpiX, Canvas.DpiY, Canvas.Format);

        }

        private long LastUpdate;

        private Stopwatch sp = Stopwatch.StartNew();

        System.Threading.Mutex renderMutex = new System.Threading.Mutex();

        private object lockerz = new object();



        private bool[] Simplified = new bool[10000];

        private double x0, xp0, xz0;//last draw, used for mode = Ecg
        private double lastPointX;
        private double lastWindowEnd = 0;


        private void RenderCanvas()
        {
            if (this.IsLocked())
                return;

            
            //try
            {
                using (new Locker(this))
                {
                    var writeableBmp = Canvas;


                    if (DoNotUpdate)
                        return;

                    {//preserve MAX FPS
                        var minInterval = 1 / MaxFps;

                        if (sp.ElapsedMilliseconds < LastUpdate + minInterval)
                        {
                            return;
                        }
                    }

                    LastUpdate = sp.ElapsedMilliseconds;

                    // Init vars

                    // Start render loop

                    var ic = Source as ICollection;
                    var il = Source as IList;
                    var ie = Source as IEnumerable;

                    if (il == null)
                        return;


                    if (il.Count < 2)
                        return;


                    IDisposable locker = null;

                    //if (Source is ILockable lc)
                    {
                        //locker = lc.GetLocker();
                    }

                    var map = this.PointMapper;

                    double latestX;//latest data X
                    int startX = -1;//start index of point for render

                    {
                        double y;

                        map.Map(il[il.Count - 1], out latestX, out y);

                        double tmpX, tmpY;

                        for (int i = il.Count-1; i >= 0; i--)
                        {
                            map.Map(il[i], out tmpX, out tmpY);

                            if (latestX - tmpX > TailSpanWidth)
                            {
                                startX = i;
                                break;
                            }
                        }
                    }


                    var cnt = il.Count - startX;//show count

                    if (DrawMode == DrawMode.AllOfData)
                        cnt = int.MaxValue;

                    using (locker)
                    {
                        double minX = double.MaxValue, minY = double.MaxValue;
                        double maxX = double.MinValue, maxY = double.MinValue;


                        var st = il.Count - cnt;

                        if (st < 0) st = 0;

                        //calc min max
                        for (var i = st; i < il.Count; i++)
                        {
                            var item = il[i];

                            double x, y;

                            map.Map(item, out x, out y);


                            if (x > maxX) maxX = x;
                            if (x < minX) minX = x;
                            if (y > maxY) maxY = y;
                            if (y < minY) minY = y;
                        }

                        if (minX < double.MinValue || maxX > double.MaxValue ||
                            minY < double.MinValue || maxY > double.MaxValue)
                            return;

                        //Console.WriteLine(string.Format("data spans: {0},{1},{2},{3}", minX, maxX, minY, maxY));

                        if (DrawMode == DrawMode.TailOfData || DrawMode == DrawMode.TailOfDataEkg)
                        {
                            minX = maxX - TailSpanWidth;
                        }

                        


                        var xTicks = GetScaleDetails(minX, maxX);
                        var yTicks = GetScaleDetails(minY, maxY);

                        /*
                        {//
                            minX = (double)xTicks.Item1;
                            maxX = (double)xTicks.Item2;

                            minY = (double)yTicks.Item1;
                            maxY = (double)yTicks.Item2;
                        }
                        */


                        var absMaxX = Math.Max(Math.Abs(minX), Math.Abs(maxX));
                        var absMaxY = Math.Max(Math.Abs(minY), Math.Abs(maxY));

                        var lx = (int)Math.Log10(absMaxX) + 1;
                        var ly = (int)Math.Log10(absMaxY) + 1;

                        var t = new Thickness(100, 10, 10, 20);

                        var wp = w - t.Left - t.Right;
                        var hp = h - t.Bottom - t.Top;



                        OneDTransformation xtrans, ytrans;


                        {
                            //var minx_ = RefreshMode == RefreshMode.Normal ? minX : maxX - TailSpanWidth;

                            xtrans = OneDTransformation.FromInOut(minX, maxX, 0 + t.Left, w - t.Right);

                            if (YScaleMode == ScaleMode.Manual)
                                ytrans = new OneDTransformation(YScale, h / 2 - t.Top);
                            else
                                ytrans = OneDTransformation.FromInOut(minY, maxY, h - t.Bottom, t.Top);

                        }

                        /*
                        var delta = 0.0;

                        if (RefreshMode == RefreshMode.Ecgish)
                        {
                            var xp = xtrans.Transform(x0);

                            delta = xp0 - xp;

                            if (xp >= wp - delta)
                            {
                                //nothing
                            }
                            else
                            {
                                //delta -= wp;
                            }

                            delta = 0;//delta % wp;
                        }
                        else
                        {
                            delta = 0;
                        }
                        */

                        //correct x for ecg mode

                        var correctXp = new Func<double, double>(xp =>
                        {

                            if (DrawMode !=DrawMode.TailOfDataEkg)
                                return xp;

                            //mahalle boresh ine
                            var lastWindowEndpP = xtrans.Transform(lastWindowEnd);

                            var xpmin = xtrans.Transform(minX);
                            var xpmax = xtrans.Transform(maxX);

                            if (xp > lastWindowEndpP)
                                return xp - (lastWindowEndpP - xpmin);
                            else
                                return xp - (lastWindowEndpP - xpmax);
                        });

                        { //draw

                            using (writeableBmp.GetBitmapContext())
                            {
                                writeableBmp.Clear(Colors.White);
                                //writeableBmp.FillRectangle(0, 0, (int)w, (int)h, Colors.White);

                                if(ShowGrids)
                                {//grid thicks

                                    {//vertical lines

                                        var deltaX = (double)(xTicks.Item2 - xTicks.Item1);
                                        var xCnt = 1 + Math.Ceiling(deltaX / (double)xTicks.Item3);

                                        for (var i = 0; i <= xCnt; i++)
                                        {
                                            var X = (double)(minX + (deltaX * i) / xCnt);

                                            var px = xtrans.Transform(X);

                                            px = correctXp(px);//.X = (p0.X + delta) ;

                                            writeableBmp.DrawLineDotted(px, t.Top, px, h - t.Bottom, Colors.Gray);

                                            if (i % 4 == 0)
                                            {
                                                writeableBmp.DrawLineD(px, t.Top, px, h - t.Bottom, Colors.Gray, 2);

                                                var pp = new Point();

                                                pp.Y = h - 20;// - transf.t.Right;
                                                pp.X = px - 10;

                                                textWriter.DrawString(writeableBmp, pp, X.ToString("N2"));
                                            }
                                        }
                                    }


                                    {//horizontal lines (minor)
                                     /**/
                                        var deltaY = (double)(yTicks.Item2 - yTicks.Item1);
                                        var yCnt = 1 + Math.Ceiling(deltaY / (double)yTicks.Item3);



                                        for (var i = 0; i <= yCnt; i++)
                                        {
                                            var Y = (double)(minY + (deltaY * i) / yCnt);

                                            //if (i == yCnt)
                                            //    Y--;

                                            var py = ytrans.Transform(Y);

                                            //var p0 = transf.Transform(minX, Y);
                                            //var p1 = transf.Transform(maxX, Y);

                                            writeableBmp.DrawLineDotted(t.Left, py, w - t.Right, py, Colors.Gray);

                                            //writeableBmp.DrawLineDotted(p0, p1, Colors.Gray);

                                            if (i % 4 == 0)
                                            {
                                                writeableBmp.DrawLineD(t.Left, py, w - t.Right, py, Colors.Gray);
                                                //writeableBmp.DrawLine(p0, p1, Colors.Gray, 2);

                                                var pp = new Point();

                                                pp.X = 0;// - transf.t.Right;
                                                pp.Y = py - 12;

                                                textWriter.DrawString(writeableBmp, pp, Y.ToString("N2"));
                                            }

                                        }
                                        /**/
                                    }

                                }


                                {//graph
                                 /**/
                                    for (var i = st; i < il.Count - 1; i++)
                                    {
                                        var o1 = il[i];
                                        var o2 = il[i + 1];

                                        double x1, y1, x2, y2;

                                        map.Map(o1, out x1, out y1);
                                        map.Map(o2, out x2, out y2);


                                        var xp1 = xtrans.Transform(x1);
                                        var xp2 = xtrans.Transform(x2);

                                        var yp1 = ytrans.Transform(y1);
                                        var yp2 = ytrans.Transform(y2);

                                        var xz1 = correctXp(xp1);
                                        var xz2 = correctXp(xp2);

                                        if (xz1 < xz2)
                                            writeableBmp.DrawLineD(xz1, yp1, xz2, yp2, Colors.Black, 1);
                                    }

                                    /**/
                                    if (DrawMode == DrawMode.TailOfDataEkg)
                                    {
                                        var o1 = il[il.Count - 1];
                                        
                                        double x, y;

                                        map.Map(o1, out x, out y);

                                        if (x - lastWindowEnd >= TailSpanWidth)
                                        {
                                            do
                                            {
                                                lastWindowEnd += TailSpanWidth;
                                            } while (lastWindowEnd + TailSpanWidth < maxX);

                                        }

                                        var xz = correctXp(xtrans.Transform(x));
                                        var yp = ytrans.Transform(y);

                                        var www = 3;

                                        writeableBmp.FillRectangle((int)xz - www, (int)yp - www, (int)xz + www, (int)yp + www,
                                            Colors.Black);

                                        writeableBmp.DrawLine((int)xz, (int)t.Top, (int)xz, (int)(h - t.Bottom),
                                            Colors.Black);
                                    } /**/

                                    /**/
                                }



                            }


                            //writeableBmp.Unlock();
                        }
                    }
                }
            }
            //catch
            {

            }
            

           

        }


        private bool _IsLocked = false;

        public void Lock()
        {
            _IsLocked = true;
        }

        public void Unlock()
        {
            _IsLocked = false;
        }

        public bool IsLocked()
        {
            return _IsLocked;
        }


        private static Tuple<decimal, decimal, decimal> GetScaleDetails_dec(decimal min, decimal max)
        {
            //from https://stackoverflow.com/a/49911176
            // Minimal increment to avoid round extreme values to be on the edge of the chart
            decimal epsilon = (max - min) / 1e6m;
            max += epsilon;
            min -= epsilon;
            decimal range = max - min;

            // Target number of values to be displayed on the Y axis (it may be less)
            int stepCount = 20;
            // First approximation
            decimal roughStep = range / (stepCount - 1);

            // Set best step for the range
            decimal[] goodNormalizedSteps = { 1, 1.5m, 2, 2.5m, 5, 7.5m, 10 }; // keep the 10 at the end
                                                                               // Or use these if you prefer:  { 1, 2, 5, 10 };

            // Normalize rough step to find the normalized one that fits best
            decimal stepPower = (decimal)Math.Pow(10, -Math.Floor(Math.Log10((double)Math.Abs(roughStep))));
            var normalizedStep = roughStep * stepPower;
            var goodNormalizedStep = goodNormalizedSteps.First(n => n >= normalizedStep);
            decimal step = goodNormalizedStep / stepPower;

            // Determine the scale limits based on the chosen step.
            decimal scaleMax = Math.Ceiling(max / step) * step;
            decimal scaleMin = Math.Floor(min / step) * step;

            return new Tuple<decimal, decimal, decimal>(scaleMin, scaleMax, step);
        }

        private static Tuple<double, double, double> GetScaleDetails(double min, double max)
        {
            if (min == max)
            {
                return new Tuple<double, double, double>(min - 10, min + 10, 10);
            }
            //from https://stackoverflow.com/a/49911176
            // Minimal increment to avoid round extreme values to be on the edge of the chart
            double epsilon = (max - min) / 1e6;
            max += epsilon;
            min -= epsilon;
            double range = max - min;

            // Target number of values to be displayed on the Y axis (it may be less)
            int stepCount = 20;
            // First approximation
            double roughStep = range / (stepCount - 1);

            // Set best step for the range
            double[] goodNormalizedSteps = { 1e-5, 0.001, 0.01, 1, 1.5, 2, 2.5, 5, 7.5, 10 }; // keep the 10 at the end
                                                                              // Or use these if you prefer:  { 1, 2, 5, 10 };

            // Normalize rough step to find the normalized one that fits best
            double stepPower = (double)Math.Pow(10, -Math.Floor(Math.Log10((double)Math.Abs(roughStep))));
            var normalizedStep = roughStep * stepPower;
            var goodNormalizedStep = goodNormalizedSteps.First(n => n >= normalizedStep);
            double step = goodNormalizedStep / stepPower;

            // Determine the scale limits based on the chosen step.
            double scaleMax = Math.Ceiling(max / step) * step;
            double scaleMin = Math.Floor(min / step) * step;

            return new Tuple<double, double, double>(scaleMin, scaleMax, step);
        }

        #region properties


        public bool DoPopOut
        {
            get { return (bool)GetValue(DoPopOutProperty); }
            set { SetValue(DoPopOutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DoPopOut.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoPopOutProperty =
            DependencyProperty.Register("DoPopOut", typeof(bool), typeof(LineChart), new PropertyMetadata(true));





        public bool DoNotUpdate
        {
            get { return (bool)GetValue(DoNotUpdateProperty); }
            set { SetValue(DoNotUpdateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DoNotUpdate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoNotUpdateProperty =
            DependencyProperty.Register("DoNotUpdate", typeof(bool), typeof(LineChart), new PropertyMetadata(false));




        public INotifyCollectionChanged Source
        {
            get { return (INotifyCollectionChanged)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(INotifyCollectionChanged), typeof(LineChart), new PropertyMetadata(null, SourcePropertyChanged));

        static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tt = d as LineChart;



            var old = e.OldValue as INotifyCollectionChanged;

            if (old != null)
                old.CollectionChanged -= tt.Old_CollectionChanged;



            var nw = e.NewValue as INotifyCollectionChanged;

            if (nw != null)
                nw.CollectionChanged += tt.Old_CollectionChanged;

        }

        private void Old_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Dispatcher.Invoke(new Action(() => this.RenderCanvas()));
            }
        }


        public static readonly DependencyProperty ShowGridsProperty = DependencyProperty.Register(
            nameof(ShowGrids), typeof(bool), typeof(LineChart), new PropertyMetadata(true));

        public bool ShowGrids
        {
            get { return (bool)GetValue(ShowGridsProperty); }
            set { SetValue(ShowGridsProperty, value); }
        }


        [Obsolete("user DurationToShow instead")]
        //last points to show (count)
        //صد تا نقطه آخر رو نشون بده
        public int PointsToShow
        {
            get { return (int)GetValue(PointsToShowProperty); }
            set { SetValue(PointsToShowProperty, value); }
        }

        
        


        public static readonly DependencyProperty TailSpanWidthProperty = DependencyProperty.Register(
            nameof(TailSpanWidth), typeof(double), typeof(LineChart), new PropertyMetadata(10.0));

        public double TailSpanWidth
        {
            get { return (double)GetValue(TailSpanWidthProperty); }
            set { SetValue(TailSpanWidthProperty, value); }
        }

        
        // Using a DependencyProperty as the backing store for PointsToShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsToShowProperty =
            DependencyProperty.Register("PointsToShow", typeof(int), typeof(LineChart), new PropertyMetadata(100));

        /*
        [Obsolete]
        //show all points (PointsToShow = infinity)
        public bool ShowAllPoints
        {
            get { return (bool)GetValue(ShowAllPointsProperty); }
            set { SetValue(ShowAllPointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowAllPoints.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAllPointsProperty =
            DependencyProperty.Register("ShowAllPoints", typeof(bool), typeof(LineChart), new PropertyMetadata(true));

        */
        //maximum frame per second render
        public double MaxFps
        {
            get { return (double)GetValue(MaxFpsProperty); }
            set { SetValue(MaxFpsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxFps.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxFpsProperty =
            DependencyProperty.Register("MaxFps", typeof(double), typeof(LineChart), new PropertyMetadata(25.0));





        public IChartPointMapper PointMapper
        {
            get { return (IChartPointMapper)GetValue(PointMapperProperty); }
            set { SetValue(PointMapperProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PointMapper.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointMapperProperty =
            DependencyProperty.Register("PointMapper", typeof(IChartPointMapper), typeof(LineChart), new PropertyMetadata(null));




        public static readonly DependencyProperty DrawModeProperty = DependencyProperty.Register(
            nameof(DrawMode), typeof(DrawMode), typeof(LineChart), new PropertyMetadata(default(DrawMode)));

        public DrawMode DrawMode
        {
            get { return (DrawMode)GetValue(DrawModeProperty); }
            set { SetValue(DrawModeProperty, value); }
        }



        public static readonly DependencyProperty YScaleProperty = DependencyProperty.Register(
            nameof(YScale), typeof(double), typeof(LineChart), new PropertyMetadata(0.0));

        public double YScale
        {
            get { return (double)GetValue(YScaleProperty); }
            set { SetValue(YScaleProperty, value); }
        }



        public static readonly DependencyProperty YScaleModeProperty = DependencyProperty.Register(
            nameof(YScaleMode), typeof(ScaleMode), typeof(LineChart), new PropertyMetadata(ScaleMode.Auto));

        public ScaleMode YScaleMode
        {
            get { return (ScaleMode)GetValue(YScaleModeProperty); }
            set { SetValue(YScaleModeProperty, value); }
        }

        #endregion

        //public delegate void TranslateDelegate(object obj, out double x, out double y);

        //public TranslateDelegate Translate;

        public WriteableBitmap Canvas { get; private set; }

        private void BtnPopOut_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new Window();

            var ctrl = new LineChart();

            ctrl.Source = this.Source;
            ctrl.PointMapper = this.PointMapper;
            //ctrl.PointsToShow = this.PointsToShow;
            ctrl.DrawMode = this.DrawMode;
            ctrl.TailSpanWidth = this.TailSpanWidth;
            //ctrl.ShowAllPoints = this.ShowAllPoints;
            ctrl.MaxFps = this.MaxFps;
            ctrl.DoPopOut = false;

            wnd.Content = ctrl;
            this.DoNotUpdate = true;
            wnd.ShowDialog();
            ctrl.DoNotUpdate = true;
            ctrl.Source = null;

            this.DoNotUpdate = false;
        }

        private RenderTargetBitmap CanvasRender { get; set; }


    }
}

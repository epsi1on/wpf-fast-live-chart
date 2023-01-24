using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfFastCharting.Lib;

namespace WpfFastCharting.Examples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
			this.DataContext = this.Context = new DContext();

			new Thread(Context.StartGenratingData).Start();
        }

		DContext Context;
        public class DContext:INotifyPropertyChanged
        {
			#region INotifyPropertyChanged members and helpers

			public event PropertyChangedEventHandler PropertyChanged;

			protected static bool AreEqualObjects(object obj1, object obj2)
			{
				var obj1Null = ReferenceEquals(obj1, null);
				var obj2Null = ReferenceEquals(obj2, null);

				if (obj1Null && obj2Null)
					return true;

				if (obj1Null || obj2Null)
					return false;

				if (obj1.GetType() != obj2.GetType())
					return false;

				if (ReferenceEquals(obj1, obj2))
					return true;

				return obj1.Equals(obj2);
			}

			protected void OnPropertyChanged(params string[] propertyNames)
			{
				if (propertyNames == null)
					return;

				if (this.PropertyChanged != null)
					foreach (var propertyName in propertyNames)
						this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}

			#endregion

			private ObservableCollection<Tuple<double, double>> source = new ObservableCollection<Tuple<double, double>>();

            public ObservableCollection<Tuple<double, double>> Source
            {
                get { return source; }
                set
                {
                    source = value;

					if (PropertyChanged != null)
						PropertyChanged(this, new PropertyChangedEventArgs(nameof(Source)));
                }
            }


			public void StartGenratingData()
			{
				var rnd = new Random();

				double x = 0;

				while (true)
				{
					Thread.Sleep(10);

					x += 1;
					var y = 0.5 - rnd.NextDouble();

					var tpl = Tuple.Create(x, y);

					source.Add(tpl);
				}
			}
        }
    }


    public class TuplePointMapper : IChartPointMapper
    {
		public void Map(object obj, out double x, out double y)
		{
			var tp = obj as Tuple<double, double>;

			x = tp.Item1;
			y = tp.Item2;
		}
    }


}

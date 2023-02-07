This project is intended to render line charts for WPF with high performance.
Other features like zoom, pan, graphics, mouse hit etc. may not be quite good, but performance is pretty high regard to some other controls because this code uses WriteableBitmap object.
Mainly use for live chart with high input rate.

Perofmance is more than 30FPS for data with hundreds points

![Screen Shot](screenshot.png?raw=true "Title")

# Simple Usage

simply set two properties:

```
var objects = new ObservableCollection<double[]>();

chart1.Source = objects;
chart1.Mapper = new ArrayMapper;


public class ArrayMapper:IChartPointMapper
{
  public void Map(object obj, out double x, out double y)
  {
    var arr = obj as double[];

    x = arr[0];
    y = arr[1];
  }
}
```
From now wherever any object added to `objects`, control will be notified and redraws the chart. no need for manual update.

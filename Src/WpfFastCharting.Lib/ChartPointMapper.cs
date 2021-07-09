namespace WpfFastCharting.Lib
{
    public interface IChartPointMapper
    {
        void Map(object obj, out double x, out double y);
    }
}

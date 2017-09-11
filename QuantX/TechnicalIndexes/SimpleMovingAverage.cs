using System.Linq;
using QuantX.Extension;

namespace QuantX.TechnicalIndexes
{
  public class SimpleMovingAverage : TechnicalIndex<double>
  {
    public int Period { get; private set; }
    public SimpleMovingAverage(TechnicalIndex<double> source, int period)
    {
      Period = period;
      Bind(source);
    }
    protected override void Main()
    {
      var src = Source as TechnicalIndex<double>;
      History.EnsureCount(src.Count);
      History[src.Count - 1] = src.Last(Period).Average();
      OnUpdate();
    }
  }
}
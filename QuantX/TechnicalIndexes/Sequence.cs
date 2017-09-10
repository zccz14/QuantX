namespace QuantX.TechnicalIndexes
{
  public class Sequence<T>: TechnicalIndex<T> {
    public void Add(T item)
    {
      History.Add(item);
      OnUpdate();
    }
  }
}
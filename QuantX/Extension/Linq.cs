using System.Collections.Generic;

namespace QuantX.Extension
{
  public static class LinqExtension
  {
    /// <summary>
    /// [Lazy] Read Last N Elements
    /// </summary>
    public static IEnumerable<T> Last<T> (this IReadOnlyList<T> list, int n) {
      for (int i = list.Count - 1; i >= 0 && n > 0; i--, n--) {
        yield return list[i];
      }
    }
  }
}
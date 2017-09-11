using System.Collections.Generic;
using System.Linq;

namespace QuantX.Extension
{
  public static class ListExtension
  {
    /// <summary>
    /// Ensure and Fill the list by last or default
    /// </summary>
    public static void EnsureCount<T> (this IList<T> list, int n) {
      while (list.Count < n)
      {
        list.Add(list.LastOrDefault());
      }
    }
  }
}
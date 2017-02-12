using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX {
    /// <summary>
    /// 拓展方法类
    /// </summary>
    public static partial class Extension {
        /// <summary>
        /// 惰性地取只读列表 list 中的最后 n 个
        /// <para>实际上有可能不到 n 个，会返回不到 n 个元素的枚举器</para>
        /// <para>返回的枚举顺序是逆序的</para>
        /// </summary>
        /// <remarks>这个方法在量化系统中特别常用</remarks>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="list">只读列表</param>
        /// <param name="n">元素数量</param>
        /// <param name="skip">跳过元素数量</param>
        /// <returns>最后 n 个元素的逆序枚举器（可能为空）</returns>
        public static IEnumerable<T> Last<T> (this IReadOnlyList<T> list, int n, int skip = 0) {
            for (int i = list.Count - 1 - skip; i >= 0 && n > 0; i--, n--) {
                yield return list[i];
            }
        }
        /// <summary>
        /// 惰性逆向获取只读列表。
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="list">只读列表</param>
        /// <returns>枚举器</returns>
        public static IEnumerable<T> Reverse<T> (this IReadOnlyList<T> list) {
            for (int i = list.Count - 1; i >= 0; i--) {
                yield return list[i];
            }
        }
        /// <summary>
        /// 惰性获取左开右闭区间
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">只读列表</param>
        /// <param name="left">左端点</param>
        /// <param name="right">右端点</param>
        /// <returns>枚举器</returns>
        public static IEnumerable<T> Range<T> (this IReadOnlyList<T> list, int left, int right) {
            for (int i = left; i < right; i++) {
                yield return list[i];
            }
        }
        /// <summary>
        /// 枚举闭区间[idxBegin ... idxEnd]
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="list">只读列表</param>
        /// <param name="idxBegin">起始索引</param>
        /// <param name="idxEnd">终止索引</param>
        /// <returns>枚举器</returns>
        public static IEnumerable<T> RangeByIndex<T> (this IReadOnlyList<T> list, int idxBegin, int idxEnd) {
            for (int i = idxBegin; i <= idxEnd; i++) {
                yield return list[i];
            }
        }
        /// <summary>
        /// 从 index 开始往回取元素
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="list">只读列表</param>
        /// <param name="index">索引号</param>
        /// <returns>枚举器</returns>
        public static IEnumerable<T> ReverseAt<T> (this IReadOnlyList<T> list, int index) {
            for (int i = Math.Min(index, list.Count - 1); i >= 0; i--) {
                yield return list[i];
            }
        }
        /// <summary>
        /// 跳过只读列表中指定数量的元素，然后返回剩下的元素
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="list">只读列表</param>
        /// <param name="count">数量</param>
        /// <returns>枚举器</returns>
        public static IEnumerable<T> Skip<T> (this IReadOnlyList<T> list, int count) {
            for (int i = Math.Max(count, 0); i < list.Count; i++) {
                yield return list[i];
            }
        }
        /// <summary>
        /// 判断初始值-中间值-最终值是否回调达到一个比例
        /// </summary>
        /// <param name="start">初始值</param>
        /// <param name="middle">中间值</param>
        /// <param name="end">最终值</param>
        /// <param name="ratio">比例</param>
        /// <returns>逻辑值</returns>
        public static bool isBackAdjusted(double start, double middle, double end, double ratio) {
            return (middle - end) / (middle - start) >= ratio;
        }
        /// <summary>
        /// 变化率 DiffRatio = (to - from) / from
        /// </summary>
        /// <param name="from">原始值</param>
        /// <param name="to">当前值</param>
        /// <returns>变化率</returns>
        public static double DiffRatio(double from, double to) {
            return (to - from) / from;
        }
    }
}

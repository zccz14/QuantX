﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX {
    /// <summary>
    /// 拓展方法类
    /// </summary>
    public static class Extension {
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
    }
}

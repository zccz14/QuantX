using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX.TI {
    /// <summary>
    /// 序列指标类
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public class Sequence<T> : ITI<T> {
        /// <summary>
        /// 添加一个元素
        /// </summary>
        /// <param name="item">元素</param>
        public void Add(T item) {
            bufHistory.Add(item);
            OnData?.Invoke(this, bufHistory.Last());
        }
        /// <summary>
        /// 历史数据
        /// </summary>
        public IReadOnlyList<T> History {
            get {
                return bufHistory;
            }
        }
        /// <summary>
        /// 数据事件
        /// </summary>
        public event EventHandler<T> OnData;
        private List<T> bufHistory = new List<T>();
    }
}

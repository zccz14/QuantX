using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX.TI {
    /// <summary>
    /// 收盘价指标
    /// </summary>
    public class ClosePrice : ITI<double> {
        /// <summary>
        /// 根据 BarType 类型的指标取收盘价
        /// </summary>
        /// <param name="ti">价格技术指标</param>
        public ClosePrice (ITI<EQuant.BarType> ti) {
            ti.OnData += main;
        }

        private void main (object sender, EQuant.BarType e) {
            bufHistory.Add(e.ClosePrice);
            OnData?.Invoke(this, bufHistory.Last());
        }
        /// <summary>
        /// 历史数据
        /// </summary>
        public IReadOnlyList<double> History {
            get {
                return bufHistory;
            }
        }
        /// <summary>
        /// 数据事件
        /// </summary>
        public event EventHandler<double> OnData;
        private List<double> bufHistory = new List<double>();
    }
    /// <summary>
    /// 开盘价指标
    /// </summary>
    public class OpenPrice : ITI<double> {
        /// <summary>
        /// 根据 BarType 类型的指标取开盘价
        /// </summary>
        /// <param name="ti">价格技术指标</param>
        public OpenPrice (ITI<EQuant.BarType> ti) {
            ti.OnData += main;
        }

        private void main (object sender, EQuant.BarType e) {
            bufHistory.Add(e.OpenPrice);
            OnData?.Invoke(this, bufHistory.Last());
        }
        /// <summary>
        /// 历史数据
        /// </summary>
        public IReadOnlyList<double> History {
            get {
                return bufHistory;
            }
        }
        /// <summary>
        /// 数据事件
        /// </summary>
        public event EventHandler<double> OnData;
        private List<double> bufHistory = new List<double>();
    }
}

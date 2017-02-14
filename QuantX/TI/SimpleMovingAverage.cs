using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX.TI {
    /// <summary>
    /// 简单移动平均线
    /// </summary>
    public class SimpleMovingAverage : ITI<double> {
        /// <summary>
        /// 基于 TI 上的 period 周期的简单移动平均线
        /// </summary>
        /// <param name="TI">基础指标</param>
        /// <param name="period">周期</param>
        public SimpleMovingAverage (ITI<double> TI, int period) {
            _TI = TI;
            _period = period;
            TI.OnData += main;
        }

        private void main (object sender, double e) {
            int cc = _TI.History.Count;
            for (int i = bufHistory.Count; i < cc; i++) {
                bufHistory.Add(_TI.History.Last(_period).Average());
                OnData?.Invoke(this, bufHistory.Last());
            }
        }

        /// <summary>
        /// 获取SMA的实例
        /// </summary>
        /// <remarks>运用单例模式减少重复构造的开销</remarks>
        /// <param name="TI">基础指标</param>
        /// <param name="period">周期</param>
        /// <returns>SMA 实例</returns>
        public static SimpleMovingAverage GetInstance (ITI<double> TI, int period) {
            foreach (var x in _instances) {
                if (TI.Equals(x._TI) && period.Equals(x._period)) {
                    return x;
                }
            }
            var res = new SimpleMovingAverage(TI, period);
            _instances.Add(res);
            return res;
        }
        private static HashSet<SimpleMovingAverage> _instances = new HashSet<SimpleMovingAverage>();
        /// <summary>
        /// 实例池
        /// </summary>
        public static IEnumerable<SimpleMovingAverage> Instances {
            get { return _instances; }
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
        /// 隐式转换：取当前的指标值
        /// </summary>
        /// <param name="TI">指标</param>
        public static implicit operator double(SimpleMovingAverage TI) {
            return TI.History.LastOrDefault();
        }

        /// <summary>
        /// 输出均线数据的事件
        /// </summary>
        public event EventHandler<double> OnData;
        private List<double> bufHistory = new List<double>();
        private ITI<double> _TI;
        private int _period;
        /// <summary>
        /// 转换成字符串
        /// </summary>
        /// <returns>转换成字符串</returns>
        public override string ToString () {
            return string.Format("{0}.SMA({1})", _TI, _period);
        }
    }
}

namespace QuantX {
    using TI;
    public static partial class Extension {
        /// <summary>
        /// 获取指标的 SMA
        /// </summary>
        /// <param name="TI">基础指标</param>
        /// <param name="period">周期</param>
        /// <returns>SMA 实例</returns>
        public static SimpleMovingAverage SMA (this ITI<double> TI, int period) {
            return SimpleMovingAverage.GetInstance(TI, period);
        }
    }
}
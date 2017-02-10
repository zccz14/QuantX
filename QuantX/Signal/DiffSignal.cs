using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX.Signal {
    /// <summary>
    /// 当前值与向前引用的若干值的差是否达到一定绝对值
    /// </summary>
    public class DiffSignal : ISignal {
        /// <summary>
        /// 当前值与 period 历史值的差达到 diff 时出信号
        /// </summary>
        /// <param name="Source">源数据</param>
        /// <param name="period">周期</param>
        /// <param name="diff">差值</param>
        public DiffSignal (ITI<double> Source, int period, double diff) {
            this.Source = Source;
            this.period = period;
            this.diff = diff;
            _lastCount = 0;
            this.Source.OnData += main;
        }
        /// <summary>
        /// 历史数据
        /// </summary>
        public IReadOnlyList<bool> History {
            get {
                return bufHistory;
            }
        }
        /// <summary>
        /// 失效事件
        /// </summary>
        public event EventHandler OnFalse;
        /// <summary>
        /// 数据事件
        /// </summary>
        public event EventHandler<bool> OnData;
        /// <summary>
        /// 有效事件
        /// </summary>
        public event EventHandler OnTrue;

        private void main (object sender, double data) {
            int count = Math.Min(period, bufHistory.Count + 1 - _lastCount);
            double high = Source.History.Last(count).Max();
            double low = Source.History.Last(count).Min();
            bufHistory.Add(high - data > diff || data - low > diff);
            if (bufHistory.Last()) {
                _lastCount = bufHistory.Count;
                OnTrue?.Invoke(this, null);
            } else {
                OnFalse?.Invoke(this, null);
            }
            OnData?.Invoke(this, bufHistory.Last());
        }
        private List<bool> bufHistory = new List<bool>();
        private int period;
        private double diff;
        private int _lastCount;
        private ITI<double> Source;
        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="Source">源数据</param>
        /// <param name="period">周期</param>
        /// <param name="diff">差量阈值</param>
        /// <returns>实例</returns>
        public static DiffSignal GetInstance(ITI<double> Source, int period, double diff) {
            foreach (var x in _instances) {
                if (Source.Equals(x.Source) && period.Equals(x.period) && diff.Equals(x.diff)) {
                    return x;
                }
            }
            var ins = new DiffSignal(Source, period, diff);
            _instances.Add(ins);
            return ins;
        }
        private static HashSet<DiffSignal> _instances = new HashSet<DiffSignal>();
    }
}

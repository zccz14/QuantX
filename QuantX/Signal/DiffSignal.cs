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
        /// <summary>
        /// 详细数据
        /// </summary>
        public class EventArgs : System.EventArgs {
            /// <summary>
            /// 最大位置
            /// </summary>
            public int idxMax;
            /// <summary>
            /// 最小位置
            /// </summary>
            public int idxMin;
            /// <summary>
            /// 判定结果
            /// </summary>
            public bool result;
        }
        private void main (object sender, double data) {
            int idxMax = _lastCount, idxMin = _lastCount;
            #region 查找 [_lastCount, Count) 范围内的最值点
            for (int i = _lastCount; i < Source.History.Count; i++) {
                if (Source.History[i] > Source.History[idxMax]) {
                    idxMax = i;
                }
                if (Source.History[i] < Source.History[idxMin]) {
                    idxMin = i;
                }
            }
            #endregion
            double high = Source.History[idxMax];
            double low = Source.History[idxMin];
            bool result = high - data > diff || data - low > diff;
            bufHistory.Add(result);
            EventArgs arg = new EventArgs();
            arg.idxMax = idxMax;
            arg.idxMin = idxMin;
            arg.result = result;
            if (bufHistory.Last()) {
                _lastCount = bufHistory.Count;
                OnTrue?.Invoke(this, arg);
            } else {
                OnFalse?.Invoke(this, arg);
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX.Signal {
    /// <summary>
    /// 延迟信号类
    /// </summary>
    public class DelaySignal : ISignal {
        /// <summary>
        /// 延迟信号到满足条件时再发出
        /// </summary>
        /// <param name="Clock">时钟信号</param>
        /// <param name="Condition">条件</param>
        public DelaySignal(ISignal Clock, Func<bool> Condition) {
            this.Clock = Clock;
            this.Condition = Condition;
            Clock.OnData += main;
        }

        private void main (object sender, bool e) {
            bufHistory.Add(Work(e));
            if (bufHistory.Last()) {
                OnTrue?.Invoke(this, null);
            } else {
                OnFalse?.Invoke(this, null);
            }
            OnData?.Invoke(this, bufHistory.Last());
        }

        private bool Work (bool e) {
            if (e) {
                _active = true;
            } else {
                if (_active) {
                    if (Condition()) {
                        _active = false;
                        return true;
                    }
                }
            }
            return false;
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
        /// 数据事件
        /// </summary>
        public event EventHandler<bool> OnData;
        /// <summary>
        /// 失效事件
        /// </summary>
        public event EventHandler OnFalse;
        /// <summary>
        /// 生效事件
        /// </summary>
        public event EventHandler OnTrue;
        private List<bool> bufHistory = new List<bool>();
        private ISignal Clock;
        private Func<bool> Condition;
        private bool _active = false;
    }
}

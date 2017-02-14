using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantX.TI;
namespace QuantX.Signal {
    /// <summary>
    /// Boll 通道开口信号
    /// </summary>
    public class BollOpenSignal : ISignal {
        /// <summary>
        /// Boll 开口信号 
        /// </summary>
        /// <param name="Boll">Boll指标</param>
        /// <param name="percentage">开口/收口百分比</param>
        public BollOpenSignal (Boll Boll, double percentage) {
            _Boll = Boll;
            _per = percentage;
            _Boll.OnData += main;
        }
        /// <summary>
        /// 获取 Boll 通道开口信号的实例
        /// </summary>
        /// <param name="Boll">Boll指标</param>
        /// <param name="percentage">开口/收口百分比</param>
        /// <returns>信号实例</returns>
        public static BollOpenSignal GetInstance(Boll Boll, double percentage) {
            foreach (var x in _instances) {
                if (Boll.Equals(x._Boll) && percentage.Equals(x._per)) {
                    return x;
                }
            }
            var ins = new BollOpenSignal(Boll, percentage);
            _instances.Add(ins);
            return ins;
        }

        private void main (object sender, BollData e) {
            if (bufHistory.LastOrDefault()) {
                #region 现处于开口状态
                #region 更新最大值
                if (e.Width > maxWidth) {
                    maxWidth = e.Width;
                    idxMax = _Boll.History.Count;
                }
                #endregion
                if (e.Width < maxWidth * (1 - _per)) {
                    idxMin = idxClose = _Boll.History.Count;
                    maxWidth = e.Width;
                    bufHistory.Add(false);
                } else {
                    bufHistory.Add(true);
                }
                #endregion
            } else {
                #region 现处于收口状态
                #region 更新最小位置
                if (e.Width < minWidth) {
                    minWidth = e.Width;
                    idxMin = _Boll.History.Count;
                }
                #endregion
                #region 开口条件：比最小位置扩大 _per 倍
                if (e.Width > minWidth * (1 + _per)) {
                    idxMax = idxOpen = _Boll.History.Count; // 开口
                    maxWidth = e.Width;
                    bufHistory.Add(true);
                } else {
                    bufHistory.Add(false);
                }
                #endregion
                #endregion
            }
            if (bufHistory.Last()) {
                OnTrue?.Invoke(this, null);
            } else {
                OnFalse?.Invoke(this, null);
            }
            OnData?.Invoke(this, bufHistory.Last());
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
        private Boll _Boll;
        private double _per;

        private int idxOpen = 0;
        private int idxClose = 0;
        private double minWidth = Double.PositiveInfinity;
        private double maxWidth = 0;
        private int idxMin = 0;
        private int idxMax = 0;

        private static HashSet<BollOpenSignal> _instances = new HashSet<BollOpenSignal>();
    }
}

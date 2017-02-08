using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX.TI {
    /// <summary>
    /// 指数移动平均指标类
    /// </summary>
    public class ExponentialMovingAverage : ITI<double> {
        /// <summary>
        /// 基于 TI，构造周期为 N 的指数移动平均指标
        /// </summary>
        /// <param name="TI">基础指标</param>
        /// <param name="N">周期</param>
        public ExponentialMovingAverage(ITI<double> TI, int N) {
            _TI = TI;
            _N = N;
            _TI.OnData += main;
        }

        private void main (object sender, double e) {
            double a = 2.0 / (_N + 1);
            bufHistory.Add(e * a + bufHistory.LastOrDefault() * (1 - a));
            OnData?.Invoke(this, bufHistory.Last());
        }

        /// <summary>
        /// 获取 EMA 实例
        /// </summary>
        /// <remarks>运用单例模式复用指标</remarks>
        /// <param name="TI">基础指标</param>
        /// <param name="N">周期</param>
        /// <returns>EMA 实例</returns>
        public static ExponentialMovingAverage GetInstance(ITI<double> TI, int N) {
            foreach (var x in _instances) {
                if (TI.Equals(x._TI) && N.Equals(x._N)) {
                    return x;
                }
            }
            var res = new ExponentialMovingAverage(TI, N);
            _instances.Add(res);
            return res;
        }
        private static HashSet<ExponentialMovingAverage> _instances = new HashSet<ExponentialMovingAverage>();
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
        private ITI<double> _TI;
        private int _N;
    }
}

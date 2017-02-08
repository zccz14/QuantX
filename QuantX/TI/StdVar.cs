using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX.TI {
    /// <summary>
    /// 标准差指标类
    /// </summary>
    public class StdVar : ITI<double> {
        /// <summary>
        /// 基于 TI 指标，构造周期为 N 的标准差
        /// </summary>
        /// <param name="TI">基础指标</param>
        /// <param name="N">周期</param>
        public StdVar(ITI<double> TI, int N) {
            _TI = TI;
            _N = N;
            _MA = SimpleMovingAverage.GetInstance(TI, N);
            _TI.OnData += main;
            _MA.OnData += main;
        }

        private void main (object sender, double e) {
            int c = bufHistory.Count;
            int c1 = _TI.History.Count;
            int c2 = _MA.History.Count;
            if (c == c1 || c == c2) {
                return;
            }
            int cc = Math.Min(c1, c2);
            for (int i = c; i < cc; i++) {
                var avg = _MA.History[i];
                var stdVar = Math.Sqrt(_TI.History.Last(_N, cc - 1 - i).Aggregate(0.0, (prev, next) => prev + Math.Pow(next - avg, 2)));
                bufHistory.Add(stdVar);
                OnData?.Invoke(this, bufHistory.Last());
            }
        }
        /// <summary>
        /// 获取 StdVar 实例
        /// </summary>
        /// <param name="TI">基础指标</param>
        /// <param name="N">周期</param>
        /// <returns></returns>
        public static StdVar GetInstance(ITI<double> TI, int N) {
            foreach (var x in _instances) {
                if (TI.Equals(x._TI) && N.Equals(x._N)) {
                    return x;
                }
            }
            var ret = new StdVar(TI, N);
            _instances.Add(ret);
            return ret;
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
        private int _N;
        private ITI<double> _TI;
        private SimpleMovingAverage _MA;
        private static HashSet<StdVar> _instances = new HashSet<StdVar>();
    }
}

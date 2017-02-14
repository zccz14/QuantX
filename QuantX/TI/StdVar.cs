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
            _MA = SMA.GetInstance(TI, N);
            //_TI.OnData += main;
            _MA.OnData += main;
        }

        private void main (object sender, double e) {
            int cc = Math.Min(_TI.History.Count, _MA.History.Count);
            for (int i = bufHistory.Count; i < cc; i++) {
                var avg = _MA.History[i];
                var stdVar = Math.Sqrt(
                    _TI.History
                    .ReverseAt(i)
                    .Take(_N)
                    .Select(v => v - avg)
                    .Sum(v => v * v) / (_N - 1));
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
        private SMA _MA;
        private static HashSet<StdVar> _instances = new HashSet<StdVar>();
    }
}
namespace QuantX {
    public static partial class Extension {
        /// <summary>
        /// 获取指标的标准差指标
        /// </summary>
        /// <param name="ti">指标</param>
        /// <param name="period">周期</param>
        /// <returns>标准差指标</returns>
        public static TI.StdVar StdVar(this ITI<double> ti, int period) {
            return TI.StdVar.GetInstance(ti, period);
        }
    }
}
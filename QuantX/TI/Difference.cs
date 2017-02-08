using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX.TI {
    /// <summary>
    /// 差量指标：两个数值指标的差
    /// </summary>
    public class Difference : ITI<double> {
        /// <summary>
        /// 基于 TI1、TI2，构造差量指标 TI1 - TI2
        /// </summary>
        /// <param name="TI1">被减数指标</param>
        /// <param name="TI2">减数指标</param>
        public Difference(ITI<double> TI1, ITI<double> TI2) {
            _TI1 = TI1;
            _TI2 = TI2;
            _TI1.OnData += main;
            _TI2.OnData += main;
        }

        private void main (object sender, double e) {
            int c = bufHistory.Count;
            var cc = (new int[2] { _TI1.History.Count, _TI2.History.Count }).Min();
            for (int i = c; i < cc; i++) {
                bufHistory.Add(_TI1.History[i] - _TI2.History[i]);
                OnData?.Invoke(this, bufHistory.Last());
            }
        }
        /// <summary>
        /// 获取 Diff 实例
        /// </summary>
        /// <remarks>运用单例模式复用实例</remarks>
        /// <param name="TI1">被减数指标</param>
        /// <param name="TI2">减数指标</param>
        /// <returns>Diff 实例</returns>
        public static Difference GetInstance(ITI<double> TI1, ITI<double> TI2) {
            foreach (var x in _instances) {
                if (TI1.Equals(x._TI1) && TI2.Equals(x._TI2)) {
                    return x;
                }
            }
            var ret = new Difference(TI1, TI2);
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
        private ITI<double> _TI1;
        private ITI<double> _TI2;
        private static HashSet<Difference> _instances = new HashSet<Difference>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX.TI {
    /// <summary>
    /// PC 指标
    /// </summary>
    public class PC : ITI<double> {
        /// <summary>
        /// 基于 Price, 波动率 Vol，计算流通盘为 Circulation 的 PC 指标
        /// </summary>
        /// <param name="Price">价格指标</param>
        /// <param name="Vol">波动率</param>
        /// <param name="Circulation">流通盘大小</param>
        public PC(ITI<double> Price, Volatility Vol, double Circulation) {
            _Price = Price;
            _Vol = Vol;
            _Circulation = Circulation;
            _Price.OnData += main;
            _Vol.OnData += main;
        }
        /// <summary>
        /// 获取 PC 实例
        /// </summary>
        /// <param name="Price">价格指标</param>
        /// <param name="Vol">波动率</param>
        /// <param name="Circulation">流通盘大小</param>
        /// <returns></returns>
        public static PC GetInstance(ITI<double> Price, Volatility Vol, double Circulation) {
            foreach (var x in _instances) {
                if (Price.Equals(x._Price) && Vol.Equals(x._Vol) && Circulation.Equals(x._Circulation)) {
                    return x;
                }
            }
            var ret = new PC(Price, Vol, Circulation);
            _instances.Add(ret);
            return ret;
        }

        private void main (object sender, double e) {
            int cc = Math.Min(_Price.History.Count, _Vol.History.Count);
            for (int i = bufHistory.Count; i < cc; i++) {
                bufHistory.Add(CalcOnIndex(i));
                OnData?.Invoke(this, bufHistory.Last());
            }
        }

        private double CalcOnIndex (int i) {
            double vol = _Vol.History[i];
            double price = _Price.History[i];
            if (vol == 0) {
                return 0;
            }
            double turnover = vol / _Circulation;
            foreach (double x in bufPriceToVol.Keys.ToList()) {
                bufPriceToVol[x] *= 1 - turnover;
                if (bufPriceToVol[x] == 0) {
                    bufPriceToVol.Remove(x);
                }
            }
            if (bufPriceToVol.ContainsKey(price)) {
                bufPriceToVol[price] += vol;
            } else {
                bufPriceToVol[price] = vol;
            }
            double win = bufPriceToVol.Where(v => v.Key < price).Select(v => v.Value).Sum();
            double total = bufPriceToVol.Select(v => v.Value).Sum();
            double nextWinRatio = total > 0 ? win / total : 0;
            double ret = (nextWinRatio - lastWinRatio) / turnover;
            lastWinRatio = nextWinRatio;
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
        private Dictionary<double, double> bufPriceToVol = new Dictionary<double, double>();
        private ITI<double> _Price;
        private Volatility _Vol;
        private double _Circulation;
        private double lastWinRatio;
        private static HashSet<PC> _instances = new HashSet<PC>();
    }
}
namespace QuantX {
    public static partial class Extension {
        /// <summary>
        /// 获取 PC 实例
        /// </summary>
        /// <param name="Price">价格指标</param>
        /// <param name="Vol">波动率</param>
        /// <param name="Circulation">流通盘</param>
        /// <returns>PC实例</returns>
        public static TI.PC PC(this ITI<double> Price, TI.Volatility Vol, double Circulation) {
            return TI.PC.GetInstance(Price, Vol, Circulation);
        }
    }
}
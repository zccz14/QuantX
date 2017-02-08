using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX.TI {
    /// <summary>
    /// 波动率指标
    /// </summary>
    public class Volatility : ITI<double> {
        /// <summary>
        /// 基于 Open、Close，计算周期为 period 的波动率
        /// </summary>
        /// <remarks>输出周期为基础指标的 period 倍</remarks>
        /// <param name="Open">开盘价</param>
        /// <param name="Close">收盘价</param>
        /// <param name="period">周期</param>
        public Volatility(ITI<double> Open, ITI<double> Close, int period) {
            _Open = Open;
            _Close = Close;
            _period = period;
            _LogROI = LogROI.GetInstance(Open, Close);
            _StdVarLogROI = StdVar.GetInstance(_LogROI, _period);
            //_Open.OnData += main;
            //_Close.OnData += main;
            //_LogROI.OnData += main;
            _StdVarLogROI.OnData += main;
        }

        private void main (object sender, double e) {
            if (_LogROI.History.Count % _period == 0) {
                bufHistory.Add(_StdVarLogROI.History.Last() * Math.Sqrt(_period));
                OnData?.Invoke(this, bufHistory.Last());
            }
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
        private LogROI _LogROI;
        private ITI<double> _Open;
        private ITI<double> _Close;
        private int _period;
        private StdVar _StdVarLogROI;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX.TI {
    /// <summary>
    /// ROI 指标: Return On Investment 投资回报率
    /// </summary>
    public class ROI : ITI<double> {
        /// <summary>
        /// ROI = (Close - Open) / Open
        /// </summary>
        /// <param name="Open">购入（开盘）价</param>
        /// <param name="Close">售出（收盘）价</param>
        public ROI(ITI<double> Open, ITI<double> Close) {
            _Open = Open;
            _Close = Close;
            _Open.OnData += main;
            _Close.OnData += main;
        }

        private void main (object sender, double e) {
            int cc = Math.Min(_Open.History.Count, _Close.History.Count);
            for (int i = bufHistory.Count; i < cc; i++) {
                bufHistory.Add((_Close.History[i] - _Open.History[i]) / _Open.History[i]);
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
        /// <summary>
        /// 获取 ROI 实例
        /// </summary>
        /// <param name="Open">购入（开盘）价</param>
        /// <param name="Close">售出（收盘）价</param>
        /// <returns>ROI 实例</returns>
        public static ROI GetInstance(ITI<double> Open, ITI<double> Close) {
            foreach (var x in _instances) {
                if (Open.Equals(x._Open) && Close.Equals(x._Close)) {
                    return x;
                }
            }
            var ins = new ROI(Open, Close);
            _instances.Add(ins);
            return ins;
        }
        private List<double> bufHistory = new List<double>();
        private ITI<double> _Open;
        private ITI<double> _Close;
        private static HashSet<ROI> _instances = new HashSet<ROI>();
    }
    /// <summary>
    /// Log ROI 指标: Log Return On Investment 对数投资回报率
    /// </summary>
    public class LogROI : ITI<double> {
        /// <summary>
        /// ROI = (Close - Open) / Open
        /// </summary>
        /// <param name="Open">购入（开盘）价</param>
        /// <param name="Close">售出（收盘）价</param>
        public LogROI (ITI<double> Open, ITI<double> Close) {
            _Open = Open;
            _Close = Close;
            _Open.OnData += main;
            _Close.OnData += main;
        }

        private void main (object sender, double e) {
            int cc = Math.Min(_Open.History.Count, _Close.History.Count);
            for (int i = bufHistory.Count; i < cc; i++) {
                bufHistory.Add(Math.Log(_Close.History[i]) - Math.Log(_Open.History[i]));
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
        /// <summary>
        /// 获取 LogROI 实例
        /// </summary>
        /// <param name="Open">购入（开盘）价</param>
        /// <param name="Close">售出（收盘）价</param>
        /// <returns>LogROI 实例</returns>
        public static LogROI GetInstance (ITI<double> Open, ITI<double> Close) {
            foreach (var x in _instances) {
                if (Open.Equals(x._Open) && Close.Equals(x._Close)) {
                    return x;
                }
            }
            var ins = new LogROI(Open, Close);
            _instances.Add(ins);
            return ins;
        }
        private List<double> bufHistory = new List<double>();
        private ITI<double> _Open;
        private ITI<double> _Close;
        private static HashSet<LogROI> _instances = new HashSet<LogROI>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX.TI {
    /// <summary>
    /// 收盘价指标
    /// </summary>
    public class ClosePrice : ITI<double> {
        /// <summary>
        /// 根据 BarType 类型的指标取收盘价
        /// </summary>
        /// <param name="Bar">价格技术指标</param>
        public ClosePrice (ITI<EQuant.BarType> Bar) {
            this.Bar = Bar;
            this.Bar.OnData += main;
        }

        private void main (object sender, EQuant.BarType e) {
            bufHistory.Add(e.ClosePrice);
            OnData?.Invoke(this, bufHistory.Last());
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
        private ITI<EQuant.BarType> Bar;
        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="Bar">BarType 指标</param>
        /// <returns>实例</returns>
        public static ClosePrice GetInstance(ITI<EQuant.BarType> Bar) {
            foreach (var x in _instances) {
                if (Bar.Equals(x.Bar)) {
                    return x;
                }
            }
            var ins = new ClosePrice(Bar);
            _instances.Add(ins);
            return ins;
        }
        /// <summary>
        /// 实例池
        /// </summary>
        public static IEnumerable<ClosePrice> Instances { get { return _instances; } }
        private static HashSet<ClosePrice> _instances = new HashSet<ClosePrice>();
    }
    /// <summary>
    /// 开盘价指标
    /// </summary>
    public class OpenPrice : ITI<double> {
        /// <summary>
        /// 根据 BarType 类型的指标取开盘价
        /// </summary>
        /// <param name="Bar">价格技术指标</param>
        public OpenPrice (ITI<EQuant.BarType> Bar) {
            this.Bar = Bar;
            this.Bar.OnData += main;
        }

        private void main (object sender, EQuant.BarType e) {
            bufHistory.Add(e.OpenPrice);
            OnData?.Invoke(this, bufHistory.Last());
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
        private ITI<EQuant.BarType> Bar;
        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="Bar">BarType 指标</param>
        /// <returns>实例</returns>
        public static OpenPrice GetInstance (ITI<EQuant.BarType> Bar) {
            foreach (var x in _instances) {
                if (Bar.Equals(x.Bar)) {
                    return x;
                }
            }
            var ins = new OpenPrice(Bar);
            _instances.Add(ins);
            return ins;
        }
        /// <summary>
        /// 实例池
        /// </summary>
        public static IEnumerable<OpenPrice> Instances { get { return _instances; } }
        private static HashSet<OpenPrice> _instances = new HashSet<OpenPrice>();
    }
}

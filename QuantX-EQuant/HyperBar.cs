using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX.TI {
    /// <summary>
    /// 超级 Bar 类
    /// </summary>
    public class HyperBar : ITI<EQuant.BarType> {
        /// <summary>
        /// 合并 BarType 到更高周期的 BarType
        /// </summary>
        /// <param name="BaseBar">基础</param>
        /// <param name="cnt">合并周期</param>
        public HyperBar (ITI<EQuant.BarType> BaseBar, int cnt) {
            _ = this.cnt = cnt;
            this.BaseBar = BaseBar;
            this.BaseBar.OnData += main;
        }
        /// <summary>
        /// 历史数据
        /// </summary>
        public IReadOnlyList<EQuant.BarType> History {
            get {
                return bufHistory;
            }
        }
        /// <summary>
        /// 数据事件
        /// </summary>
        public event EventHandler<EQuant.BarType> OnData;
        private void main (object sender, EQuant.BarType data) {
            if (_ == cnt) {
                bar = new EQuant.BarType(data);
            } else {
                bar.MaxPrice = Math.Max(bar.MaxPrice, data.MaxPrice);
                bar.MinPrice = Math.Min(bar.MinPrice, data.MinPrice);
                bar.Volume += data.Volume;
            }
            _--;
            if (_ == 0) {
                _ = cnt;
                bar.ClosePrice = data.ClosePrice;
                bar.OpenInterest = data.OpenInterest;
                bufHistory.Add(bar);
                OnData?.Invoke(this, bufHistory.Last());
            }
        }
        private List<EQuant.BarType> bufHistory = new List<EQuant.BarType>();
        private int _, cnt;
        private EQuant.BarType bar;
        /// <summary>
        /// 获取 HyperBar 实例
        /// </summary>
        /// <param name="BaseBar">BaseBar</param>
        /// <param name="cnt">提升周期</param>
        /// <returns>实例</returns>
        public static HyperBar GetInstance(ITI<EQuant.BarType> BaseBar, int cnt) {
            foreach (var x in _instances) {
                if (BaseBar.Equals(x.BaseBar) && cnt.Equals(x.cnt)) {
                    return x;
                }
            }
            var ins = new HyperBar(BaseBar, cnt);
            _instances.Add(ins);
            return ins;
        }
        /// <summary>
        /// 实例池
        /// </summary>
        public static IEnumerable<HyperBar> Instances { get { return _instances; } }
        private static HashSet<HyperBar> _instances = new HashSet<HyperBar>();
        private ITI<EQuant.BarType> BaseBar;
    }
}
namespace QuantX {
    public static partial class EQuantExtension {
        /// <summary>
        /// 链式构造：HyperBar
        /// </summary>
        /// <param name="Source">源</param>
        /// <param name="Period">周期</param>
        /// <returns>HyperBar 实例</returns>
        public static TI.HyperBar HyperBar (this ITI<EQuant.BarType> Source, int Period) {
            return TI.HyperBar.GetInstance(Source, Period);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX {
    /// <summary>
    /// 超级 Bar 类
    /// </summary>
    public class HyperBar : ITI<EQuant.BarType> {
        /// <summary>
        /// 合并 BarType 到更高周期的 BarType
        /// </summary>
        /// <param name="ti">基础</param>
        /// <param name="cnt">合并周期</param>
        public HyperBar (ITI<EQuant.BarType> ti, int cnt) {
            _ = _cnt = cnt;
            ti.OnData += main;
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
            if (_ == _cnt) {
                bar = new EQuant.BarType(data);
            } else {
                bar.MaxPrice = Math.Max(bar.MaxPrice, data.MaxPrice);
                bar.MinPrice = Math.Min(bar.MinPrice, data.MinPrice);
                bar.Volume += data.Volume;
            }
            _--;
            if (_ == 0) {
                _ = _cnt;
                bar.ClosePrice = data.ClosePrice;
                bar.OpenInterest = data.OpenInterest;
                bufHistory.Add(bar);
                OnData?.Invoke(this, bufHistory.Last());
            }
        }
        private List<EQuant.BarType> bufHistory = new List<EQuant.BarType>();
        private int _, _cnt;
        private EQuant.BarType bar;
    }
}

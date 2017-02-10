using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX.TI {
    /// <summary>
    /// 基础 Bar 适配指标
    /// </summary>
    public class BaseBar : ITI<EQuant.BarType> {
        /// <summary>
        /// 从 PushBarEventHandler 到 BarType
        /// </summary>
        /// <param name="exe">Handler</param>
        public BaseBar (EQuant.STG.ContractExecuter exe) {
            exe.OnBar += main;
        }

        private void main (object sender, EQuant.STG.PushBarEventArgs e) {
            bufHistory.Add(new EQuant.BarType(e.Bar as EQuant.BarType));
            OnData?.Invoke(this, bufHistory.Last());
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
        private List<EQuant.BarType> bufHistory = new List<EQuant.BarType>();
    }
}

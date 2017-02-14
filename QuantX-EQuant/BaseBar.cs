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
            _exe = exe;
            _exe.OnBar += main;
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
        private EQuant.STG.ContractExecuter _exe;
        /// <summary>
        /// 获取 BaseBar 实例
        /// </summary>
        /// <param name="exe">合约执行器</param>
        /// <returns>BaseBar 实例</returns>
        public static BaseBar GetInstance (EQuant.STG.ContractExecuter exe) {
            foreach (var x in _instances) {
                if (exe.Equals(x._exe)) {
                    return x;
                }
            }
            var ins = new BaseBar(exe);
            _instances.Add(ins);
            return ins;
        }
        /// <summary>
        /// 实例池
        /// </summary>
        public static IEnumerable<BaseBar> Instances { get { return _instances; } }
        private static HashSet<BaseBar> _instances = new HashSet<BaseBar>();
    }
}

namespace QuantX {
    /// <summary>
    /// 拓展方法类
    /// </summary>
    public static partial class Extension {
        /// <summary>
        /// 链式构造 BaseBar 实例
        /// </summary>
        /// <param name="exe">合约执行器</param>
        /// <returns>BaseBar 实例</returns>
        public static TI.BaseBar BaseBar (this EQuant.STG.ContractExecuter exe) {
            return TI.BaseBar.GetInstance(exe);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX.TI {
    /// <summary>
    /// MACD 指标类
    /// </summary>
    public class MACD : ITI<MACDData> {
        /// <summary>
        /// 基于 TI 指标，构造参数为 (Fast, Slow, Diff) 的 MACD 指标
        /// </summary>
        /// <param name="TI">基础指标</param>
        /// <param name="Fast">快线周期</param>
        /// <param name="Slow">慢线周期</param>
        /// <param name="Diff">差线周期</param>
        public MACD (ITI<double> TI, int Fast, int Slow, int Diff) {
            _TI = TI;
            _Fast = Fast;
            _Slow = Slow;
            _Diff = Diff;
            _FEMA = _TI.EMA(_Fast);
            _SEMA = _TI.EMA(_Slow);
            _DIFF = Difference.GetInstance(_FEMA, _SEMA);
            _DEA = _DIFF.EMA(_Diff);
            _MACD = Difference.GetInstance(_DIFF, _DEA);
            //_TI.OnData += main;
            //_FEMA.OnData += main;
            //_SEMA.OnData += main;
            //_DIFF.OnData += main;
            //_DEA.OnData += main;
            _MACD.OnData += main;
        }

        private void main (object sender, double e) {
            int cc = _MACD.History.Count;
            for (int i = bufHistory.Count; i < cc; i++) {
                bufHistory.Add(new MACDData(_DIFF.History[i], _DEA.History[i], _MACD.History[i]));
                OnData?.Invoke(this, bufHistory.Last());
            }
        }
        /// <summary>
        /// 获得 MACD 实例
        /// </summary>
        /// <param name="Source">源数据</param>
        /// <param name="Fast">快线周期</param>
        /// <param name="Slow">慢线周期</param>
        /// <param name="Diff">差量周期</param>
        /// <returns>MACD 指标实例</returns>
        public static MACD GetInstance(ITI<double> Source, int Fast, int Slow, int Diff) {
            foreach (var x in _instances) {
                if (Source.Equals(x._TI) && Fast.Equals(x._Fast) && Slow.Equals(x._Slow) && Diff.Equals(x._Diff)) {
                    return x;
                }
            }
            var ins = new MACD(Source, Fast, Slow, Diff);
            _instances.Add(ins);
            return ins;
        }
        /// <summary>
        /// 隐式转换：取最新数据
        /// </summary>
        /// <param name="x">MACD 指标</param>
        public static implicit operator MACDData (MACD x) {
            return x.History.Last();
        }
        /// <summary>
        /// 历史数据
        /// </summary>
        public IReadOnlyList<MACDData> History {
            get {
                return bufHistory;
            }
        }
        /// <summary>
        /// 数据事件
        /// </summary>
        public event EventHandler<MACDData> OnData;
        private List<MACDData> bufHistory = new List<MACDData>();
        private ITI<double> _TI;
        private int _Fast;
        private int _Slow;
        private int _Diff;
        private EMA _FEMA;
        private EMA _SEMA;
        private Difference _DIFF;
        private EMA _DEA;
        private Difference _MACD;
        private static HashSet<MACD> _instances = new HashSet<MACD>();
    }
    /// <summary>
    /// MACD 数据类
    /// </summary>
    public class MACDData {
        /// <summary>
        /// 构造 MACD 数据
        /// </summary>
        /// <param name="diff">Fast - Slow</param>
        /// <param name="dma">ma(Diff)</param>
        /// <param name="macd">Diff - DEA</param>
        public MACDData (double diff, double dma, double macd) {
            Diff = diff;
            DEA = dma;
            MACD = macd;
        }
        /// <summary>
        /// Diff: 快慢线的差量
        /// </summary>
        public double Diff;
        /// <summary>
        /// DEA: Diff 的移动平均线
        /// </summary>
        public double DEA;
        /// <summary>
        /// MACD: Diff - DEA
        /// </summary>
        public double MACD;
    }
}
namespace QuantX {
    public static partial class Extension {
        /// <summary>
        /// 获得 MACD 实例
        /// </summary>
        /// <param name="Source">源数据</param>
        /// <param name="Fast">快线周期</param>
        /// <param name="Slow">慢线周期</param>
        /// <param name="Diff">差量周期</param>
        /// <returns>MACD 指标实例</returns>
        public static TI.MACD MACD (this ITI<double> Source, int Fast, int Slow, int Diff) {
            return TI.MACD.GetInstance(Source, Fast, Slow, Diff);
        }
    }
}
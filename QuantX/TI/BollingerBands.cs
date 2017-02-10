using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QuantX.TI {
    /// <summary>
    /// Boll 线数据类
    /// </summary>
    public class BollingerBandsData {
        /// <summary>
        /// 构造 Boll 数据实例
        /// </summary>
        /// <param name="upper">上轨</param>
        /// <param name="middle">中轨</param>
        /// <param name="lower">下轨</param>
        public BollingerBandsData(double upper, double middle, double lower) {
            if (upper < middle || middle < lower) {
                throw new Exception("Boll Data Invalid");
            }
            Upper = upper;
            Lower = lower;
            Middle = middle;
        }
        /// <summary>
        /// 上轨
        /// </summary>
        public double Upper;
        /// <summary>
        /// 中轨
        /// </summary>
        public double Middle;
        /// <summary>
        /// 下轨
        /// </summary>
        public double Lower;
        /// <summary>
        /// 通道宽度
        /// </summary>
        public double Width { get { return Upper - Lower; } }
        /// <summary>
        /// 宽度比例
        /// </summary>
        public double Ratio { get { return Width / Middle; } }
    }
    /// <summary>
    /// Boll 指标
    /// </summary>
    public class BollingerBands : ITI<BollingerBandsData> {
        /// <summary>
        /// 基于 TI 计算 period 周期 K 倍标准差的 Boll 指标
        /// </summary>
        /// <param name="TI">基础指标</param>
        /// <param name="N">Boll周期</param>
        /// <param name="K">标准差倍数</param>
        public BollingerBands (ITI<double> TI, int N, double K) {
            _TI = TI;
            _N = N;
            _K = K;
            _MA = SimpleMovingAverage.GetInstance(TI, N);
            _STDVAR = StdVar.GetInstance(TI, N);
            //_TI.OnData += main;
            //_MA.OnData += main;
            _STDVAR.OnData += main;
        }

        private void main (object sender, double e) {
            int cc = (new int[] { _TI.History.Count, _MA.History.Count, _STDVAR.History.Count }).Min();
            for (int i = bufHistory.Count; i < cc; i++) {
                double avg = _MA.History[i];
                double stdVar = _STDVAR.History[i];
                bufHistory.Add(new BollingerBandsData(avg + _K * stdVar, avg, avg - _K * stdVar));
                OnData?.Invoke(this, bufHistory.Last());
            }
        }

        /// <summary>
        /// 获取 Boll 指标实例
        /// </summary>
        /// <param name="TI">基础指标</param>
        /// <param name="N">周期</param>
        /// <param name="K">标准差倍数</param>
        /// <returns>Boll 指标实例</returns>
        public static BollingerBands GetInstance(ITI<double> TI, int N, double K) {
            foreach(var x in _instances) {
                if (TI.Equals(x._TI) && N.Equals(x._N) && K.Equals(x._K)) {
                    return x;
                }
            }
            var ret = new BollingerBands(TI, N, K);
            _instances.Add(ret);
            return ret;
        }
        /// <summary>
        /// Boll 历史数据
        /// </summary>
        public IReadOnlyList<BollingerBandsData> History {
            get {
                return bufHistory;
            }
        }
        /// <summary>
        /// 实例池
        /// </summary>
        public static IEnumerable<BollingerBands> Instances {
            get {
                return _instances;
            }
        }
        /// <summary>
        /// 新的 Boll 数据发生事件
        /// </summary>
        public event EventHandler<BollingerBandsData> OnData;
        private List<BollingerBandsData> bufHistory = new List<BollingerBandsData>();
        private ITI<double> _TI;
        private int _N;
        private double _K;
        private SimpleMovingAverage _MA;
        private StdVar _STDVAR;

        private static HashSet<BollingerBands> _instances = new HashSet<BollingerBands>();
    }
}

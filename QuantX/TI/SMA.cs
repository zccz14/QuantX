using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX.TI {
    /// <summary>
    /// 简单移动平均线
    /// </summary>
    public class SMA : ITI<double> {
        /// <summary>
        /// 基于 TI 上的 period 周期的简单移动平均线
        /// </summary>
        /// <param name="Source">基础指标</param>
        /// <param name="period">周期</param>
        private SMA (ITI<double> Source, int period) {
            p = new Param(Source, period);
            Source.OnData += main;
        }

        private void main (object sender, double e) {
            int cc = p.Source.History.Count;
            for (int i = bufHistory.Count; i < cc; i++) {
                bufHistory.Add(p.Source.History.Last(p.Period).Average());
                OnData?.Invoke(this, bufHistory.Last());
            }
            OnBurst?.Invoke(this, bufHistory.Count);
        }
        /// <summary>
        /// 获取SMA的实例
        /// </summary>
        /// <remarks>运用单例模式减少重复构造的开销</remarks>
        /// <param name="Source">基础指标</param>
        /// <param name="period">周期</param>
        /// <returns>SMA 实例</returns>
        public static SMA GetInstance (ITI<double> Source, int period) {
            var p = new Param(Source, period);
            if (!_instances.ContainsKey(p)) {
                _instances[p] = new SMA(Source, period);
            }
            return _instances[p];
        }
        private static Dictionary<Param, SMA> _instances = new Dictionary<Param, SMA>();
        /// <summary>
        /// 实例池
        /// </summary>
        public static IReadOnlyDictionary<Param, SMA> Instances {
            get { return _instances; }
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
        /// 隐式转换：取当前的指标值
        /// </summary>
        /// <param name="TI">指标</param>
        public static implicit operator double(SMA TI) {
            return TI.History.LastOrDefault();
        }

        /// <summary>
        /// 数据爆发输出事件
        /// </summary>
        public event EventHandler<int> OnBurst;
        /// <summary>
        /// 数据周期输出事件
        /// </summary>
        public event EventHandler<double> OnData;
        private List<double> bufHistory = new List<double>();
        private Param p;
        /// <summary>
        /// 转换成字符串
        /// </summary>
        /// <returns>转换成字符串</returns>
        public override string ToString () {
            return string.Format("{0}.SMA({1})", p.Source, p.Period);
        }
        /// <summary>
        /// SMA 参数
        /// </summary>
        public struct Param {
            /// <summary>
            /// 源
            /// </summary>
            public ITI<double> Source;
            /// <summary>
            /// 周期
            /// </summary>
            public int Period;
            /// <summary>
            /// SMA 参数构造器
            /// </summary>
            /// <param name="source">源</param>
            /// <param name="period">周期</param>
            public Param (ITI<double> source, int period) {
                Source = source;
                Period = period;
            }
        }
    }
}

namespace QuantX {
    public static partial class Extension {
        /// <summary>
        /// 获取指标的 SMA
        /// </summary>
        /// <param name="TI">基础指标</param>
        /// <param name="period">周期</param>
        /// <returns>SMA 实例</returns>
        public static TI.SMA SMA (this ITI<double> TI, int period) {
            return QuantX.TI.SMA.GetInstance(TI, period);
        }
    }
}
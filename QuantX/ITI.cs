/*
 * 技术指标接口 (ITI: Interface of Technical Indicator)
 */
using System;
using System.Collections.Generic;

namespace QuantX {
    /// <summary>
    /// 技术指标基本定义
    /// </summary>
    /// <typeparam name="T">指标输出类型</typeparam>
    public interface ITIBasic<T> {
        /// <summary>
        /// 输出事件
        /// </summary>
        event EventHandler<T> OnData;
    }
    /// <summary>
    /// 可持久化技术指标
    /// </summary>
    /// <typeparam name="T">指标输出类型</typeparam>
    public interface ITIPersistent<T> : ITIBasic<T> {
        /// <summary>
        /// 只读的历史记录
        /// </summary>
        IReadOnlyList<T> History { get; }
    }
    /// <summary>
    /// 常用的技术指标定义
    /// </summary>
    /// <typeparam name="T">指标输出类型</typeparam>
    public interface ITI<T> : ITIPersistent<T> { }
    /// <summary>
    /// 信号：输出为 bool 值的技术指标
    /// </summary>
    public interface ISignal : ITI<bool> {
        /// <summary>
        /// 当指标为真时触发的事件
        /// </summary>
        event EventHandler OnTrue;
        /// <summary>
        /// 当指标为假时触发的事件
        /// </summary>
        event EventHandler OnFalse;
    }
}

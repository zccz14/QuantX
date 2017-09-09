# QuantX

QuantX is a quantitive trade library for quant research and real-world trading.

QuantX is written by C#, which is simple and powerful enough and easy to extend.

QuantX is based on  `.NET Standard`, so it can be referenced by `.NET core`, `.NET Framework`, `Xamarin`, ... etc.

Features: (in draft)

+ Technical index framework (experimental)
  + Pre-build technical indexes
  + Customer technical indexes
+ Pre-build helpful static pure functions
+ Trading strategy framework
+ Strategy simulation



## Technical Index Framework

All quantitive trading relies on vivid technical indexes, such as Moving Average, MACD and Boll Bands. In other words, technical indexes take an essential role in quantitive trading.

How does QuantX help people build a technical index system? QuantX defined a technical index framework, so it is simple to write a new technical index. For most use cases, QuantX has some pre-build indexes.

QuantX takes a problem-oriented method to design the technical index system.

### TimeLine Scalar

The concept 'time' is abstract in quantive sequence analysis, sometimes we may ignore the actual datetime of price data, and just gives them an index number 0, 1, ... etc. However, multi-period analysis is needed. When taking diffrent period index into consideration, introducing the concept 'timeline' helps a lot.

General speaking, the original price data object is also a **technical index** and it is **atomic**, which can be composited into hyper period technical data. If compositing 1-minute bar into 15-minute bar, we call the 1-minute bar "**source**" and the 15-minute bar "**derivative**". And the derivative's time scalar is 15, which means the source data number is 15 times larger than the derivative.

To define the relation, we create two field "Source" and "TimeScalar". "Source" is the reference of the source technical index, and "TimeScalar" is the timeline scalar as what we talk above. 

```c#
public partial class TechnicalIndex {
    public TechnicalIndex Source { get; private set; }
    public int TimeScalar { get; private set; } = 1;
}
```

### Calculation Order

Almost technical indexes are dependent on original data and/or other technical indexes. A technical index instance doesn't know who is read its data. When updating data, a technical index will fire an `Update` event. A technical index has no duty to tell the following up technical index what was updated.

```c#
public partial class TechnicalIndex {
    public event EventHandler Update;
    protected void OnUpdate() { Update?.Invoke(this, EventArgs.Empty); }
}
```

### Multiple Sources

Several technical indexes have to listen to multiple sources, but we can still set up **only one equivalent** source to listen.

**We assumed that invoking listener has a order of event subscribing and the binding action is irreversible.**

```c#
public partial class TechnicalIndex {
    public TechnicalIndex Bind(TechnicalIndex source, int time = 1) { /* ...return this */ }
}
```

The method `Bind` returns `this` for chain-call style usage.

The order of binding is arbitrary.

```c#
var A = new TechnicalIndex();
var B = new TechnicalIndex().Bind(A, 2);
var C = new TechnicalIndex().Bind(A, 3);
var D = new TechnicalIndex().Bind(B, 2).Bind(C, 3); // same as E
var E = new TechnicalIndex().Bind(C, 3).Bind(B, 2); // same as D
```

How do we get the equivalent source? Is there always an single equivalent source for multiple source binding?

Treat the binding relation as a tree. The first binding of a new technical index is definity valid and easy to understand.

But what if we bind a technical index which has binding an existing source? we can find the lowest common ancestor (LCA) of the current source and the new source. And then rebind the callback listener to the LCA. By the way, we will update the TimeScalar to suit the new binding relation. If the LCA is invalid, we will throw an exception.

We need not to maintaince a list of tuple of Source and TimeScalar to implement this function. But we are also lost in the back way. If there is definity a need to reverse binding action, call the method `ResetBinding` to delete the current binding and then rebind other technical indexes.

```c#
public partial class TechnicalIndex {
    public TechnicalIndex ResetBinding() { /* ...return this */ }
}
```

### Historical Data Reference

#### Output Type Contract

If there is a contract of output type, we can set up a typed list to store historical data.

```c#
public partial class TechnicalIndex<T> : TechnicalIndex {
    protected List<T> History = new List<T>();
}
```

It's the beginning of the following attributes. Thanks to the declaration of output type, we can now define other exciting properties.

#### Read As List

Technical index must implemented IReadonlyList.

```c#
public partial class TechnicalIndex<T> : IReadonlyList<T> {
    public T this[int index] => History[index];
    public int Count => History.Count;
    public IEnumerator<T> GetEnumerator() => History.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Now, we can read historical data just like a simple list.

## Old Version

There's an old version reserved in `old-version` branch. [checkout](https://github.com/zccz14/QuantX/tree/old-version)
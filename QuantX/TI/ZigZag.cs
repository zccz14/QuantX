using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX.TI {
	public struct ZigZagData {
		public double value;
		/// <summary>
		/// isHigh
		/// </summary>
		public bool isHigh, isLow;
		public ZigZagData (double v, bool h, bool l) {
			value = v;
			isHigh = h;
			isLow = l;
		}
	}
	public struct ZigZagParam {
		public ITI<double> high, low;
		public int depth, deviation, backstep;
		public ZigZagParam(ITI<double> h, ITI<double> l, int dep, int dev, int bs) {
			high = h;
			low = l;
			depth = dep;
			deviation = dev;
			backstep = bs;
		}
	}
	public class ZigZag : ITI<ZigZagData> {
		public IReadOnlyList<ZigZagData> History {
			get {
				return bufHistory;
			}
		}

		public event EventHandler<ZigZagData> OnData;
		/// <summary>
		/// 获取ZigZag的实例
		/// </summary>
		/// <remarks>运用单例模式减少重复构造的开销</remarks>
		/// <param name="Source">基础指标</param>
		/// <param name="period">周期</param>
		/// <returns>ZigZag 实例</returns>
		public static ZigZag GetInstance (ZigZagParam p) {
			if (!_instances.ContainsKey(p)) {
				_instances[p] = new ZigZag(p);
			}
			return _instances[p];
		}
		private static Dictionary<ZigZagParam, ZigZag> _instances = new Dictionary<ZigZagParam, ZigZag>();
		/// <summary>
		/// 实例池
		/// </summary>
		public static IReadOnlyDictionary<ZigZagParam, ZigZag> Instances {
			get { return _instances; }
		}
		private ZigZag (ZigZagParam param) {
			arg = param;
			param.high.OnData += main;
			param.low.OnData += main;
		}
		private void main (object sender, double value) {
			int cc = Math.Min(arg.high.History.Count, arg.high.History.Count);
			for (int i = bufHistory.Count; i < cc; i++) {
				while (bufHistory.Count < i) {
					bufHistory.Add(new ZigZagData());
				}
				int res = calc(i, prev, arg.high.History, arg.low.History);
				for (int j = prev; j < res; j++) {
					OnData?.Invoke(this, bufHistory[j]);
				}
				prev = res;
			}
		}
		enum State {
			BOTH = 0,
			HIGH = 1,
			LOW = -1
		};
		private int calc (int rates_total, int prev_calculated, IReadOnlyList<double> high, IReadOnlyList<double> low) {
			int determinted = prev_calculated;
			if (prev_calculated == 0) {

			}
			double lastlow, lasthigh;
			int lastlowpos, lasthighpos;
			State s = State.BOTH;
			ZigZagData cur;
			if (prev_calculated == 0) {
				s = State.BOTH;
				lasthigh = 0;
				lastlow = 0;
				lasthighpos = -1;
				lastlowpos = -1;
			} else {
				if (bufHistory[prev_calculated - 1].isHigh) {
					s = State.LOW;
					lasthigh = bufHistory[prev_calculated - 1].value;
					lasthighpos = prev_calculated - 1;
					lastlow = 0;
					lastlowpos = -1;
				} else if (bufHistory[prev_calculated - 1].isLow) {
					s = State.HIGH;
					lasthigh = 0;
					lasthighpos = -1;
					lastlow = bufHistory[prev_calculated - 1].value;
					lastlowpos = prev_calculated - 1;
				} else {
					throw new Exception("ZigZag[prev_calculated - 1] is not a high/low point");
				}
			}
			// launch
			for (int i = prev_calculated; i < rates_total; i++) {
				// get low
				{
					double nearlow = low.ReverseAt(i).Take(arg.depth).Min();
					if (lastlow != nearlow) {
						lastlow = nearlow;
						if (low[i] <= nearlow + arg.deviation) {
							for (int j = 0; j < arg.backstep; j++) {
								if (i - j - 1 >= 0 && bufHistory[i - j - 1].isLow && bufHistory[i - j - 1].value > nearlow) {
									bufHistory[i - j - 1] = new ZigZagData();
								}
							}
						}
					}
					if (low[i] == nearlow) {
						bufHistory[i] = new ZigZagData(low[i], false, true);
					} else {
						bufHistory[i] = new ZigZagData();
					}
				}
				// get high
				{
					double nearhigh = high.ReverseAt(i).Take(arg.depth).Max();
					if (lasthigh != nearhigh) {
						lasthigh = nearhigh;
						if (high[i] >= nearhigh - arg.deviation) {
							for (int j = 0; j < arg.backstep; j++) {
								if (i - j - 1 >= 0 && bufHistory[i - j - 1].isHigh && bufHistory[i - j - 1].value < nearhigh) {
									bufHistory[i - j - 1] = new ZigZagData();
								}
							}
						}
					}
					if (high[i] == nearhigh) {
						bufHistory[i] = new ZigZagData(high[i], true, false);
					} else {
						bufHistory[i] = new ZigZagData();
					}
				}
				{
					if (s == State.BOTH) {
						if (lasthigh == 0 && lastlow == 0) {
							if (bufHistory[i].isHigh) {
								lasthigh = high[i];
								lasthighpos = i;
								s = State.LOW;
							}
							if (bufHistory[i].isLow) {
								lastlow = low[i];
								lastlowpos = i;
								s = State.HIGH;
							}
						}
					} else if (s == State.HIGH) {
						if (bufHistory[i].isLow && bufHistory[i].value < lastlow) {
							bufHistory[lastlowpos] = new ZigZagData();
							lastlowpos = i;
							lastlow = bufHistory[i].value;
						}
						if (bufHistory[i].isHigh) {
							lasthighpos = i;
							lasthigh = bufHistory[i].value;
							s = State.LOW;
						}
					} else if (s == State.LOW) {
						if (bufHistory[i].isHigh && bufHistory[i].value > lasthigh) {
							bufHistory[lasthighpos] = new ZigZagData();
							lasthighpos = i;
							lasthigh = bufHistory[i].value;
						}
						if (bufHistory[i].isLow) {
							lastlowpos = i;
							lasthigh = bufHistory[i].value;
							s = State.HIGH;
						}
					} else {
						throw new Exception("Invalid ZigZag State: " + s);
					}
				}
			}
			return determinted;
		}
		private ZigZagParam arg;
		private List<ZigZagData> bufHistory = new List<ZigZagData>();
		private int prev = 0;
	}
}
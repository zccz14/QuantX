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
		enum looking_for {
			UNKNOWN = 0, // searching for next high or low
			Pike = 1,  // searching for next high
			Sill = -1  // searching for next low
		};
		private int calc (int rates_total, int prev_calculated, IReadOnlyList<double> high, IReadOnlyList<double> low) {
			int i = 0, level = 3;
			int limit = 0, counterZ = 0;
			looking_for whatlookfor = looking_for.UNKNOWN;
			int shift = 0, back = 0, lasthighpos = 0, lastlowpos = 0;
			double val = 0, res = 0;
			double curlow = 0, curhigh = 0, lasthigh = 0, lastlow = 0;
			//--- initializing
			if (prev_calculated == 0) {
				for (int ii = 0; ii < rates_total; ii++) {
					bufHistory[ii] = new ZigZagData();
				}
			}
			//--- 
			if (rates_total < 100)
				return (0);
			//--- set start position for calculations
			if (prev_calculated == 0)
				limit = arg.depth;

			//--- ZigZag was already counted before
			if (prev_calculated > 0) {
				i = rates_total - 1;
				//--- searching third extremum from the last uncompleted bar
				while (counterZ < level && i > rates_total - 100) {
					res = bufHistory[i].value;
					if (res != 0)
						counterZ++;
					i--;
				}
				i++;
				limit = i;

				//--- what type of exremum we are going to find
				if (bufHistory[i].isLow) {
					curlow = bufHistory[i].value;
					whatlookfor = looking_for.Pike;
				} else {
					curhigh = bufHistory[i].value;
					whatlookfor = looking_for.Sill;
				}
				//--- chipping
				for (i = limit + 1; i < rates_total; i++) {
					bufHistory[i] = new ZigZagData();
				}
			}

			//--- searching High and Low
			for (shift = limit; shift < rates_total; shift++) {
				val = low.Last(arg.depth, low.Count - 1 - shift).Min();
				if (val == lastlow)
					val = 0.0;
				else {
					lastlow = val;
					if ((low[shift] - val) > arg.deviation)
						val = 0.0;
					else {
						for (back = 1; back <= arg.backstep; back++) {
							res = bufHistory[shift - back].value;
							if ((res != 0) && (res > val))
								bufHistory[shift - back] = new ZigZagData();
						}
					}
				}
				if (low[shift] == val)
					bufHistory[shift] = new ZigZagData(val, false, true);
				else
					bufHistory[shift] = new ZigZagData();
				//--- high
				val = high.Last(arg.depth, high.Count - 1 - shift).Max();
				if (val == lasthigh)
					val = 0.0;
				else {
					lasthigh = val;
					if ((val - high[shift]) > arg.deviation)
						val = 0.0;
					else {
						for (back = 1; back <= arg.backstep; back++) {
							res = bufHistory[shift - back].value;
							if ((res != 0) && (res < val))
								bufHistory[shift - back] = new ZigZagData();
						}
					}
				}
				if (high[shift] == val)
					bufHistory[shift] = new ZigZagData(val, true, false);
				else
					bufHistory[shift] = new ZigZagData();
			}

			//--- last preparation
			if (whatlookfor == 0)// uncertain quantity
			  {
				lastlow = 0;
				lasthigh = 0;
			} else {
				lastlow = curlow;
				lasthigh = curhigh;
			}

			//--- final rejection
			for (shift = limit; shift < rates_total; shift++) {
				res = 0.0;
				switch (whatlookfor) {
					case looking_for.UNKNOWN: // search for peak or lawn
						if (lastlow == 0 && lasthigh == 0) {
							if (bufHistory[shift].isHigh) {
								lasthigh = high[shift];
								lasthighpos = shift;
								whatlookfor = looking_for.Sill;
								bufHistory[shift] = new ZigZagData(lasthigh, true, false);
								res = 1;
							}
							if (bufHistory[shift].isLow) {
								lastlow = low[shift];
								lastlowpos = shift;
								whatlookfor = looking_for.Pike;
								bufHistory[shift] = new ZigZagData(lastlow, false, true);
								res = 1;
							}
						}
						break;
					case looking_for.Pike: // search for peak
						if (bufHistory[shift].isLow && bufHistory[shift].value < lastlow && !bufHistory[shift].isHigh) {
							bufHistory[lastlowpos] = new ZigZagData();
							lastlowpos = shift;
							lastlow = bufHistory[shift].value;
							bufHistory[shift] = new ZigZagData(lastlow, false, true);
							res = 1;
						}
						if (bufHistory[shift].isHigh && !bufHistory[shift].isLow) {
							lasthigh = bufHistory[shift].value;
							lasthighpos = shift;
							bufHistory[shift] = new ZigZagData(lasthigh, true, false);
							whatlookfor = looking_for.Sill;
							res = 1;
						}
						break;
					case looking_for.Sill: // search for lawn
						if (bufHistory[shift].isHigh && bufHistory[shift].value > lasthigh && !bufHistory[shift].isLow) {
							bufHistory[lasthighpos] = new ZigZagData();
							lasthighpos = shift;
							lasthigh = bufHistory[shift].value;
							bufHistory[shift] = new ZigZagData(lasthigh, true, false);
						}
						if (bufHistory[shift].isLow && !bufHistory[shift].isHigh) {
							lastlow = bufHistory[shift].value;
							lastlowpos = shift;
							bufHistory[shift] = new ZigZagData(lastlow, false, true);
							whatlookfor = looking_for.Pike;
						}
						break;
					default:
						return (rates_total);
				}
			}

			//--- return value of prev_calculated for next call
			return (rates_total);
		}
		private ZigZagParam arg;
		private List<ZigZagData> bufHistory = new List<ZigZagData>();
		private int prev = 0;
	}
}
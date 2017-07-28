using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantX.TI {
	public class ZigZagData {
		public double value, high, low;
		public ZigZagData (double v, double h, double l) {
			value = v;
			high = h;
			low = l;
		}
	}
	public struct ZigZagParam {
		public ITI<double> high, low;
		public int depth, deviation, backstep;
		public ZigZagParam (ITI<double> h, ITI<double> l, int dep, int dev, int bs) {
			high = h;
			low = l;
			depth = dep;
			deviation = dev;
			backstep = bs;
		}
	}
	public class ZigZag : ITI<ZigZagData> {
		public IReadOnlyList<ZigZagData> History => throw new Exception("Not Implemented");

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
			int cc = Math.Min(arg.high.History.Count, arg.low.History.Count);
			while (ZigzagBuffer.Count < cc) {
				ZigzagBuffer.Add(0);
				HighMapBuffer.Add(0);
				LowMapBuffer.Add(0);
			}
			int res = calc(cc, prev, arg.high.History, arg.low.History);
			OnData?.Invoke(this, null);
			prev = res;
		}
		private void ArrayInitialize<T> (List<T> list, T value) {
			for (int i = 0; i < list.Count; i++) {
				list[i] = value;
			}
		}
		private bool IsStopped () {
			return false;
		}
		public List<double> LowMapBuffer = new List<double>();
		public List<double> HighMapBuffer = new List<double>();
		public List<double> ZigzagBuffer = new List<double>();
		private int ExtDepth => arg.depth;
		private double deviation => arg.deviation;
		private int ExtBackstep => arg.backstep;
		private const int prepare = 200;
		private const int level = 3;
		private const int Pike = 1;
		private const int Sill = -1;
		private int calc (int rates_total, int prev_calculated, IReadOnlyList<double> high, IReadOnlyList<double> low) {
			int i = 0;
			int limit = 0, counterZ = 0, whatlookfor = 0;
			int shift = 0, back = 0, lasthighpos = 0, lastlowpos = 0;
			double val = 0, res = 0;
			double curlow = 0, curhigh = 0, lasthigh = 0, lastlow = 0;
			//--- initializing
			if (prev_calculated == 0) {
				ArrayInitialize(ZigzagBuffer, 0.0);

				ArrayInitialize(HighMapBuffer, 0.0);

				ArrayInitialize(LowMapBuffer, 0.0);
			}
			//--- 
			if (rates_total < prepare)
				return (0);
			//--- set start position for calculations
			if (prev_calculated == 0)
				limit = ExtDepth;

			//--- ZigZag was already counted before
			if (prev_calculated > 0) {
				i = rates_total - 1;
				//--- searching third extremum from the last uncompleted bar
				while (counterZ < level && i > rates_total - prepare) {
					res = ZigzagBuffer[i];
					if (res != 0)
						counterZ++;
					i--;
				}
				i++;
				limit = i;

				//--- what type of exremum we are going to find
				if (LowMapBuffer[i] != 0) {
					curlow = LowMapBuffer[i];
					whatlookfor = Pike;
				} else {
					curhigh = HighMapBuffer[i];
					whatlookfor = Sill;
				}
				//--- chipping
				for (i = limit + 1; i < rates_total && !IsStopped(); i++) {
					ZigzagBuffer[i] = 0.0;
					LowMapBuffer[i] = 0.0;
					HighMapBuffer[i] = 0.0;
				}
			}

			//--- searching High and Low
			for (shift = limit; shift < rates_total && !IsStopped(); shift++) {
				val = low[iLowest(low, ExtDepth, shift)];
				if (val == lastlow)
					val = 0.0;
				else {
					lastlow = val;
					if ((low[shift] - val) > deviation)
						val = 0.0;
					else {
						for (back = 1; back <= ExtBackstep; back++) {
							res = LowMapBuffer[shift - back];
							if ((res != 0) && (res > val))
								LowMapBuffer[shift - back] = 0.0;
						}
					}
				}
				if (low[shift] == val)
					LowMapBuffer[shift] = val;
				else
					LowMapBuffer[shift] = 0.0;
				//--- high
				val = high[iHighest(high, ExtDepth, shift)];
				if (val == lasthigh)
					val = 0.0;
				else {
					lasthigh = val;
					if ((val - high[shift]) > deviation)
						val = 0.0;
					else {
						for (back = 1; back <= ExtBackstep; back++) {
							res = HighMapBuffer[shift - back];
							if ((res != 0) && (res < val))
								HighMapBuffer[shift - back] = 0.0;
						}
					}
				}
				if (high[shift] == val)
					HighMapBuffer[shift] = val;
				else
					HighMapBuffer[shift] = 0.0;
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
			for (shift = limit; shift < rates_total && !IsStopped(); shift++) {
				switch (whatlookfor) {
					case 0: // search for peak or lawn
						if (lastlow == 0 && lasthigh == 0) {
							if (HighMapBuffer[shift] != 0) {
								lasthigh = high[shift];
								lasthighpos = shift;
								whatlookfor = Sill;
								ZigzagBuffer[shift] = lasthigh;
							}
							if (LowMapBuffer[shift] != 0) {
								lastlow = low[shift];
								lastlowpos = shift;
								whatlookfor = Pike;
								ZigzagBuffer[shift] = lastlow;
							}
						}
						break;
					case Pike: // search for peak
						if (LowMapBuffer[shift] != 0.0 && LowMapBuffer[shift] < lastlow && HighMapBuffer[shift] == 0.0) {
							ZigzagBuffer[lastlowpos] = 0.0;
							lastlowpos = shift;
							lastlow = LowMapBuffer[shift];
							ZigzagBuffer[shift] = lastlow;
						}
						if (HighMapBuffer[shift] != 0.0 && LowMapBuffer[shift] == 0.0) {
							lasthigh = HighMapBuffer[shift];
							lasthighpos = shift;
							ZigzagBuffer[shift] = lasthigh;
							whatlookfor = Sill;
						}
						break;
					case Sill: // search for lawn
						if (HighMapBuffer[shift] != 0.0 && HighMapBuffer[shift] > lasthigh && LowMapBuffer[shift] == 0.0) {
							ZigzagBuffer[lasthighpos] = 0.0;
							lasthighpos = shift;
							lasthigh = HighMapBuffer[shift];
							ZigzagBuffer[shift] = lasthigh;
						}
						if (LowMapBuffer[shift] != 0.0 && HighMapBuffer[shift] == 0.0) {
							lastlow = LowMapBuffer[shift];
							lastlowpos = shift;
							ZigzagBuffer[shift] = lastlow;
							whatlookfor = Pike;
						}
						break;
					default:
						return (rates_total);
				}
			}

			//--- return value of prev_calculated for next call
			return (rates_total);
		}
		//+------------------------------------------------------------------+
		//|  searching index of the highest bar                              |
		//+------------------------------------------------------------------+
		private int iHighest (IReadOnlyList<double> array,
			 int depth,

			 int startPos) {
			int index = startPos;
			//--- start index validation
			if (startPos < 0) {

				return 0;
			}
			int size = array.Count;
			//--- depth correction if need
			if (startPos - depth < 0)
				depth = startPos;
			double max = array[startPos];
			//--- start searching
			for (int i = startPos; i > startPos - depth; i--) {
				if (array[i] > max) {
					index = i;
					max = array[i];
				}
			}
			//--- return index of the highest bar
			return (index);
		}
		//+------------------------------------------------------------------+
		//|  searching index of the lowest bar                               |
		//+------------------------------------------------------------------+
		int iLowest (IReadOnlyList<double> array,
					int depth,
					int startPos) {
			int index = startPos;
			//--- start index validation
			if (startPos < 0) {
				return 0;
			}
			int size = array.Count;
			//--- depth correction if need
			if (startPos - depth < 0)
				depth = startPos;
			double min = array[startPos];
			//--- start searching
			for (int i = startPos; i > startPos - depth; i--) {
				if (array[i] < min) {
					index = i;
					min = array[i];
				}
			}
			//--- return index of the lowest bar
			return (index);
		}
		private ZigZagParam arg;
		private int prev = 0;
	}
}
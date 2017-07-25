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
		private ZigZag (ZigZagParam param) {
			arg = param;
			param.high.OnData += main;
			param.low.OnData += main;
		}
		private void main (object sender, double value) {
			int cc = Math.Min(arg.high.History.Count, arg.high.History.Count);
			if (cc < 100) {
				return;
			}
			for (int i = bufHistory.Count; i < cc; i++) {
				int level = 3, ii;
				int limit = 0, counterZ = 0, whatlookfor = 0;
				int shift = 0, back = 0, lasthighpos = 0, lastlowpos = 0;
				double val = 0, res = 0;
				double curlow = 0, curhigh = 0, lasthigh = 0, lastlow = 0;
				int prev_calculated = i, rates_total = cc;
				//--- set start position for calculations
				if (prev_calculated == 0) {
					limit = arg.depth;
				}

				//--- ZigZag was already counted before
				if (prev_calculated > 0) {
					ii = rates_total - 1;
					//--- searching third extremum from the last uncompleted bar
					while (counterZ < level && ii > rates_total - 100) {
						if (bufHistory[ii].value != 0)
							counterZ++;
						ii--;
					}
					ii++;
					limit = ii;

					//--- what type of exremum we are going to find
					if (bufHistory[ii].isLow) {
						curlow = bufHistory[ii].value;
						whatlookfor = 1;
					} else {
						curhigh = bufHistory[ii].value;
						whatlookfor = -1;
					}
					//--- chipping
					for (ii = limit + 1; ii < rates_total; ii++) {
						bufHistory[ii] = new ZigZagData();
					}
				}

				//--- searching High and Low
				for (shift = limit; shift < rates_total; shift++) {
					val = arg.low.History.Last(arg.depth, arg.low.History.Count - shift - 1).Min();
					if (val == lastlow) {
						val = 0.0;
					} else {
						lastlow = val;
						if ((arg.low.History[shift] - val) > arg.deviation) {
							val = 0.0;
						} else {
							for (back = 1; back <= arg.backstep; back++) {
								res = bufHistory[shift - back].value;
								if ((res != 0) && (res > val)) {
									bufHistory[shift - back] = new ZigZagData();
								}
							}
						}
					}
					if (arg.low.History[shift] == val) {
						bufHistory[shift] = new ZigZagData(val, false, true);
					} else {
						bufHistory[shift] = new ZigZagData();
					}
					//--- high
					val = arg.high.History.Last(arg.depth, arg.high.History.Count - 1 - shift).Max();

					if (val == lasthigh) {
						val = 0.0;
					} else {
						lasthigh = val;
						if ((val - arg.high.History[shift]) > arg.deviation) {
							val = 0.0;
						} else {
							for (back = 1; back <= arg.backstep; back++) {
								res = bufHistory[shift - back].value;
								if ((res != 0) && (res < val)) {
									bufHistory[shift - back] = new ZigZagData();
								}
							}
						}
					}
					if (arg.high.History[shift] == val) {
						bufHistory[shift] = new ZigZagData(val, true, false);
					} else {
						bufHistory[shift] = new ZigZagData();
					}
				}

				//--- last preparation
				if (whatlookfor == 0) {// uncertain quantity
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
						case 0: // search for peak or lawn
							if (lastlow == 0 && lasthigh == 0) {
								if (bufHistory[shift].isHigh) {
									lasthigh = arg.high.History[shift];
									lasthighpos = shift;
									whatlookfor = -1;
									bufHistory[shift] = new ZigZagData(lasthigh, true, false);
									res = 1;
								}
								if (bufHistory[shift].isLow) {
									lastlow = arg.low.History[shift];
									lastlowpos = shift;
									whatlookfor = 1;
									bufHistory[shift] = new ZigZagData(lastlow, false, true);
									res = 1;
								}
							}
							break;
						case 1: // search for peak
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
								whatlookfor = -1; // Sill
								res = 1;
							}
							break;
						case -1: // search for lawn
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
								whatlookfor = 1; // Pike
							}
							break;
					}
					OnData?.Invoke(this, bufHistory.Last());
				}

			}
		}
		private ZigZagParam arg;
		private List<ZigZagData> bufHistory = new List<ZigZagData>();

	}
}
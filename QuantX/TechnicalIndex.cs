﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantX
{
	public class TechnicalIndex
	{
		public TechnicalIndex Source { get; private set; }
		public int TimeScalar { get; private set; } = 1;
		public event EventHandler Update;
		protected void OnUpdate() => Update?.Invoke(this, EventArgs.Empty);

		public TechnicalIndex Root => Source == null ? this : Source.Root;
		public int Time => TimeScalar * Source?.Time ?? TimeScalar;
		public int Depth => Source?.Depth + 1 ?? 0;

		protected virtual void Main()
		{
		}

		private static TechnicalIndex LCA(TechnicalIndex u, TechnicalIndex v)
		{
			int du = u.Depth;
			int dv = v.Depth;
			while (du < dv)
			{
				v = v.Source;
				dv--;
			}
			while (dv < du)
			{
				u = u.Source;
				du--;
			}
			while (u != v)
			{
				v = v.Source;
				u = u.Source;
				du--;
				dv--;
			}
			return u;
		}

		private static int RelativeTime(TechnicalIndex thisIndex, TechnicalIndex thatIndex) => thisIndex.Time / thatIndex.Time;

		public TechnicalIndex Regist(TechnicalIndex index, int time)
		{
			if (Source != null)
			{
				var newSource = LCA(Source, index); // equivalent source
				// re-subscribe
				Source.Update -= OnSourceOnUpdate;
				newSource.Update += OnSourceOnUpdate;
				// re-assign member
				TimeScalar = Math.Max(RelativeTime(Source, newSource) * TimeScalar, val2: time);
				Source = newSource;
			} else
			{
				Source = index;
				TimeScalar = time;
				Source.Update += OnSourceOnUpdate;
			}
			return this;
		}

		private int Count { get; set; }
		private void OnSourceOnUpdate(object sender, EventArgs args)
		{
			Count++;
			if (Count % TimeScalar == 0)
			{
				Main();
			}
		}
	}
}
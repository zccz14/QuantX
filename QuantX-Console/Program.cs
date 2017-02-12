using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantX.TI;
using QuantX;

namespace QuantX_Console {
    class Program {
        static void Main (string[] args) {
            var iS = new Sequence<double>();
            iS.SMA(20).OnData += (sender, e) => {
                Console.WriteLine("{1} Data => {0}", e, sender);
            };
            iS.StdVar(20).OnData += (sender, e) => {
                Console.WriteLine("StdVar => {0}", e);
            };
            iS.Boll(20, 2.0).OnData += (sender, e) => {
                Console.WriteLine("Upper => {0}, Middle => {1}, Lower => {2}", e.Upper, e.Middle, e.Lower);
            };
            var arr = new double[] {
                2182, 2188, 2188, 2189, 2185,
                2188, 2193, 2184, 2190, 2195,
                2193, 2193, 2192, 2192, 2192,
                2191, 2198, 2195, 2191, 2192
            };
            foreach (var x in arr) {
                iS.Add(x);
            }
            while (true) {
                double price = double.Parse(Console.ReadLine());
                iS.Add(price);
                var MA = iS.SMA(2);
                Console.WriteLine("MAIns:{0} Length:{1}", SimpleMovingAverage.Instances.Count(), iS.History.Count());
                Console.WriteLine("{0}", Extension.isBackAdjusted(0, 2, 1, 0.5));
            }
        }
    }
}

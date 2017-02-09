using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantX.TI;

namespace QuantX_Console {
    class Program {
        static void Main (string[] args) {
            var iS = new Sequence<double>();
            SimpleMovingAverage.GetInstance(iS, 3).OnData += (sender, e) => {
                Console.WriteLine("SMA3 Data => {0}", e);
            };
            SimpleMovingAverage.GetInstance(iS, 2).OnData += (sender, e) => {
                Console.WriteLine("SMA2 Data => {0}", e);
            };
            SimpleMovingAverage.GetInstance(SimpleMovingAverage.GetInstance(iS, 3), 2).OnData += (sender, e) => {
                Console.WriteLine("SMA2 of SMA3 Data => {0}", e);
            };
            while (true) {
                double price = double.Parse(Console.ReadLine());
                iS.Add(price);
                var MA = SimpleMovingAverage.GetInstance(iS, 2);
                Console.WriteLine("MAIns:{0} Length:{1}", SimpleMovingAverage.Instances.Count(), iS.History.Count());
            }
        }
    }
}

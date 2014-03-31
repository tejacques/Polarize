using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polarize.Tests
{
    public class Program
    {
        public static void Main()
        {
            var benchmark = new Benchmarks();

            // Setup
            benchmark.SetUp();

            // JIT
            benchmark._Jit();

            // Real thing
            benchmark.BenchmarkFilterFieldsSpeed();
        }
    }
}

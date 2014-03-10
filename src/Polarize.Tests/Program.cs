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
            var test = new Tests();

            // Setup
            test.SetUp();

            // JIT
            test.A_Jit();

            // Real thing
            test.TestFilterFieldsSpeed();
        }
    }
}

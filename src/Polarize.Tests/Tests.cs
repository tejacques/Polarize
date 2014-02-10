using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polarize.Tests
{
    [TestFixture]
    public class Tests
    {
        Dictionary<string, Dictionary<string, int>> testDictionary;
        int loops = 50000;

        [TestFixtureSetUp]
        public void SetUp()
        {
            testDictionary = new Dictionary<string, Dictionary<string, int>>
            {
                { "A", new Dictionary<string, int>
                    {
                        {"a", 1},
                        {"b", 2},
                        {"c", 3}
                    }
                },
                { "B", new Dictionary<string, int>
                    {
                        {"i", 1},
                        {"j", 2},
                        {"k", 3}
                    }
                },
                { "C", new Dictionary<string, int>
                    {
                        {"x", 1},
                        {"y", 2},
                        {"z", 3}
                    }
                }
            };
        }

        [Test]
        public void A_Jit()
        {
            var tempLoops = loops;
            loops = 1;
            TestFilterSpeed();
            TestFilterFieldsSpeed();
            loops = tempLoops;
        }

        [Test]
        public void TestFilter()
        {
            JsonFilter<Dictionary<string, Dictionary<string, int>>> filter = testDictionary;

            var json = JsonConvert.SerializeObject(filter);
            var expected = "{\"A\":{\"a\":1,\"b\":2,\"c\":3},\"B\":{\"i\":1,\"j\":2,\"k\":3},\"C\":{\"x\":1,\"y\":2,\"z\":3}}";

            Assert.AreEqual(expected, json);
        }

        [Test]
        public void TestFilterSpeed()
        {
            JsonFilter<Dictionary<string, Dictionary<string, int>>> filter = testDictionary;
            for(int i = 0; i < loops; i++)
            {
                var json = JsonConvert.SerializeObject(filter);
            }
        }

        [Test]
        public void TestFilterFields()
        {
            JsonFilter<Dictionary<string, Dictionary<string, int>>> filter =
                JsonFilter.Create(testDictionary, "A", "B.i", "C.z");

            var json = JsonConvert.SerializeObject(filter);
            var expected = "{\"A\":{\"a\":1,\"b\":2,\"c\":3},\"B\":{\"i\":1},\"C\":{\"z\":3}}";
            Assert.AreEqual(expected, json);
        }

        [Test]
        public void TestFilterFieldsSpeed()
        {
            JsonFilter<Dictionary<string, Dictionary<string, int>>> filter =
                JsonFilter.Create(testDictionary, "A", "B.i", "C.z");
            for (int i = 0; i < loops; i++)
            {
                var json = JsonConvert.SerializeObject(filter);
            }
        }
    }
}

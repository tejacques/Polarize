using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polarize.Tests
{
    [TestFixture]
    public class Benchmarks
    {
        Dictionary<string, Dictionary<string, int>> testDictionary;
        int loops = 10000;
        List<object> l;

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


            int len = 50;
            l = new List<object>(len);

            for (int i = 0; i < len; i++)
            {
                l.Add(new
                {
                    field = 1
                });
            }
        }

        [Test]
        public void _Jit()
        {
            var tempLoops = loops;
            loops = 1;
            BenchmarkRawSpeed();
            BenchmarkFilterSpeed();
            BenchmarkFilterFieldsSpeed();
            BenchmarkFilterListWithLimitSpeed();
            BenchmarkSerializeListSpeed();
            BenchmarkSerializeListWithLimitSpeed();
            BenchmarkCreateFilter();
            loops = tempLoops;
        }

        [Test]
        public void BenchmarkRawSpeed()
        {
            for (int i = 0; i < loops; i++)
            {
                var json = JsonConvert.SerializeObject(testDictionary);
            }
        }

        [Test]
        public void BenchmarkFilterSpeed()
        {
            JsonFilter<Dictionary<string, Dictionary<string, int>>> filter = testDictionary;
            for(int i = 0; i < loops; i++)
            {
                var json = JsonConvert.SerializeObject(filter);
            }
        }

        [Test]
        public void BenchmarkFilterFieldsSpeed()
        {
            JsonFilter<Dictionary<string, Dictionary<string, int>>> filter =
                JsonFilter.Create(testDictionary, "A", "B.i", "C.z");
            for (int i = 0; i < loops; i++)
            {
                var json = JsonConvert.SerializeObject(filter);
            }
        }

        [Test]
        public void BenchmarkSerializeListSpeed()
        {
            for (int i = 0; i < loops; i++)
            {
                var json = JsonConvert.SerializeObject(l);
            }
        }

        [Test]
        public void BenchmarkSerializeListWithLimitSpeed()
        {
            for (int i = 0; i < loops; i++)
            {
                var json = JsonConvert.SerializeObject(
                    l.Skip(0).Take(5));
            }
        }

        [Test]
        public void BenchmarkFilterListWithLimitSpeed()
        {
            var filterJSON = @"{
                ""constraints"" : {
                    """" : {
                        ""limit"": 5,
                        ""offset"": 0
                    }
                }
            }";

            var filterIN = JsonConvert
                .DeserializeObject<JsonFilter>(filterJSON);

            var filter = JsonFilter.Create(l, filterIN);

            for (int i = 0; i < loops; i++)
            {
                var json = JsonConvert.SerializeObject(filter);
                var x = json;
            }
        }

        [Test]
        public void BenchmarkCreateFilter()
        {
            for (int i = 0; i < loops; i++)
            {
                var filterJSON = @"
                {
                    ""constraints"" : {
                        """" : {
                            ""limit"": 5,
                            ""offset"": 0
                        }
                    }
                }";

                var filterIN = JsonConvert
                    .DeserializeObject<JsonFilter>(filterJSON);

                var filter = JsonFilter.Create(l, filterIN);
            }
        }
    }
}

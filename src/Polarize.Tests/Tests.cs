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
    public class Tests
    {
        Dictionary<string, Dictionary<string, int>> testDictionary;
        int loops = 100000;

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
            var test = string.Join(".", "a.b.c".Split('.').Select(s => s)) + ".test";
            Console.WriteLine(test);
            var tempLoops = loops;
            loops = 1;
            //TestFilterSpeed();
            //TestFilterFieldsSpeed();
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

        [Test]
        public void TestFilter2()
        {
            var expected = "{\"b\":\"test2\"}";
            var filter = JsonFilter.Create(
                (object)(new { a = "test1", b = "test2" }),
                new [] { "b" });
            var json = JsonConvert.SerializeObject(filter);
            Assert.AreEqual(expected, json);
        }

        //[Test]
        public void TestFilter3()
        {
            var expected = "{\"b\":\"testb\"}";

            var jsonObject1 = JObject.Parse(@"{ fields : [
                'users.fields(
                    name.fields(first,middle,last),
                    y
                ).limit(5)',
                'score'
            ]}");

            var jsonObject2 = JObject.Parse(@"{
                fields : [
                    'users[0:10].name.first',
                    'users.name.middle',
                    'users.name.last',
                    'users.age',
                    'score'
                ],
                'contraints' : {
                    'info.name' : {
                        limit : 50,
                        offset : 10
                    }
                }
            }");

            var jsonObject3 = JObject.Parse(@"{ fields : [
                'users[0:9]:{
                    name:{first,middle,last},
                    age
                }',
            ]}");

            var filter = JsonFilter.Create(
                (object)(new
                {
                    users = new []
                    {
                        new {
                            name = new
                            {
                                first = "Tom",
                                middle = "Edward",
                                last = "Jacques"
                            },
                            age = 25
                        },
                        new {
                            name = new
                            {
                                first = "Chucky",
                                middle = "M.",
                                last = "Ellison"
                            },
                            age = 5000
                        }
                    },
                    score = 50
                }),
                jsonObject1);
            var json = JsonConvert.SerializeObject(filter);
            Assert.AreEqual(expected, json);

        }
    }
}

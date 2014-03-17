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
        public void A_Jit()
        {
            var tempLoops = loops;
            TestFilter();
            TestFilter2();
            TestFilter3();
            loops = 1;
            TestFilterSpeed();
            TestFilterFieldsSpeed();
            TestFilterListWithLimitSpeed();
            TestSerializeListSpeed();
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
        public void TestSerializeListSpeed()
        {
            for (int i = 0; i < loops; i++)
            {
                var json = JsonConvert.SerializeObject(l);
            }
        }

        [Test]
        public void TestFilterListWithLimitSpeed()
        {
            var filterJSON = @"{
                ""constraints"" : {
                    """" : {
                        ""limit"": 50,
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
        public void TestFilter2()
        {
            var expected = "{\"b\":\"test2\"}";
            var filter = JsonFilter.Create(
                (object)(new { a = "test1", b = "test2" }),
                new [] { "b" });
            var json = JsonConvert.SerializeObject(filter);
            Assert.AreEqual(expected, json);
        }

        [Test]
        public void TestFilter3()
        {
            var expected = "{"
                +"\"users\":[{"
                    +"\"name\":{"+
                        "\"first\":\"The\","
                        +"\"middle\":\"Incredible\","
                        +"\"last\":\"Hulk\""
                    +"},"
                    +"\"age\":25"
                +"}],"
                +"\"score\":50"
            +"}";

            var filterJSON = @"{
                ""fields"" : [
                    ""users.name.first"",
                    ""users.name.middle"",
                    ""users.name.last"",
                    ""users.age"",
                    ""score""
                ],
                ""constraints"" : {
                    ""users"" : {
                        ""limit"": 1,
                        ""offset"": 0
                    }
                }
            }";

            var filterIN = JsonConvert
                .DeserializeObject<JsonFilter>(filterJSON);

            var filter = JsonFilter.Create(
                (object)(new
                {
                    users = new []
                    {
                        new {
                            name = new
                            {
                                first = "The",
                                middle = "Incredible",
                                last = "Hulk"
                            },
                            age = 25
                        },
                        new {
                            name = new
                            {
                                first = "Tony",
                                middle = "Iron Man",
                                last = "Stark"
                            },
                            age = 5000
                        }
                    },
                    score = 50
                }),
                filterIN);
            var json = JsonConvert.SerializeObject(filter);
            Assert.AreEqual(expected, json);

        }

        internal class CustomConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(CustomConverterObject)
                    == objectType;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var val = (CustomConverterObject)value;
                writer.WriteStartObject();

                writer.WritePropertyName("first");
                writer.WriteValue(val.first);

                writer.WritePropertyName("second");
                writer.WriteValue(val.second);

                writer.WritePropertyName("third");
                writer.WriteValue(val.third);

                writer.WriteEndObject();

            }
        }

        [JsonConverter(typeof(CustomConverter))]
        internal class CustomConverterObject
        {
            public string first;
            public string second;
            public string third;
        }

        [Test]
        public void TestFilterCustomSerializer()
        {

            var expected = "{"
                + "\"objects\":[{"
                    + "\"first\":\"The\","
                    + "\"third\":\"Hulk\""
                + "}]"
            + "}";

            var filterJSON = @"{
                ""fields"" : [
                    ""objects.first"",
                    ""objects.third"",
                ],
                ""constraints"" : {
                    ""objects"" : {
                        ""limit"": 1,
                        ""offset"": 0
                    }
                }
            }";

            var filterIN = JsonConvert
                .DeserializeObject<JsonFilter>(filterJSON);

            var filter = JsonFilter.Create(
                (object)(new
                {
                    objects = new[]
                    {
                        new CustomConverterObject
                        {
                            first = "The",
                            second = "Incredible",
                            third = "Hulk"
                        },
                        new CustomConverterObject
                        {
                            first = "The",
                            second = "Incredible",
                            third = "Hulk"
                        },
                    }
                }),
                filterIN);
            var json = JsonConvert.SerializeObject(filter);
            Assert.AreEqual(expected, json);

        }
    }
}

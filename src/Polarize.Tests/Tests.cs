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
        public void TestFilter()
        {
            JsonFilter<Dictionary<string, Dictionary<string, int>>> filter = testDictionary;

            var json = JsonConvert.SerializeObject(filter);
            var expected = "{\"A\":{\"a\":1,\"b\":2,\"c\":3},\"B\":{\"i\":1,\"j\":2,\"k\":3},\"C\":{\"x\":1,\"y\":2,\"z\":3}}";

            Assert.AreEqual(expected, json);
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
        public void TestFilterFields2()
        {
            JsonFilter<Dictionary<string, Dictionary<string, int>>> filter =
                JsonFilter.Create(testDictionary, "D");

            var json = JsonConvert.SerializeObject(filter);
            var expected = "{}";
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

        /// <summary>
        /// Test to make sure we only get one thing back
        /// </summary>
        [Test]
        public void TestConstraints()
        {
            var expected = "[{"
                    + "\"A\":\"1\","
                    + "\"B\":\"2\","
                    + "\"C\":\"3\""
                + "}]";

            var filterJSON = @"{
                ""constraints"" : {
                    """" : {
                        ""limit"": 1,
                        ""offset"": 9
                    }
                }
            }";

            TestContraintsHelper(expected, filterJSON);
        }

        /// <summary>
        /// Test to make sure we only get one thing back
        /// </summary>
        [Test]
        public void TestConstraints2()
        {
            var expected = "[{"
                    + "\"A\":\"1\","
                    + "\"B\":\"2\","
                    + "\"C\":\"3\""
                + "}]";

            var filterJSON = @"{
                ""constraints"" : {
                    """" : {
                        ""limit"": 1,
                        ""offset"": 9
                    }
                }
            }";

            TestContraintsHelper(expected, filterJSON);
        }

        /// <summary>
        /// Test to make sure we only get one thing back
        /// </summary>
        [Test]
        public void TestConstraints3()
        {
            var expected = "[{"
                    + "\"A\":\"1\","
                    + "\"B\":\"2\","
                    + "\"C\":\"3\""
                + "}]";

            var filterJSON = @"{
                ""constraints"" : {
                    """" : {
                        ""limit"": 10,
                        ""offset"": 9
                    }
                }
            }";

            TestContraintsHelper(expected, filterJSON);
        }

        /// <summary>
        /// Test to make sure we get two things back
        /// </summary>
        [Test]
        public void TestConstraints4()
        {
            var expected = "["
                +"{"
                    + "\"A\":\"1\","
                    + "\"C\":\"3\""
                + "},"
                + "{"
                    + "\"A\":\"1\","
                    + "\"C\":\"3\""
                + "}"
            +"]";

            var filterJSON = @"{
                ""fields"" : [
                    ""A"",
                    ""C""
                ],
                ""constraints"" : {
                    """" : {
                        ""limit"": 2,
                        ""offset"": 1
                    }
                }
            }";

            TestContraintsHelper(expected, filterJSON);
        }

        private static void TestContraintsHelper(string expected, string filterJSON)
        {
            var dict = new Dictionary<string, string>()
            {
                { "A", "1"},
                { "B", "2"},
                { "C", "3"},
            };

            var list = new List<Dictionary<string, string>>(10);

            for (int i = 0; i < 10; i++)
            {
                list.Add(dict);
            }



            var filterIN = JsonConvert
                .DeserializeObject<JsonFilter>(filterJSON);

            var filter = JsonFilter.Create(list, filterIN);

            var json = JsonConvert.SerializeObject(filter);
            Assert.AreEqual(expected, json);
        }

        [Test]
        public void TestConstraints5()
        {
            var expected = "{"
                + "\"Items\":["
                + "{"
                    + "\"A\":\"1\""
                + "},"
                + "{"
                    + "\"A\":\"1\""
                + "}"
            + "]}";

            var filterJSON = @"{
                ""constraints"" : {
                    ""Items"" : {
                        ""limit"": 2,
                        ""offset"": 1
                    }
                }
            }";

            var dict = new Dictionary<string, string>()
            {
                { "A", "1"},
            };

            var list = new List<Dictionary<string, string>>(10);

            for (int i = 0; i < 4; i++)
            {
                list.Add(dict);
            }

            var obj = new
            {
                Items = list
            };

            var filterIN = JsonConvert
                .DeserializeObject<JsonFilter>(filterJSON);

            var filter = JsonFilter.Create(obj, filterIN);

            var json = JsonConvert.SerializeObject(filter);
            Assert.AreEqual(expected, json);
        }

        [Test]
        public void TestFilterCustomSerializerConstraints()
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
                        ""offset"": 1
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
                            second = "Amazing",
                            third = "Spiderman"
                        },
                        new CustomConverterObject
                        {
                            first = "The",
                            second = "Incredible",
                            third = "Hulk"
                        },
                        new CustomConverterObject
                        {
                            first = "The",
                            second = "Immovable",
                            third = "Object"
                        },
                    }
                }),
                filterIN);

            var json = JsonConvert.SerializeObject(filter);
            Assert.AreEqual(expected, json);

        }
    }
}

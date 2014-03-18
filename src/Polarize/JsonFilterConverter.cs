using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polarize
{
    public class JsonFilterConverter : JsonConverter
    {
        private static JsonSerializer _customSerializer;
        public override bool CanConvert(Type objectType)
        {
            return typeof(JsonFilter).IsAssignableFrom(objectType);
        }

        internal class JsonFilterValues
        {
            public string[] Fields;
            public Dictionary<string, JsonConstraint> Constraints;

            public JsonFilterValues()
            {
                Fields = null;
                Constraints = null;
            }
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var filter = serializer
                .Deserialize<JsonFilterValues>(reader);

            return new JsonFilter(
                null,
                filter.Fields,
                filter.Constraints);
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            JsonFilter jsf = (JsonFilter)value;

            // If there were no restrictions set, then serialize everything
            if ((null == jsf.Fields || jsf.Fields.Length == 0)
                && (null == jsf.Constraints || 0 == jsf.Constraints.Count))
            {
                serializer.Serialize(writer, jsf.Value);
                return;
            }

            var fieldStack = new List<string>();
            var customWriter = new JsonFilterWriter(writer, jsf, fieldStack);

            // Comment / Uncomment these lines to test modifying only the writer
            //serializer.Serialize(customWriter, jsf.Value);
            //return;

            var customConverter = new JsonFilterConverterInternal(
                serializer,
                jsf,
                writer,
                fieldStack);

            var customSerializer = GetSerializer(
                writer, serializer, customConverter, jsf, fieldStack);
            

            //customConverter.WriteJson(customWriter, jsf.Value, customSerializer);
            customSerializer.Serialize(customWriter, jsf.Value);
        }

        private static JsonSerializer GetSerializer(
            JsonWriter writer,
            JsonSerializer serializer,
            JsonFilterConverterInternal customConverter,
            JsonFilter jsf,
            List<string> fieldStack)
        {
            if (true)//null == _customSerializer)
            {
                _customSerializer = CreateSerializer(serializer);
            }

            SetConverter(customConverter);

            return _customSerializer;
        }

        private static void SetConverter(JsonFilterConverterInternal customConverter)
        {
            if (_customSerializer.Converters.Count > 0
                && _customSerializer.Converters[0] is JsonFilterConverterInternal)
            {
                _customSerializer.Converters[0] = customConverter;
            }
            else
            {
                _customSerializer.Converters.Insert(0, customConverter);
            }
        }

        private static JsonSerializer CreateSerializer(JsonSerializer serializer)
        {
            var serializerSettings = new JsonSerializerSettings()
            {
                Binder = serializer.Binder,
                CheckAdditionalContent = serializer.CheckAdditionalContent,
                ConstructorHandling = serializer.ConstructorHandling,
                ContractResolver = serializer.ContractResolver,
                Context = serializer.Context,
                Culture = serializer.Culture,
                DateFormatHandling = serializer.DateFormatHandling,
                DateFormatString = serializer.DateFormatString,
                DateParseHandling = serializer.DateParseHandling,
                DateTimeZoneHandling = serializer.DateTimeZoneHandling,
                DefaultValueHandling = serializer.DefaultValueHandling,
                FloatFormatHandling = serializer.FloatFormatHandling,
                FloatParseHandling = serializer.FloatParseHandling,
                Formatting = serializer.Formatting,
                MaxDepth = serializer.MaxDepth,
                MissingMemberHandling = serializer.MissingMemberHandling,
                NullValueHandling = serializer.NullValueHandling,
                ObjectCreationHandling = serializer.ObjectCreationHandling,
                PreserveReferencesHandling = serializer.PreserveReferencesHandling,
                ReferenceLoopHandling = serializer.ReferenceLoopHandling,
                ReferenceResolver = serializer.ReferenceResolver,
                StringEscapeHandling = serializer.StringEscapeHandling,
                TraceWriter = serializer.TraceWriter,
                TypeNameAssemblyFormat = serializer.TypeNameAssemblyFormat,
                TypeNameHandling = serializer.TypeNameHandling
            };

            return JsonSerializer.Create(serializerSettings);
        }

        public void WriteJsonSlow(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            JsonFilter jsf = (JsonFilter)value;
            if (null == jsf.Fields || jsf.Fields.Length == 0)
            {
                serializer.Serialize(writer, jsf.Value);
                return;
            }

            Stream stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            

            StreamWriter nullStreamWriter = new StreamWriter(Stream.Null);
            JsonWriter nullWriter = new JsonTextWriter(nullStreamWriter);

            JsonWriter myWriter = new JsonTextWriter(sw);
            serializer.Serialize(myWriter, jsf.Value);
            myWriter.Flush();
            stream.Position = 0;

            StreamReader sr = new StreamReader(stream);
            JsonReader myReader = new JsonTextReader(sr);

            Stack<string> fieldStack = new Stack<string>();
            while(myReader.Read())
            {
                if (JsonToken.PropertyName == myReader.TokenType)
                {
                    var name = myReader.Value as string;
                    fieldStack.Push(name);

                    string fieldPath = string.Join(".", fieldStack.Reverse());
                    
                    
                    if(jsf.FieldSet.Contains(fieldPath))
                    {
                        writer.WritePropertyName(name);
                        myReader.Read();
                        fieldStack.Pop();
                        writer.WriteToken(myReader, true);
                    }
                    else if (jsf.FieldPrefixSet.Contains(fieldPath))
                    {
                        writer.WritePropertyName(name);
                        myReader.Read();
                        if (!IsStartToken(myReader.TokenType))
                        {
                            fieldStack.Pop();
                        }
                        writer.WriteToken(myReader, false);
                    }
                    else
                    {
                        myReader.Read();
                        fieldStack.Pop();
                        nullWriter.WriteToken(myReader, true);
                    }
                }
                else
                {
                    if (IsEndToken(myReader.TokenType))
                    {
                        if (fieldStack.Count > 0)
                        {
                            fieldStack.Pop();
                        }
                    }
                    writer.WriteToken(myReader, false);
                }
                
            }

            
        }

        internal static bool IsEndToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                case JsonToken.EndConstructor:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsStartToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.StartObject:
                case JsonToken.StartArray:
                case JsonToken.StartConstructor:
                    return true;
                default:
                    return false;
            }
        }

    }
}

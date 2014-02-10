using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polarize
{
    public class JsonFilterSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(JsonFilter).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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

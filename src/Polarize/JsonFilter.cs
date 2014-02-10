using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polarize
{
    [JsonConverter(typeof(JsonFilterSerializer))]
    public class JsonFilter
    {
        public object Value;
        public readonly string[] Fields;
        public readonly HashSet<string> FieldPrefixSet;
        public readonly HashSet<string> FieldSet;

        public static JsonFilter<T> Create<T>(T value, params string[] fields)
        {
            return new JsonFilter<T>(value, fields);
        }

        internal JsonFilter(object value, string[] fields)
        {
            Value = value;

            if (null != fields)
            {
                Array.Sort(fields);
                Fields = fields;

                FieldSet = new HashSet<string>(fields);
                FieldPrefixSet = new HashSet<string>(
                    fields.SelectMany(field =>
                    {
                        var splitFields = field.Split(StringSplits.Period);
                        string[] fieldPaths = new string[splitFields.Length];
                        StringBuilder sb = new StringBuilder(splitFields[0]);
                        fieldPaths[0] = sb.ToString();
                        for (int i = 1; i < splitFields.Length; i++)
                        {
                            sb.Append('.');
                            sb.Append(splitFields[i]);
                            fieldPaths[i] = sb.ToString();
                        }

                        return fieldPaths;
                    }));
            }
        }
    }

    [JsonConverter(typeof(JsonFilterSerializer))]
    public class JsonFilter<T> : JsonFilter
    {
        internal JsonFilter(T value, string[] fields)
            : base(value, fields) {}

        public static implicit operator JsonFilter<T>(T t)
        {
            return JsonFilter.Create(t);
        }

        public static implicit operator T(JsonFilter<T> jsf)
        {
            return (T)jsf.Value;
        }
    }
}

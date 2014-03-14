using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polarize
{
    [JsonConverter(typeof(JsonFilterConverter))]
    public class JsonFilter
    {
        public object Value;
        public readonly JObject JObject;
        public readonly string[] Fields;
        public readonly HashSet<string> FieldPrefixSet;
        public readonly HashSet<string> FieldSet;
        public readonly Dictionary<string, JsonConstraint> Constraints;

        public static JsonFilter<T> Create<T>(
            T value,
            params string[] fields)
        {
            return new JsonFilter<T>(value, fields);
        }

        public static JsonFilter<T> Create<T>(
            T value,
            string[] fields,
            Dictionary<string, JsonConstraint> constraints)
        {
            return new JsonFilter<T>(value, fields, constraints);
        }

        public static JsonFilter<T> Create<T>(
            T value,
            JsonFilter filter)
        {
            return new JsonFilter<T>(value, filter);
        }

        internal JsonFilter(
            object value,
            string[] fields = null,
            Dictionary<string, JsonConstraint> constraints = null)
        {
            Value = value;

            if (null != fields)
            {
                // Copy the original fields
                Fields = fields.ToArray();

                // Sort copied array
                Array.Sort(Fields);
            }
            else
            {
                Fields = new string[0];
            }

            FieldSet = new HashSet<string>(Fields);
            FieldPrefixSet = new HashSet<string>(
                Fields.SelectMany(field =>
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


            if (null != constraints)
            {
                Constraints =
                    new Dictionary<string, JsonConstraint>(constraints);
            }
            else
            {
                Constraints =
                    new Dictionary<string, JsonConstraint>();
            }
        }

        internal JsonFilter(object value, JsonFilter filter)
            : this(value, filter.Fields, filter.Constraints)
        { }
    }

    [JsonConverter(typeof(JsonFilterConverter))]
    public class JsonFilter<T> : JsonFilter
    {
        internal JsonFilter(
            T value,
            string[] fields = null,
            Dictionary<string, JsonConstraint> constraints = null)
            : base(value, fields, constraints) {}

        internal JsonFilter(T value, JsonFilter filter)
            : base(value, filter) { }

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

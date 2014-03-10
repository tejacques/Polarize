using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polarize
{
    internal class JsonFilterConverterInternal : JsonConverter
    {
        private bool _intercept;
        private JsonSerializer _serializer;
        private JsonFilter _jsonFilter;
        private string _initialPath;
        private JsonWriter _writer;
        private List<string> _fieldStack;

        public JsonFilterConverterInternal(
            JsonSerializer serializer,
            JsonFilter jsonFilter,
            JsonWriter writer,
            List<string> fieldStack)
        {
            _intercept = true;
            _serializer = serializer;
            _jsonFilter = jsonFilter;
            _writer = writer;
            _initialPath = writer.Path;
            _fieldStack = fieldStack;
        }
        public override bool CanConvert(Type objectType)
        {
            // We always want to return true the first time, then false so
            // we fall back to the regular serializer
            var intercept = _intercept;
            _intercept = !_intercept;

            return intercept;
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            var fieldPath = GetContainerPath();

            if (fieldPath.Length == 0)
            {
                // Root level
                // Serialize this object but continue checking
                serializer.Serialize(writer, value);
            }
            else if (_jsonFilter.FieldSet.Contains(fieldPath))
            {
                // Serialize Everything
                _serializer.Serialize(_writer, value);
                _fieldStack.RemoveAt(_fieldStack.Count - 1);

                // Intercept the next call
                _intercept = true;
            }
            else if (_jsonFilter.FieldPrefixSet.Contains(fieldPath))
            {
                // Serialize this object but continue checking
                serializer.Serialize(writer, value);
                _fieldStack.RemoveAt(_fieldStack.Count - 1);
            }
            else
            {
                // Don't Serialize
                _fieldStack.RemoveAt(_fieldStack.Count - 1);

                // Intercept the next call
                _intercept = true;
            }
        }

        private string GetContainerPath()
        {
            //var currentPath = _writer.Path.Substring(_initialPath.Length);
            //var writeState = _writer.WriteState;

            //bool insideContainer = writeState != WriteState.Property;

            //int n = insideContainer ? 1 : 0;

            var fieldPath = string.Join(
                ".",
                _fieldStack);
            //currentPath
            //    .Split(StringSplits.Period)
            //    .Where(s => s.Length > 0 && !s.StartsWith("["))
            //    .ExceptLast(n));
            return fieldPath;
        }
    }
}

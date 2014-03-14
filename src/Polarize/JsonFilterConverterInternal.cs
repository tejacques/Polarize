using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
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

            if (fieldPath.Length == 0
                || _jsonFilter.Fields.Length == 0)
            {
                // Serialize this object but continue checking
                var toSerialize = ToSerialize(fieldPath, value);
                serializer.Serialize(
                    writer,
                    toSerialize);
            }
            else if (_jsonFilter.FieldSet.Contains(fieldPath))
            {
                // Serialize Everything
                _serializer.Serialize(
                    _writer,
                    ToSerialize(fieldPath, value));

                ((JsonFilterWriter)writer).WriteAllInThisProperty = false;
                PopFieldStack();

                // Intercept the next call
                _intercept = true;
            }
            else if (_jsonFilter.FieldPrefixSet.Contains(fieldPath))
            {
                // Serialize this object but continue checking
                serializer.Serialize(
                    writer,
                    ToSerialize(fieldPath, value));

                // The writer will pop for us
                //PopFieldStack();
            }
            else
            {
                // Don't Serialize
                PopFieldStack();

                // Intercept the next call
                _intercept = true;
            }
        }

        private void PopFieldStack()
        {
            // Pop only if we're not in an Array
            if (WriteState.Array != _writer.WriteState)
            {
                _fieldStack.RemoveAt(_fieldStack.Count - 1);
            }
        }

        private object ToSerialize(
            string fieldPath,
            object value)
        {
            JsonConstraint constraint;
            if (null == _jsonFilter.Constraints
                || !_jsonFilter
                .Constraints
                .TryGetValue(fieldPath, out constraint))
            {
                return value;
            }

            return ToSeriazeInner(value, constraint);
        }

        private object ToSeriazeInner(object value, JsonConstraint constraint)
        {
            var contract = _serializer
                .ContractResolver
                .ResolveContract(value.GetType());

            var arrayContract = contract as JsonArrayContract;
            if (arrayContract == null)
            {
                return value;
            }

            if (arrayContract.IsMultidimensionalArray)
            {
                return value;
            }

            // Remove the constraint so we don't use it in the writer
            // That's a lie we have to still use it, but it fucks up on
            // lists of lists
            //_jsonFilter.Constraints.Remove(fieldPath);

            var constrained = ((IEnumerable)value).Cast<object>()
                .Skip(constraint.Offset)
                .Take(constraint.Limit);

            return constrained;
        }

        private string GetContainerPath()
        {
            var fieldPath = string.Join(
                ".",
                _fieldStack);
            return fieldPath;
        }
    }
}

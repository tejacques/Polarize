using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polarize
{
    public class JsonFilterWriter : JsonWriter
    {
        private JsonWriter _writer;
        private JsonFilter _jsonFilter;
        private bool _shouldWrite;
        private string _initialPath;
        private List<string> _fieldStack;
        private int _startCount;

        public JsonWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        public JsonFilterWriter(
            JsonWriter writer,
            JsonFilter jsonFilter,
            List<string> fieldStack)
        {
            _writer = writer;
            _jsonFilter = jsonFilter;
            _shouldWrite = true;
            _initialPath = writer.Path;
            _fieldStack = fieldStack;
            _startCount = 0;
        }

        public void ResetShouldWrite()
        {
            _shouldWrite = true;
        }

        public List<String> FieldStack
        {
            get { return _fieldStack; }
            set { _fieldStack = value; }
        }

        private bool ShouldWrite()
        {

            var fieldPath = GetFieldPath();

            if(fieldPath.Length == 0)
            {
                // Root level
                _shouldWrite = true;
            }
            if (_jsonFilter.FieldSet.Contains(fieldPath))
            {
                // Serialize Everything
                _shouldWrite = true;
            }
            else if (_jsonFilter.FieldPrefixSet.Contains(fieldPath))
            {
                // Serialize this object but continue checking
                _shouldWrite = true;
            }
            else
            {
                // Don't Serialize
                _shouldWrite = false;
            }

            return _shouldWrite;
        }

        private string GetFieldPath()
        {
            //var currentPath = _writer.Path.Substring(_initialPath.Length);
            //var writeState = _writer.WriteState;

            //bool insideContainer = writeState != WriteState.Property;

            //int n = insideContainer ? 1 : 0;
            var fieldPath = string.Join(".",
                _fieldStack);
                //currentPath
                //    .Split(StringSplits.Period)
                //    .Where(s => s.Length > 0 && !s.StartsWith("["))
                //    .ExceptLast(n)
                //.Concat(new[] { propertyName }));

            return fieldPath;
        }

        #region Overrides

        public override void Close()
        {
            _writer.Close();
        }

        public override void WriteComment(string text)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteComment(text);
        }

        #region WriteEnd

        protected override void WriteEnd(JsonToken token)
        {
            throw new NotSupportedException();
        }

        public override void WriteEnd()
        {
            if (!_shouldWrite)
            {
                if (_startCount > 0)
                {
                    _startCount--;
                    return;
                }
                _shouldWrite = true;
            }
            _writer.WriteEnd();
        }

        public override void WriteEndArray()
        {
            if (!_shouldWrite)
            {
                if (_startCount > 0)
                {
                    _startCount--;
                    return;
                }
                _shouldWrite = true;
            }
            _writer.WriteEndArray();
        }

        public override void WriteEndConstructor()
        {
            if (!_shouldWrite)
            {
                if (_startCount > 0)
                {
                    _startCount--;
                    return;
                }
                _shouldWrite = true;
            }
            _writer.WriteEndConstructor();
        }

        public override void WriteEndObject()
        {
            if (!_shouldWrite)
            {
                if (_startCount > 0)
                {
                    _startCount--;
                    return;
                }
                _shouldWrite = true;
            }
            _writer.WriteEndObject();
         }

        #endregion

        public override void WriteNull()
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteNull();
        }

        #region WritePropertyName

        public override void WritePropertyName(string name)
        {
            _fieldStack.Add(name);
            if (!ShouldWrite())
            {
                return;
            }
            _writer.WritePropertyName(name);
        }

        public override void WritePropertyName(string name, bool escape)
        {
            _fieldStack.Add(name);
            if (!ShouldWrite())
            {
                return;
            }
            _writer.WritePropertyName(name, escape);
        }

        #endregion

        public override void WriteRaw(string json)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteRaw(json);
        }

        public override void WriteRawValue(string json)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteRawValue(json);
        }

        #region WriteStart
        public override void WriteStartArray()
        {
            if (!_shouldWrite)
            {
                _startCount++;
                return;
            }
            _writer.WriteStartArray();
        }

        public override void WriteStartConstructor(string name)
        {
            if (!_shouldWrite)
            {
                _startCount++;
                return;
            }
            _writer.WriteStartConstructor(name);
        }

        public override void WriteStartObject()
        {
            if (!_shouldWrite)
            {
                _startCount++;
                return;
            }
            _writer.WriteStartObject();
        }

        #endregion

        public override void WriteUndefined()
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteUndefined();
        }

        #region WriteValue

        public override void WriteValue(bool value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(bool? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(byte value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(byte? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(byte[] value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(char value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(char? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(DateTime value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(DateTime? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(DateTimeOffset value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(DateTimeOffset? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(decimal value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(decimal? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(double value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(double? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(float value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(float? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(Guid value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(Guid? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(int value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(int? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(long value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(long? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(object value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(sbyte value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(sbyte? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(short value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(short? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(string value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(TimeSpan value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(TimeSpan? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(uint value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(uint? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(ulong value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(ulong? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(Uri value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(ushort value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        public override void WriteValue(ushort? value)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteValue(value);
        }

        #endregion

        public override void WriteWhitespace(string ws)
        {
            if (!_shouldWrite)
            {
                return;
            }
            _writer.WriteWhitespace(ws);
        }

        public override void Flush()
        {
            _writer.Flush();
        }

        #endregion
    }
}

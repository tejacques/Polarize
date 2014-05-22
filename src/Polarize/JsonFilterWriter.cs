using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polarize
{
    internal class WriterState
    {
        public int Count;
        public int Limit;
        public int Offset;
        public bool ShouldWrite;

        public WriterState()
        {
            Count = 0;
            ShouldWrite = true;
        }

        public WriterState(JsonConstraint constraint) : this()
        {
            Limit = constraint.Limit;
            Offset = constraint.Offset;
        }

        public WriterState(
            Dictionary<string, JsonConstraint> constraints,
            string fieldPath)
            : this()
        {
            JsonConstraint constraint;
            if (null != constraints
                && constraints.TryGetValue(
                    fieldPath, out constraint))
            {
                Limit = constraint.Limit;
                Offset = constraint.Offset;
            }
            else
            {
                Limit = -1;
                Offset = 0;
            }
        }
    }
    public class JsonFilterWriter : JsonWriter
    {
        private JsonWriter _writer;
        private JsonFilter _jsonFilter;
        private bool _shouldWrite;
        private string _initialPath;
        private List<string> _fieldStack;
        private List<WriterState> _arrayStack;
        private int _startCount;
        private int _writeAllInThisProperty;

        internal bool WriteAllInThisProperty
        {
            get
            {
                return _writeAllInThisProperty > 0;
            }
            set
            {
                if (value)
                {
                    _writeAllInThisProperty = 1;
                }
                else
                {
                    _writeAllInThisProperty = 0;
                }
            }
        }

        public JsonWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        public JsonConstraint NextConstraint { get; set; }

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
            _writeAllInThisProperty = 0;
            _arrayStack = new List<WriterState>();
        }

        public List<String> FieldStack
        {
            get { return _fieldStack; }
            set { _fieldStack = value; }
        }

        private bool ShouldWriteProperty(string name)
        {
            if (!ShouldWriteArrayElement)
            {
                return false;
            }

            var jsonFieldsLen = _jsonFilter.Fields.Length;
            _fieldStack.Add(name);

            if (_writeAllInThisProperty > 0)
            {
                _writeAllInThisProperty++;
                return true;
            }

            string fieldPath = string.Empty;

            if (0 == jsonFieldsLen)
            {
                // Nothing is filtered
                _shouldWrite = true;
            }
            else if (0 == (fieldPath = GetFieldPath()).Length)
            {
                // Root level
                _shouldWrite = true;
            }
            else if (_jsonFilter.FieldSet.Contains(fieldPath))
            {
                // Serialize Everything
                _shouldWrite = true;
                _writeAllInThisProperty = 1;
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

                // Remove the field since we aren't serializing it
                PopFieldStackInner();
            }

            return ShouldWrite;
        }

        private string GetFieldPath()
        {
            var fieldPath = string.Join(".",
                _fieldStack);

            return fieldPath;
        }

        #region Overrides

        public override void Close()
        {
            _writer.Close();
        }

        public override void WriteComment(string text)
        {
            if (!ShouldWriteRaw)
            {
                return;
            }
            _writer.WriteComment(text);
        }

        private void PopFieldStackInner()
        {
            if(_fieldStack.Count > 0)
            {
                _fieldStack.RemoveAt(_fieldStack.Count - 1);
                if(_writeAllInThisProperty > 0)
                {
                    _writeAllInThisProperty--;
                }
            }
        }

        private void PopFieldStack()
        {
            // Pop only if we're not in an Array
            if (WriteState.Array != _writer.WriteState
                && _startCount == 0)
            {
                PopFieldStackInner();
            }
        }

        private void PopFieldStackWriteValue()
        {
            if (WriteState.Property == _writer.WriteState)
            {
                PopFieldStackInner();
                if (_startCount > 0)
                {
                    _startCount--;
                }
            }
        }

        private bool ShouldWriteEnd()
        {
            if (!_shouldWrite)
            {
                if (_startCount > 0)
                {
                    _startCount--;
                    return false;
                }
                _shouldWrite = true;
            }

            return ShouldWrite;
        }

        private bool ShouldWriteEndArray()
        {
            if (!_shouldWrite)
            {
                if (_startCount > 0)
                {
                    _startCount--;
                    return false;
                }
                _shouldWrite = true;
            }

            return true;
        }

        #region WriteEnd

        protected override void WriteEnd(JsonToken token)
        {
            throw new NotSupportedException();
        }

        public override void WriteEnd()
        {
            if (ShouldWriteEnd())
            {
                _writer.WriteEnd();
                PopFieldStack();
            }
        }

        public override void WriteEndArray()
        {
            if (ShouldWriteEndArray())
            {
                _writer.WriteEndArray();
                PopArrayStack();
                PopFieldStack();
            }
        }

        private void PopArrayStack()
        {
            _arrayStack.RemoveAt(_arrayStack.Count - 1);
        }

        public override void WriteEndConstructor()
        {
            if (ShouldWriteEnd())
            {
                _writer.WriteEndConstructor();
                PopFieldStack();
            }
        }

        public override void WriteEndObject()
        {
            if (ShouldWriteEnd())
            {
                _writer.WriteEndObject();
                PopFieldStack();
            }
         }

        #endregion

        public override void WriteNull()
        {
            if (!ShouldWrite)
            {
                return;
            }
            _writer.WriteNull();
        }

        #region WritePropertyName

        public override void WritePropertyName(string name)
        {
            if (!ShouldWriteProperty(name))
            {
                return;
            }
            _writer.WritePropertyName(name);
        }

        public override void WritePropertyName(string name, bool escape)
        {
            if (!ShouldWriteProperty(name))
            {
                return;
            }
            _writer.WritePropertyName(name, escape);
        }

        #endregion

        public override void WriteRaw(string json)
        {
            if (!ShouldWriteRaw)
            {
                return;
            }
            _writer.WriteRaw(json);
        }

        public override void WriteRawValue(string json)
        {
            if (!ShouldWriteRaw)
            {
                return;
            }
            _writer.WriteRawValue(json);
        }

        private bool ShouldWriteStart()
        {
            if (!ShouldWrite)
            {
                _startCount++;
                return false;
            }

            return true;
        }

        #region WriteStart
        public override void WriteStartArray()
        {
            if (ShouldWriteStart())
            {
                WriterState state;
                if (null != NextConstraint)
                {
                    state = new WriterState(NextConstraint);
                    NextConstraint = null;
                }
                else
                {
                    state = new WriterState(
                        _jsonFilter.Constraints,
                        GetFieldPath());
                }

                _arrayStack.Add(state);
                _writer.WriteStartArray();
            }
        }

        public override void WriteStartConstructor(string name)
        {
            if (ShouldWriteStart())
            {
                _writer.WriteStartConstructor(name);
            }
        }

        public override void WriteStartObject()
        {
            if (ShouldWriteStart())
            {
                _writer.WriteStartObject();
            }
        }

        #endregion

        public override void WriteUndefined()
        {
            if (!ShouldWriteRaw)
            {
                return;
            }
            _writer.WriteUndefined();
        }

        #region WriteValue

        // Why does it check that the state is WriteState.Array first?
        private bool ShouldWrite
        {
            get
            {
                bool shouldWrite = _shouldWrite;

                if (!shouldWrite)
                {
                    return false;
                }

                if (WriteState.Array == _writer.WriteState)
                {

                    var state = _arrayStack.Last();

                    if (!state.ShouldWrite)
                    {
                        return false;
                    }

                    var count = state.Count;
                    state.Count++;
                    // Check that offset has been passed
                    if (count < state.Offset)
                    {
                        return false;
                    }

                    // Check that limit has not been passed
                    if (state.Limit >= 0
                        && state.Count > state.Offset + state.Limit)
                    {
                        state.ShouldWrite = false;
                        _shouldWrite = false;
                        return false;
                    }

                    // Within limits
                    return true;
                }
                else if (_arrayStack.Count > 0)
                {
                    var state = _arrayStack.Last();

                    if (!state.ShouldWrite)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private bool ShouldWriteRaw
        {
            get
            {
                if (!_shouldWrite)
                {
                    return false;
                }
                else if (_arrayStack.Count > 0)
                {
                    var state = _arrayStack.Last();

                    if (!state.ShouldWrite)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private bool ShouldWriteArrayElement
        {
            get
            {
                if (_arrayStack.Count > 0)
                {
                    var state = _arrayStack.Last();

                    if (!state.ShouldWrite)
                    {
                        return false;
                    }
                }

                return true;
            }
        }


        public override void WriteValue(bool value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(bool? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(byte value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(byte? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(byte[] value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(char value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(char? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(DateTime value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(DateTime? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(DateTimeOffset value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(DateTimeOffset? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(decimal value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(decimal? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(double value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(double? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(float value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(float? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(Guid value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(Guid? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(int value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(int? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(long value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(long? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(object value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(sbyte value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(sbyte? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(short value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(short? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(string value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(TimeSpan value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(TimeSpan? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(uint value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(uint? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(ulong value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(ulong? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(Uri value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(ushort value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        public override void WriteValue(ushort? value)
        {
            PopFieldStackWriteValue();
            if (ShouldWrite)
            {
                _writer.WriteValue(value);
            }
        }

        #endregion

        public override void WriteWhitespace(string ws)
        {
            if (!ShouldWriteRaw)
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

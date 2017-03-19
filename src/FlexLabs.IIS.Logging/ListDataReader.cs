using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace FlexLabs.IIS.Logging
{
    class ListDataReader<T> : IDataReader
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly IDictionary<string, PropertyInfo> _properties;
        private readonly string[] _propertyNames;
        private readonly Type _type;

        public ListDataReader(IList<T> dataSource)
        {
            _enumerator = dataSource.GetEnumerator();
            _type = typeof(T);
            _properties = _type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(p => p.Name);
            _propertyNames = _properties.Keys.ToArray();
        }

        public object this[int i] => GetValue(i);
        public object this[string name] => GetValue(name);

        public int Depth => 1;
        public bool IsClosed => false;
        public int RecordsAffected => -1;
        public int FieldCount => _properties.Count;

        public void Close() { }
        public void Dispose() => _enumerator.Dispose();
        public bool NextResult() => false;
        public bool Read() => _enumerator.MoveNext();

        public object GetValue(int index) => _properties[GetName(index)].GetValue(_enumerator.Current, null);
        private object GetValue(string name) => _properties[name].GetValue(_enumerator.Current, null);

        public string GetName(int i) => _propertyNames[i];
        public bool IsDBNull(int i) => GetValue(i) == null;
        public int GetOrdinal(string name)
        {
            for (int i = 0; i < _propertyNames.Length; i++)
            {
                if (_propertyNames[i] == name)
                    return i;
            }
            return -1;
        }
        public Type GetFieldType(int i) => _properties[GetName(i)].PropertyType;
        public string GetDataTypeName(int i) => GetFieldType(i).Name;

        public bool GetBoolean(int i) => (bool)GetValue(i);
        public byte GetByte(int i) => (byte)GetValue(i);
        public char GetChar(int i) => (char)GetValue(i);
        public DateTime GetDateTime(int i) => (DateTime)GetValue(i);
        public decimal GetDecimal(int i) => (decimal)GetValue(i);
        public double GetDouble(int i) => (double)GetValue(i);
        public float GetFloat(int i) => (float)GetValue(i);
        public Guid GetGuid(int i) => (Guid)GetValue(i);
        public short GetInt16(int i) => (short)GetValue(i);
        public int GetInt32(int i) => (int)GetValue(i);
        public long GetInt64(int i) => (long)GetValue(i);
        public string GetString(int i) => (string)GetValue(i);

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }
    }
}

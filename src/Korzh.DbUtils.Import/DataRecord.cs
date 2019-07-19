﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Korzh.DbUtils
{

    public class DataRecord : IDataRecord
    {

        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        private readonly List<string> _keys = new List<string>();

        public int FieldCount => _properties.Count;

        public object this[string name]
        {
            get 
            {
                return _properties[name];
            }

            set 
            {
                _properties[name] = value;
                if (!_keys.Contains(name)) {
                    _keys.Add(name);
                }
            }
        }

        public object this[int i] => _properties[_keys[i]];


        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            var value = GetValue(i);

            if (value != null) {

                byte[] b = (byte[])value;

                if (buffer == null) {
                    return b.LongLength;
                }

                if (bufferoffset < b.Length) {
                    length = bufferoffset + length <= b.Length ? length : b.Length - bufferoffset;
                    Array.Copy(b, fieldOffset, buffer, bufferoffset, length);
                    return length;
                }
            }

            return 0;
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            var value = GetString(i);
            var b = value.ToCharArray();

            if (bufferoffset < b.Length) {
                length = bufferoffset + length <= b.Length ? length : b.Length - bufferoffset;
                Array.Copy(b, bufferoffset, buffer, 0, length);
                return length;
            }

            return 0;
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return GetFieldType(i).ToString();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            return GetValue(i).GetType();
        }

        public float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            return _keys[i];
        }

        public int GetOrdinal(string name)
        {
            return _keys.IndexOf(name);
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            return _properties[_keys[i]];
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            var value = GetValue(i);
            return value == null;
        }

    }
}
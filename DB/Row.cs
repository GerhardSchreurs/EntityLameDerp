using DB.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DB
{
    public interface IRow
    {
        DataRowState DataRowState { get; set; }
        string SetString(DataRow row, int index);
        Int16? SetInt16Null(DataRow row, int index);
        Int16 SetInt16(DataRow row, int index);
        Byte? SetByteNull(DataRow row, int index);
        Byte SetByte(DataRow row, int index);
        SByte? SetSByteNull(DataRow row, int index);
        SByte SetSByte(DataRow row, int index);
    }

    public class Row : IRow
    {
        private DataRowState _dataRowState;
        
        public DataRowState DataRowState { 
            get => _dataRowState; 
            set => _dataRowState = value; 
        }

        public virtual void Init(DataRow row)
        {
        }

        //byte, tinyint, short (0 / 255)
        public byte SetByte(DataRow row, int index)
        {
            return Converter.ToByte(row[index]);
        }

        public byte? SetByteNull(DataRow row, int index)
        {
            if (row.IsNull(index)) { return null; }
            return SetByte(row, index);
        }

        //smallint
        public short? SetInt16Null(DataRow row, int index)
        {
            if (row.IsNull(index)) { return null; }
            return SetInt16(row, index);
        }

        public short SetInt16(DataRow row, int index)
        {
            return Converter.ToInt16(row[index]);
        }

        //sbyte (-128 / 127)
        public sbyte SetSByte(DataRow row, int index)
        {
            return Converter.ToSByte(row[index]);
        }

        public sbyte? SetSByteNull(DataRow row, int index)
        {
            if (row.IsNull(index)) { return null; }
            return SetSByte(row, index);
        }

        public string SetString(DataRow row, int index)
        {
            if (row.IsNull(index)) { return null; }
            return Converter.ToString(row[index]);
        }
    }
}

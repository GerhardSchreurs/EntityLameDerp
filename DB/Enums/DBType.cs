using System;
using System.Collections.Generic;
using System.Text;

namespace DB
{
    public enum DBType
    {
        Decimal = 0,
        Byte = 1,
        Int16 = 2,
        Int32 = 3,
        Float = 4,
        Double = 5,
        Timestamp = 7,
        Int64 = 8,
        Int24 = 9,
        Date = 10,
        Time = 11,
        DateTime = 12,
        Year = 13,
        VarString = 15,
        Bit = 16,
        JSON = 245,
        TinyBlob = 249,
        MediumBlob = 250,
        LongBlob = 251,
        Blob = 252,
        VarChar = 253,
        String = 254,
        Geometry = 255,
        UByte = 501,
        UInt16 = 502,
        UInt32 = 503,
        UInt64 = 508,
        UInt24 = 509,
        TinyText = 749,
        MediumText = 750,
        LongText = 751,
        Text = 752,
        VarBinary = 753,
        Binary = 754,
        Guid = 854,
        None = 999
    }
}

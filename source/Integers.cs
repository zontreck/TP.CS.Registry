﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP.CS.Registry
{
    public class VInt16 : Entry
    {
        public VInt16(string name, short value) : base(EntryType.Int16, name)
        {
            Parent = null;
            Value = value;
        }
        public short Value { get; set; }

        public override void readValue(BinaryReader stream)
        {
            Value = stream.ReadInt16();
        }

        public override void Write(BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write(Value);
        }

        public override string PrettyPrint(int indent = 0)
        {
            return base.PrettyPrint(indent) + $" [{Value}]";
        }

        public override void setValue(object value)
        {
            base.setValue(value);

            if (value is short str)
            {
                Value = str;
            }
        }
        public VInt16 setInt16(short value)
        {
            Value = value;
            return this;
        }
    }
    public class VInt32 : Entry
    {
        public VInt32(string name, int value) : base(EntryType.Int32, name)
        {
            Parent = null;
            Value = value;
        }
        public int Value { get; set; }

        public override void readValue(BinaryReader stream)
        {
            Value = stream.ReadInt32();
        }

        public override void Write(BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write(Value);
        }
        public override string PrettyPrint(int indent = 0)
        {
            return base.PrettyPrint(indent) + $" [{Value}]";
        }

        public override void setValue(object value)
        {
            base.setValue(value);

            if (value is int str)
            {
                Value = str;
            }
        }
        public VInt32 setInt32(int value)
        {
            Value = value;
            return this;
        }
    }

    public class VInt64 : Entry
    {
        public VInt64(string name, long value) : base(EntryType.Int64, name)
        {
            Parent = null;
            Value = value;
        }
        public long Value { get; set; }

        public override void readValue(BinaryReader stream)
        {
            Value = stream.ReadInt64();
        }

        public override void Write(BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write(Value);
        }
        public override string PrettyPrint(int indent = 0)
        {
            return base.PrettyPrint(indent) + $" [{Value}]";
        }

        public override void setValue(object value)
        {
            base.setValue(value);

            if (value is long str)
            {
                Value = str;
            }
        }
        public VInt64 setInt64(long value)
        {
            Value = value;
            return this;
        }
    }

    public class VBool : Entry
    {
        public VBool(string name, bool value) : base(EntryType.Bool, name)
        {
            Parent = null;
            Value = value;
        }
        public bool Value { get; set; }

        public override void readValue(BinaryReader stream)
        {
            Value = stream.ReadBoolean();
        }

        public override void Write(BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write(Value);
        }
        public override string PrettyPrint(int indent = 0)
        {
            return base.PrettyPrint(indent) + $" [{Value}]";
        }

        public override void setValue(object value)
        {
            base.setValue(value);

            if (value is bool str)
            {
                Value = str;
            }
        }

        public VBool setBool(bool value)
        {
            Value = value;
            return this;
        }
    }
    public class VByte : Entry
    {
        public VByte(string name, byte value) : base(EntryType.Byte, name)
        {
            Parent = null;
            Value = value;
        }
        public byte Value { get; set; }

        public override void readValue(BinaryReader stream)
        {
            Value = stream.ReadByte();
        }

        public override void Write(BinaryWriter stream)
        {
            base.Write(stream);
            stream.Write(Value);
        }
        public override string PrettyPrint(int indent = 0)
        {
            return base.PrettyPrint(indent) + $" [{Value}]";
        }

        public override void setValue(object value)
        {
            base.setValue(value);

            if (value is byte str)
            {
                Value = str;
            }
        }

        public VByte setByte(byte value)
        {
            Value = value;
            return this;
        }
    }
}

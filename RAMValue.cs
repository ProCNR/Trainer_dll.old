using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trainer
{

    public enum ValueTypes
    {
        Byte,
        Bytes2,
        Bytes4,
        Bytes8,
        Bytes16,
        SignedByte,
        SignedBytes2,
        SignedBytes4,
        SignedBytes8,
        SignedBytes16,
        Bit0,
        Bit1,
        Bit2,
        Bit3,
        Bit4,
        Bit5,
        Bit6,
        Bit7,
        Int16,
        Int32, 
        Int64,
        SignedInt16,
        SignedInt32,
        SignedInt64,
        Float,
        Double,
        String
    }

    public class RAMValue
    {
        private static List<string> Keys = new List<string>();
        
        ValueTypes _valueType;
        object _value;
        string _key;

        /// <summary>
        /// Address to Read or Write
        /// </summary>
        UIntPtr _address;

        public UIntPtr Address { get => _address; }

        /// <summary>
        /// Determine if the value is Updated in the Ram watcher (Read)
        /// </summary>
        public bool Update { get; set; }
        /// <summary>
        /// Determine if the value is Written in the Ram watcher (Write)
        /// </summary>
        public bool Write { get; set; }
        /// <summary>
        /// Determine if the value if frozen (Keeps writing the same value)
        /// </summary>
        public bool Freeze { get; set; }

        /// <summary>
        /// Determine Thread number (for multi threading reads / writes)
        /// </summary>
        public uint ThreadNumber{ get; private set; }

        /// <summary>
        /// Type of the value
        /// </summary>
        public ValueTypes ValueType { get => _valueType; }
        /// <summary>
        /// Value
        /// </summary>
        public object Value { get => _value; set { _value = value; } }

        public string Key { get => _key; }

        public RAMValue(string key,UIntPtr address, ValueTypes valueType, object value, bool update = false, bool write = false, bool freeze = false, uint threadNumber = 0)
        {
            if (Keys == null)
                Keys = new List<string>();
            if (Keys.Contains(key))
                throw new ArgumentException("Duplicate key");
            _key = key;
            Keys.Add(key);
            _address = address;
            _valueType = valueType;
            _value = value;
            Freeze = freeze;
            if (freeze)
            {
                update = false;
                write = false;
            }
            Update = update;
            Write = write;
            ThreadNumber = threadNumber;
        }

        public RAMValue(string key, ulong address, ValueTypes valueType, object value, bool update = false, bool write = false, bool freeze = false, uint threadNumber = 0)
        {
            if (Keys == null)
                Keys = new List<string>();
            if (Keys.Contains(key))
                throw new ArgumentException("Duplicate key");
            _key = key;
            Keys.Add(key);
            _address = (UIntPtr)address;
            _valueType = valueType;
            _value = value;
            Freeze = freeze;
            if (freeze)
            {
                update = false;
                write = false;
            }
            Update = update;
            Write = write;
            ThreadNumber = threadNumber;
        }

        public bool ReadValue(ref MemoryReader memoryReader)
        {
            try
            {
                switch (this.ValueType)
                {
                    case ValueTypes.Byte:
                        _value = memoryReader.ReadSingleByte((ulong)_address);
                        break;
                    case ValueTypes.Bytes2:
                        _value = memoryReader.Read2Bytes((ulong)_address);
                        break;
                    case ValueTypes.Bytes4:
                        _value = memoryReader.Read4Bytes((ulong)_address);
                        break;
                    case ValueTypes.Bytes8:
                        _value = memoryReader.Read8Bytes((ulong)_address);
                        break;
                    case ValueTypes.Bytes16:
                        _value = memoryReader.Read16Bytes((ulong)_address);
                        break;
                    case ValueTypes.SignedByte:
                        _value = memoryReader.ReadSingleSignedByte((ulong)_address);
                        break;
                    case ValueTypes.SignedBytes2:
                        _value = memoryReader.ReadSigned2Bytes((ulong)_address);
                        break;
                    case ValueTypes.SignedBytes4:
                        _value = memoryReader.ReadSigned4Bytes((ulong)_address);
                        break;
                    case ValueTypes.SignedBytes8:
                        _value = memoryReader.ReadSigned8Bytes((ulong)_address);
                        break;
                    case ValueTypes.SignedBytes16:
                        _value = memoryReader.ReadSigned16Bytes((ulong)_address);
                        break;
                    case ValueTypes.Bit0:
                        _value = memoryReader.ReadSingleBit((ulong)_address,0);
                        break;
                    case ValueTypes.Bit1:
                        _value = memoryReader.ReadSingleBit((ulong)_address,1);
                        break;
                    case ValueTypes.Bit2:
                        _value = memoryReader.ReadSingleBit((ulong)_address,2);
                        break;
                    case ValueTypes.Bit3:
                        _value = memoryReader.ReadSingleBit((ulong)_address,3);
                        break;
                    case ValueTypes.Bit4:
                        _value = memoryReader.ReadSingleBit((ulong)_address,4);
                        break;
                    case ValueTypes.Bit5:
                        _value = memoryReader.ReadSingleBit((ulong)_address,5);
                        break;
                    case ValueTypes.Bit6:
                        _value = memoryReader.ReadSingleBit((ulong)_address,6);
                        break;
                    case ValueTypes.Bit7:
                        _value = memoryReader.ReadSingleBit((ulong)_address,7);
                        break;
                    case ValueTypes.Float:
                        _value = memoryReader.ReadFloat((ulong)_address);
                        break;
                    case ValueTypes.Double:
                        _value = memoryReader.ReadDouble((ulong)_address);
                        break;
                    case ValueTypes.Int16:
                        this._value = memoryReader.ReadInt16((ulong)_address);
                        break;
                    case ValueTypes.Int32:
                        _value = memoryReader.ReadInt32((ulong)_address);
                        break;
                    case ValueTypes.Int64:
                        _value = memoryReader.ReadInt64((ulong)_address);
                        break;
                    case ValueTypes.SignedInt16:
                        _value = memoryReader.ReadSignedInt16((ulong)_address);
                        break;
                    case ValueTypes.SignedInt32:
                        _value = memoryReader.ReadSignedInt32((ulong)_address);
                        break;
                    case ValueTypes.SignedInt64:
                        _value = memoryReader.ReadSignedInt64((ulong)_address);
                        break;
                    default:
                        return false;
                }
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }
    
        public bool WriteValue(ref MemoryWriter memoryWriter,byte byteValue=0)
        {
            bool result = false;
            switch (this.ValueType)
            {
                case ValueTypes.Byte:
                    result = memoryWriter.WriteSingleByte((ulong)_address, Convert.ToByte(this.Value));
                    break;
                case ValueTypes.Bytes2:
                    result = memoryWriter.Write2Bytes((ulong)_address, BitConverter.GetBytes((short)this.Value));
                    break;
                case ValueTypes.Bytes4:
                    result = memoryWriter.Write4Bytes((ulong)_address, (byte[])(Array)this.Value);
                    break;
                case ValueTypes.Bytes8:
                    result = memoryWriter.Write8Bytes((ulong)_address, (byte[])(Array)this.Value);
                    break;
                case ValueTypes.Bytes16:
                    result = memoryWriter.Write16Bytes((ulong)_address, (byte[])(Array)this.Value);
                    break;
                case ValueTypes.SignedByte:
                    result = memoryWriter.WriteSingleSignedByte((ulong)_address, Convert.ToSByte(this.Value));
                    break;
                case ValueTypes.SignedBytes2:
                    result = memoryWriter.WriteSigned2Bytes((ulong)_address, (sbyte[])(Array)this.Value);
                    break;
                case ValueTypes.SignedBytes4:
                    result = memoryWriter.WriteSigned4Bytes((ulong)_address, (sbyte[])(Array)this.Value);
                    break;
                case ValueTypes.SignedBytes8:
                    result = memoryWriter.WriteSigned8Bytes((ulong)_address, (sbyte[])(Array)this.Value);
                    break;
                case ValueTypes.SignedBytes16:
                    result = memoryWriter.WriteSigned16Bytes((ulong)_address, (sbyte[])(Array)this.Value);
                    break;
                case ValueTypes.Bit0:
                    result = memoryWriter.WriteSingleBit((ulong)_address, byteValue, Convert.ToBoolean(this.Value), 0);
                    break;
                case ValueTypes.Bit1:
                    result = memoryWriter.WriteSingleBit((ulong)_address, byteValue, Convert.ToBoolean(this.Value), 1);
                    break;
                case ValueTypes.Bit2:
                    result = memoryWriter.WriteSingleBit((ulong)_address, byteValue, Convert.ToBoolean(this.Value), 2);
                    break;
                case ValueTypes.Bit3:
                    result = memoryWriter.WriteSingleBit((ulong)_address, byteValue, Convert.ToBoolean(this.Value), 3);
                    break;
                case ValueTypes.Bit4:
                    result = memoryWriter.WriteSingleBit((ulong)_address, byteValue, Convert.ToBoolean(this.Value), 4);
                    break;
                case ValueTypes.Bit5:
                    result = memoryWriter.WriteSingleBit((ulong)_address, byteValue, Convert.ToBoolean(this.Value), 5);
                    break;
                case ValueTypes.Bit6:
                    result = memoryWriter.WriteSingleBit((ulong)_address, byteValue, Convert.ToBoolean(this.Value), 6);
                    break;
                case ValueTypes.Bit7:
                    result = memoryWriter.WriteSingleBit((ulong)_address, byteValue, Convert.ToBoolean(this.Value), 7);
                    break;
                case ValueTypes.Float:
                    result = memoryWriter.WriteFloat((ulong)_address, Convert.ToSingle(this.Value));
                    break;
                case ValueTypes.Double:
                    result = memoryWriter.WriteDouble((ulong)_address, Convert.ToDouble(this.Value));
                    break;
                case ValueTypes.Int16:
                    result = memoryWriter.WriteInt16((ulong)_address, Convert.ToUInt16(this.Value));
                    break;
                case ValueTypes.Int32:
                    result = memoryWriter.WriteInt32((ulong)_address, Convert.ToUInt32(this.Value));
                    break;
                case ValueTypes.Int64:
                    result = memoryWriter.WriteInt64((ulong)_address, Convert.ToUInt64(this.Value));
                    break;
                case ValueTypes.SignedInt16:
                    result = memoryWriter.WriteSignedInt16((ulong)_address, Convert.ToInt16(this.Value));
                    break;
                case ValueTypes.SignedInt32:
                    result = memoryWriter.WriteSignedInt32((ulong)_address, Convert.ToInt32(this.Value));
                    break;
                case ValueTypes.SignedInt64:
                    result = memoryWriter.WriteSignedInt64((ulong)_address, Convert.ToInt64(this.Value));
                    break;
                default:
                    result = false;
                    break;
            }
            if(result)
                Write = false;
            return result;
        }
       
        public void RemoveKey()
        {
            Keys.Remove(_key);
        }

        public static void EraseKeys()
        {
            Keys = null;
        }
    }
    
    
}

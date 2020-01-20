using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Trainer
{
    public class MemoryWriter : IDisposable
    {
        private string _processName;
        private string _moduleName;
        private Process _process;
        private ProcessModule _processModule;
        private IntPtr _processPointer;
        private IntPtr _processPointerWithOffset;
        private IntPtr _modulePointer;
        private IntPtr _modulePointerWithOffset;
        private bool _useModule;
        private bool _useOffset;

        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, int nSize, IntPtr lpNumberOfBytesWritten);


        public MemoryWriter(string processName, bool is64bits = false, long? processOffset = null, List<int> processOffsetList = null, string moduleName = null, long? moduleOffset = null, List<int> moduleOffsetList = null)
            : this(new TrainerPointer(processName, is64bits, processOffset, processOffsetList, moduleName, moduleOffset, moduleOffsetList))
        { }

        public MemoryWriter(TrainerPointer trainerPointer)
        {
            _processName = trainerPointer.ProcessName;
            _process = Process.GetProcessesByName(_processName).FirstOrDefault();
            _processPointer = OpenProcess(PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, _process.Id);

            if(!trainerPointer.UseModule)
            {
                _useModule = false;
                _processPointerWithOffset = trainerPointer.ProcessOffset.HasValue ? (IntPtr)(_process.MainModule.BaseAddress.ToInt64() + (long)trainerPointer.ProcessOffset): _process.MainModule.BaseAddress;
                // if offset are used, get the correct address through pointer
                if (trainerPointer.UseOffset)
                {
                    using(var mr = new MemoryReader(trainerPointer))
                    {
                        _processPointerWithOffset = mr.ProcessPointerWithOffset;
                    }
                }
            }
            // else use module address
            else
            {
                _useModule = true;
                _moduleName = trainerPointer.ModuleName;
                _processModule = _process.Modules.Cast<ProcessModule>().SingleOrDefault(module => string.Equals(module.ModuleName, _moduleName, StringComparison.OrdinalIgnoreCase));
                _modulePointer = _processModule.BaseAddress;
                _modulePointerWithOffset = trainerPointer.ModuleOffset.HasValue ? (IntPtr)((long)_processModule.BaseAddress + trainerPointer.ModuleOffset) : _processModule.BaseAddress;

                if (trainerPointer.UseOffset)
                {
                    using (var mr = new MemoryReader(trainerPointer))
                    {
                        _modulePointerWithOffset = mr.ModulePointerWithOffset;
                    }
                }
            }
        }

        public void Dispose()
        {
            _processName = null;
            _moduleName = null;
            _processPointer = IntPtr.Zero;
            _modulePointer = IntPtr.Zero;
            _processPointerWithOffset = IntPtr.Zero;
            _modulePointerWithOffset = IntPtr.Zero;
            _process = null;
            _processModule = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Write multiple bytes value at memory address offset
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool WriteBytes(ulong addressOffset, byte[] value, ulong length = 0)
        {
            if (length > 0 && (int)length < value.Length)
                throw new ArgumentOutOfRangeException("if length is used, it must be higher than values length");

            UIntPtr finalAddress = _useModule ? 
                (UIntPtr)((ulong)_modulePointerWithOffset + addressOffset) : (UIntPtr)((ulong)_processPointerWithOffset + addressOffset);
            
            if (length>0)
            {
                byte[] tmp = value;
                value = new byte[length];
                for (int i = 0; i < tmp.Length; i++)
                    value[i] = tmp[i];
            }        
            
            return WriteProcessMemory(_processPointer,finalAddress,value,value.Length,IntPtr.Zero);
        }


        /// <summary>
        /// Write a unsigned byte value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteSingleByte(ulong addressOffset, byte value) =>
            WriteBytes(addressOffset, new byte[] { value });

        /// <summary>
        /// Write a unsigned 2 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Write2Bytes(ulong addressOffset, byte[] value) =>
            WriteBytes(addressOffset, value, 2);

        /// <summary>
        /// Write a unsigned 4 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Write4Bytes(ulong addressOffset, byte[] value) =>
            WriteBytes(addressOffset, value, 4);

        /// <summary>
        /// Write a unsigned 8 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Write8Bytes(ulong addressOffset, byte[] value) =>
            WriteBytes(addressOffset, value, 8);

        /// <summary>
        /// Write a unsigned 16 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Write16Bytes(ulong addressOffset, byte[] value) =>
            WriteBytes(addressOffset, value, 16);

        /// <summary>
        /// Write a signed byte value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteSingleSignedByte(ulong addressOffset, sbyte value) =>
            WriteBytes(addressOffset, new byte[] { (byte)value }, 1);

        /// <summary>
        /// Write a signed 2 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteSigned2Bytes(ulong addressOffset, sbyte[] value) =>
            WriteBytes(addressOffset, (byte[])(Array)value, 2);

        /// <summary>
        /// Write a signed 4 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteSigned4Bytes(ulong addressOffset, sbyte[] value) =>
            WriteBytes(addressOffset, (byte[])(Array)value, 4);

        /// <summary>
        /// Write a signed 8 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteSigned8Bytes(ulong addressOffset, sbyte[] value) =>
            WriteBytes(addressOffset, (byte[])(Array)value, 8);

        /// <summary>
        /// Write a signed 16 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteSigned16Bytes(ulong addressOffset, sbyte[] value) =>
            WriteBytes(addressOffset, (byte[])(Array)value, 16);

        /// <summary>
        /// Write a unsigned int16 value (short)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteInt16(ulong addressOffset, ushort value) =>
            WriteBytes(addressOffset, BitConverter.GetBytes(value), 2);

        /// <summary>
        /// Write a unsigned int32 value (int)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteInt32(ulong addressOffset, uint value) =>
            WriteBytes(addressOffset, BitConverter.GetBytes(value), 4);

        /// <summary>
        /// Write a unsigned int64 value (long)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteInt64(ulong addressOffset, ulong value) =>
            WriteBytes(addressOffset, BitConverter.GetBytes(value), 8);

        /// <summary>
        /// Write a signed int16 value (short)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteSignedInt16(ulong addressOffset, short value) =>
            WriteBytes(addressOffset, BitConverter.GetBytes(value), 2);

        /// <summary>
        /// Write a signed int32 value (int)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteSignedInt32(ulong addressOffset, int value) =>
            WriteBytes(addressOffset, BitConverter.GetBytes(value), 4);

        /// <summary>
        /// Write a signed int64 value (long)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteSignedInt64(ulong addressOffset, long value) =>
            WriteBytes(addressOffset, BitConverter.GetBytes(value), 8);

        /// <summary>
        /// Write a float value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteFloat(ulong addressOffset, float value) =>
            WriteBytes(addressOffset, BitConverter.GetBytes(value), 4);

        /// <summary>
        /// Write a double value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteDouble(ulong addressOffset, double value) =>
            WriteBytes(addressOffset, BitConverter.GetBytes(value), 8);

        /// <summary>
        /// Write a boolean array to a byte
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <param name="nBit"></param>
        /// <returns></returns>
        public bool WriteBits(ulong addressOffset, bool[] value)
        {
            if (value.Length != 8)
                throw new ArgumentException("The value array length must be 8");
            byte buffer = 0;
            for (int i = 0; i < 8; i++)
                if (value[i])
                    buffer |= (byte)(1 << i);
            return WriteSingleByte(addressOffset, buffer);
        }

        /// <summary>
        /// Write a single bit at given position
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="currentByteValue"></param>
        /// <param name="value"></param>
        /// <param name="nBit"></param>
        /// <returns></returns>
        public bool WriteSingleBit(ulong addressOffset, byte currentByteValue, bool value, byte nBit)
        {
            if (nBit > 7)
                throw new ArgumentException("nBit must be between 0 and 7");
            byte mask = (byte)(1 << nBit);
            byte buffer = (byte)((currentByteValue & ~mask) | (((byte)(value ? 1 : 0) << nBit) & mask));
            return WriteSingleByte(addressOffset, buffer);
        }

        /// <summary>
        /// Write a ASCII coded string
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteStringASCII(ulong addressOffset,string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write an UTF-8 coded String
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteStringUTF8(ulong addressOffset, string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write an UTF-16 (Unicode) coded String
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteStringUnicode(ulong addressOffset, string value)
        {
            throw new NotImplementedException();
        }
    }
}

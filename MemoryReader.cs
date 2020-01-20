using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Trainer
{
    public class MemoryReader : IDisposable
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

        public IntPtr ProcessPointerWithOffset { get => _processPointerWithOffset; }
        public IntPtr ModulePointerWithOffset { get => _modulePointerWithOffset; }

        const int PROCESS_MEM_READ = 0x0010;
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

        public MemoryReader(string processName, bool is64bits = false, long? processOffset = null,List<int> processOffsetList = null, string moduleName = null, long? moduleOffset = null, List<int> moduleOffsetList = null)
            : this(new TrainerPointer(processName,is64bits, processOffset, processOffsetList, moduleName, moduleOffset, moduleOffsetList))
        { }

        public MemoryReader(TrainerPointer trainerPointer)
        {
            _processName = trainerPointer.ProcessName;
            // Get the Process
            _process = Process.GetProcessesByName(_processName).FirstOrDefault();
            _processPointer = OpenProcess(PROCESS_MEM_READ, false, _process.Id);

            // Get process pointer if we don't use modules
            if (!trainerPointer.UseModule)
            {
                _useModule = false;
                _processPointerWithOffset = trainerPointer.ProcessOffset.HasValue ? (IntPtr)(_process.MainModule.BaseAddress.ToInt64() + (long)trainerPointer.ProcessOffset) : _process.MainModule.BaseAddress;

                // if offset are used, get the correct address through pointer
                if (trainerPointer.UseOffset)
                {
                    foreach (int offset in trainerPointer.OffsetListProcess)
                    {
                        _processPointerWithOffset = trainerPointer.Is64bits ? IntPtr.Add((IntPtr)ReadInt64(0),offset) : IntPtr.Add((IntPtr)ReadInt32(0), offset);
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
                    foreach (int offset in trainerPointer.OffsetListModule)
                    {
                        _modulePointerWithOffset = trainerPointer.Is64bits ? IntPtr.Add((IntPtr)ReadInt64(0), offset) : IntPtr.Add((IntPtr)ReadInt32(0), offset);
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
        /// Read multiple bytes value at memory address offset
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadBytes(ulong addressOffset, ulong length)
        {
            byte[] buffer = new byte[length];
            UIntPtr finalAddress =  _useModule ?
                                            (UIntPtr)((ulong)_modulePointerWithOffset + addressOffset) :
                                            new UIntPtr((ulong)_processPointerWithOffset + addressOffset);
            if (!ReadProcessMemory(_processPointer, finalAddress, buffer, (UIntPtr)length, IntPtr.Zero))
                return null;

            return buffer;
        }

        /// <summary>
        /// Read a single unsigned byte value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte ReadSingleByte(ulong addressOffset) => 
            ReadBytes(addressOffset, 1)[0];

        /// <summary>
        /// Read unsigned 2 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public byte[] Read2Bytes(ulong addressOffset) => 
            ReadBytes(addressOffset, 2);

        /// <summary>
        /// Read unsigned 4 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public byte[] Read4Bytes(ulong addressOffset) => 
            ReadBytes(addressOffset, 4);

        /// <summary>
        /// Read unsigned 8 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public byte[] Read8Bytes(ulong addressOffset) => 
            ReadBytes(addressOffset, 8);

        /// <summary>
        /// Read unsigned 16 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public byte[] Read16Bytes(ulong addressOffset) => 
            ReadBytes(addressOffset, 16);

        /// <summary>
        /// Read a single signed byte value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public sbyte ReadSingleSignedByte(ulong addressOffset) => 
            (sbyte)ReadSingleByte(addressOffset);

        /// <summary>
        /// Read signed 2 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public sbyte[] ReadSigned2Bytes(ulong addressOffset) =>
            (sbyte[]) (Array) Read2Bytes(addressOffset);

        /// <summary>
        /// Read signed 4 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public sbyte[] ReadSigned4Bytes(ulong addressOffset) =>
            (sbyte[])(Array)Read4Bytes(addressOffset);

        /// <summary>
        /// Read signed 8 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public sbyte[] ReadSigned8Bytes(ulong addressOffset) =>
            (sbyte[])(Array)Read8Bytes(addressOffset);

        /// <summary>
        /// Read signed 16 bytes value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public sbyte[] ReadSigned16Bytes(ulong addressOffset) =>
            (sbyte[])(Array)Read16Bytes(addressOffset);

        /// <summary>
        /// Read unsigned int16 value (short)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public ushort ReadInt16(ulong addressOffset) =>
            BitConverter.ToUInt16(Read2Bytes(addressOffset),0);

        /// <summary>
        /// Read unsigned int32 value (int) 
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public uint ReadInt32(ulong addressOffset) =>
            BitConverter.ToUInt32(Read4Bytes(addressOffset), 0);

        /// <summary>
        /// Read unsigned int64 value (long)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public ulong ReadInt64(ulong addressOffset) =>
            BitConverter.ToUInt64(Read8Bytes(addressOffset), 0);

        /// <summary>
        /// Read signed int16 value (short)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public short ReadSignedInt16(ulong addressOffset) =>
            BitConverter.ToInt16(Read2Bytes(addressOffset), 0);

        /// <summary>
        /// Read signed int32 value (int) 
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public int ReadSignedInt32(ulong addressOffset) =>
            BitConverter.ToInt32(Read4Bytes(addressOffset), 0);

        /// <summary>
        /// Read signed int64 value (long)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public long ReadSignedInt64(ulong addressOffset) =>
            BitConverter.ToInt64(Read8Bytes(addressOffset), 0);

        /// <summary>
        /// Read float value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public float ReadFloat(ulong addressOffset,int roundDecimal = 0)
        {
            byte[] buffer = Read4Bytes(addressOffset);
            float value = BitConverter.ToSingle(buffer, 0);
            if (roundDecimal > 0)
                value = (float)Math.Round(value, roundDecimal);
            return value;
        }

        /// <summary>
        /// Read double value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public double ReadDouble(ulong addressOffset, int roundDecimal = 0)
        {
            byte[] buffer = Read8Bytes(addressOffset);
            double value = BitConverter.ToDouble(buffer, 0);
            if (roundDecimal > 0)
                value = Math.Round(value, roundDecimal);
            return value;
        }

        /// <summary>
        /// Read boolean array from byte value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public bool[] ReadBooleanByte(ulong addressOffset)
        {
            bool[] value = new bool[8];
            byte buffer = ReadSingleByte(addressOffset);
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException("Big endian support is not implemented");
            for (int i=0;i<8;i++)
                value[i] = Convert.ToBoolean(buffer & (1 << i));
            return value;
        }

        /// <summary>
        /// Read a single bit value
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="nBit"></param>
        /// <returns></returns>
        public bool ReadSingleBit(ulong addressOffset, byte nBit) =>
            ReadBooleanByte(addressOffset)[nBit];

        /// <summary>
        /// Read string (ASCII coded) value, by length
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string ReadStringASCII(ulong addressOffset, uint length)
        {
            string result = string.Empty;
            byte[] buffer;

            buffer = ReadBytes(addressOffset, length);
            result = Encoding.ASCII.GetString(buffer);

            return result;
        }

        /// <summary>
        /// Read string (ASCII coded) value, until /0 character (string end)
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <returns></returns>
        public string ReadStringASCII(ulong addressOffset)
        {
            string result = string.Empty;
            byte[] buffer;
            List<byte> bytesTmp = new List<byte>();
            bool continu = true;
            byte maxIndex = 0;
            ulong readNb = 0;
            do
            {
                buffer = Read4Bytes(addressOffset + readNb * 4);

                foreach (byte b in buffer)
                {
                    if (b == 0)
                    {
                        continu = false;
                        break;
                    }
                    maxIndex++;
                }
                if (maxIndex == 4)
                {
                    bytesTmp.Concat(buffer.ToList());
                    maxIndex = 0;
                    readNb++;
                }
            }
            while (continu);
            for (int i = 0; i <= maxIndex; i++)
                bytesTmp.Add(buffer[i]);
            result = Encoding.ASCII.GetString(bytesTmp.ToArray());
            return result;
        }

        /// <summary>
        /// Read string (UTF-8 coded) value, by length or by 0 termination
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="length"></param>
        /// <param name="zeroTermination"></param>
        /// <returns></returns>
        public string ReadStringUTF8(ulong addressOffset, uint length, bool zeroTermination)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read string (UTF-16 coded) value, by length or by 0 termination
        /// </summary>
        /// <param name="addressOffset"></param>
        /// <param name="length"></param>
        /// <param name="zeroTermination"></param>
        /// <returns></returns>
        public string ReadStringUnicode(ulong addressOffset, uint length, bool zeroTermination)
        {
            throw new NotImplementedException();
        }

    }
}

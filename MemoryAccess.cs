using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Trainer
{
    public enum AccessType
    {
        ReadOnly,
        WriteOnly,
        ReadWrite
    }

    public class MemoryAccess
    {

        private MemoryReader _memoryReader;
        private MemoryWriter _memoryWriter;

        public MemoryReader MemoryReader { get => _memoryReader; private set => _memoryReader = value; }
        public MemoryWriter MemoryWriter { get => _memoryWriter; private set => _memoryWriter = value; }
        private AccessType AccessType { get; set; }
        //private List<AddressValue> FrozenAddressValues { get; set; }

        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        /// <summary>
        /// Main Constructor, simple and mainly for PC Games or custom emulators with manual process / module name and offsets
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="accessType"></param>
        /// <param name="moduleName"></param>
        /// <param name="offsetProcess"></param>
        /// <param name="offsetModule"></param>
        /*public MemoryAccess(string processName, AccessType accessType, string moduleName = null, long? offsetProcess = null, long? offsetModule = null)
        {
            if (accessType == AccessType.ReadOnly || accessType == AccessType.ReadWrite)
                MemoryReader = new MemoryReader(processName, moduleName, offsetProcess, offsetModule);
            if (accessType == AccessType.WriteOnly || accessType == AccessType.ReadWrite)
                MemoryWriter = new MemoryWriter(processName, moduleName, offsetProcess, offsetModule);
            this.AccessType = accessType;
        }*/

        /// <summary>
        /// Alternative constructor using TrainerPointer instead of raw inputs (for known version 32bits or 64bits)
        /// </summary>
        /// <param name="trainerPointer"></param>
        /// <param name="accessType"></param>
        public MemoryAccess(TrainerPointer trainerPointer, AccessType accessType)
        {
            if (accessType == AccessType.ReadOnly || accessType == AccessType.ReadWrite)
                MemoryReader = new MemoryReader(trainerPointer);
            if (accessType == AccessType.WriteOnly || accessType == AccessType.ReadWrite)
                MemoryWriter = new MemoryWriter(trainerPointer);
            this.AccessType = accessType;
        }

        /// <summary>
        /// Alternative constructor using TrainerPointer dictionary to determine which version use (32bits or 64bits)
        /// </summary>
        /// <param name="trainerPointers"></param>
        /// <param name="baseKey">Base key for dictionary ("MainRAM" for example, without "_64" suffix)</param>
        /// <param name="accessType"></param>
        public MemoryAccess(Dictionary<string, TrainerPointer> trainerPointers,string baseKey, AccessType accessType)
        {
            TrainerPointer tp;
            if (!trainerPointers.ContainsKey(baseKey))
                throw new KeyNotFoundException("The base key must be valid");
#if X64
            bool isWOW64;
            // check if process is WOW64 Process (32bits Process on 64 Bits OS)
            IsWow64Process(System.Diagnostics.Process.GetProcessesByName(trainerPointers[baseKey].ProcessName)[0].Handle, out isWOW64);
            if (isWOW64)
            {
                if (trainerPointers[baseKey].Is64bits)
                    throw new InvalidOperationException("The Trainer Pointer is 64bits only and doesn't support 32 bits. Use 64bits Game/Emulator.");
                else
                    tp = trainerPointers[baseKey];
            }
            else
            {
                // check if 64 bits is compatible
                if (trainerPointers.ContainsKey(baseKey + "_64"))
                    tp = trainerPointers[baseKey + "_64"];
                // if not, use 32 bits
                else
                    tp = trainerPointers[baseKey];
            }
#elif X86
            if (trainerPointers[baseKey].Is64bits)
                throw new InvalidOperationException("The Trainer Pointer is 64bits only. Use 64bits Trainer.dll.");
            tp = trainerPointers[baseKey];
#endif
            if (accessType == AccessType.ReadOnly || accessType == AccessType.ReadWrite)
                MemoryReader = new MemoryReader(tp);
            if (accessType == AccessType.WriteOnly || accessType == AccessType.ReadWrite)
                MemoryWriter = new MemoryWriter(tp);
            this.AccessType = accessType;
        }

        public bool WriteSingleBit(ulong addressOffset, bool value, byte nBit)
        {
            if (this.AccessType != AccessType.ReadWrite)
                throw new InvalidOperationException("The access type must be ReadWrite");
            byte buffer = MemoryReader.ReadSingleByte(addressOffset);
            return MemoryWriter.WriteSingleBit(addressOffset, buffer, value, nBit);
        }
    }
}

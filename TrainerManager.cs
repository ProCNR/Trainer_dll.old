using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trainer
{


    /// <summary>
    /// Trainer Main class, access to memory and memory management classes.
    /// </summary>
    public class TrainerManager : IDisposable
    {
        private MemoryAccess _memoryAccess;

        /// <summary>
        /// Type (platform) of the trainer like PC, PSX, SNES etc...
        /// </summary>
        TrainerType TrainerType { get; set; }
        /// <summary>
        /// Access to the Process RAM
        /// </summary>
        MemoryAccess MemoryAccess { get => _memoryAccess; set => _memoryAccess = value; }
        /// <summary>
        /// Pointer to the process RAM
        /// </summary>
        Dictionary<string,TrainerPointer> TrainerPointers { get; set; }
        /// <summary>
        /// RAM Watcher with a list of value to read, to write and to freeze
        /// </summary>
        public RAMWatch RAMWatch { get; private set; }


        /// <summary>
        /// Main Constructor (for PC Games and custom emulators)
        /// </summary>
        /// <param name="trainerPointer"></param>
        /// <param name="accessType"></param>
        public TrainerManager(TrainerPointer trainerPointer, AccessType accessType,WatchMode watchMode)
        {
            //this.TrainerPointers = trainerPointer;
            this.MemoryAccess = new MemoryAccess(trainerPointer, accessType);
            this.RAMWatch = new RAMWatch(ref _memoryAccess, watchMode);
        }

        /// <summary>
        /// Another Constructor for single emulator platform like PS2/PCSX2 or PS3/RPCS3
        /// </summary>
        /// <param name="trainerType"></param>
        /// <param name="accessType"></param>
        public TrainerManager(TrainerType trainerType, AccessType accessType, WatchMode watchMode)
        {
            this.TrainerPointers = trainerType.GetEmulatorPointer();
            this.MemoryAccess = new MemoryAccess(TrainerPointers["MainRAM"], accessType);

        }

        /// <summary>
        /// Another Constructor for multiple emulator platform like PSX
        /// </summary>
        /// <param name="trainerType"></param>
        /// <param name="accessType"></param>
        /// <param name="psxEmulator"></param>
        /// <param name="nesEmulator"></param>
        /// <param name="snesEmulator"></param>
        /// <param name="n64Emulator"></param>
        /// <param name="smsEmulator"></param>
        /// <param name="genesisEmulator"></param>
        public TrainerManager(TrainerType trainerType, AccessType accessType, WatchMode watchMode,
                                 PSXEmulator psxEmulator = PSXEmulator.Null,
                                 NESEmulator nesEmulator = NESEmulator.Null,
                                 SNESEmulator snesEmulator = SNESEmulator.Null,
                                 N64Emulator n64Emulator = N64Emulator.Null,
                                 SMSEmulator smsEmulator = SMSEmulator.Null,
                                 GenesisEmulator genesisEmulator = GenesisEmulator.Null)
        {
            switch (trainerType)
            {
                case TrainerType.PSX:
                    if (psxEmulator != PSXEmulator.Null)
                        this.TrainerPointers = trainerType.GetEmulatorPointer(psxEmulator: psxEmulator);
                    else
                        throw new ArgumentException("PSX emulator should be defined, for currently not supported emulator use the main constructor");
                    break;
                case TrainerType.NES:
                    if (nesEmulator != NESEmulator.Null)
                        this.TrainerPointers = trainerType.GetEmulatorPointer(nesEmulator: nesEmulator);
                    else
                        throw new ArgumentException("NES emulator should be defined, for currently not supported emulator use the main constructor");
                    break;
                case TrainerType.SNES:
                    if (snesEmulator != SNESEmulator.Null)
                        this.TrainerPointers = trainerType.GetEmulatorPointer(snesEmulator: snesEmulator);
                    else
                        throw new ArgumentException("SNES emulator should be defined, for currently not supported emulator use the main constructor");
                    break;
                case TrainerType.N64:
                    if (n64Emulator != N64Emulator.Null)
                        this.TrainerPointers = trainerType.GetEmulatorPointer(n64Emulator: n64Emulator);
                    else
                        throw new ArgumentException("N64 emulator should be defined, for currently not supported emulator use the main constructor");
                    break;
                case TrainerType.SMS:
                    if (smsEmulator != SMSEmulator.Null)
                        this.TrainerPointers = trainerType.GetEmulatorPointer(smsEmulator: smsEmulator);
                    else
                        throw new ArgumentException("SMS emulator should be defined, for currently not supported emulator use the main constructor");
                    break;
                case TrainerType.Genesis:
                    if (genesisEmulator != GenesisEmulator.Null)
                        this.TrainerPointers = trainerType.GetEmulatorPointer(genesisEmulator: genesisEmulator);
                    else
                        throw new ArgumentException("Genesis emulator should be defined, for currently not supported emulator use the main constructor");
                    break;
                default:
                    throw new ArgumentException();
            }
            this.MemoryAccess = new MemoryAccess(TrainerPointers["MainRAM"], accessType);
            this.RAMWatch = new RAMWatch(ref _memoryAccess, watchMode);
        }

        public RAMValue GetValue(string key)
        {
            return this.RAMWatch.GetRAMValue(key);
        }

        public void Dispose()
        {
            this.RAMWatch = null;
            this.MemoryAccess = null;
        }
    }
}

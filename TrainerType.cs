using System;
using System.Collections.Generic;

namespace Trainer
{
    public enum TrainerType
    {
        PSX,
        PS2,
        PS3,
        PC,
        NES,
        SNES,
        N64,
        SMS,
        Genesis
    }

    public enum PSXEmulator
    {
        Null,
        pSXfin,
        PSXjin,
        ePSXe_1_5_2,
        ePSXe_1_9_0,
        ePSXe_2_0_5,
        pcsxr,
        mednafen,
        BizHawk_2_3_2,
        BizHawk_1_13_2,
        retroarch_beetle
    }

    public enum NESEmulator
    {
        Null,
        Higan,
        puNES,
        Nestopia,
        BizHawk,
        Mesen,
        FCEUX
    }

    public enum SNESEmulator
    {
        Null,
        bsnes,
        bsnes_plus,
        BizHawk,
        Higan,
        Snes9x,
        ZSNES
    }

    public enum N64Emulator
    {
        Null,
        emu_1964,
        Project64,
        Mupen64Plus
    }

    public enum SMSEmulator
    {
        Null,
        BizHawk,
        Higan,
        KegaFusion,
        Mednafen
    }

    public enum GenesisEmulator
    {
        Null,
        BizHawk,
        Higan,
        Gens,
        KegaFusion
    }

    public static class TrainerTypeMethods
    {

        /* * * * * * * * * * 
         * Common Methods
         * * * * * * * * * */
        #region Common Methods

        public static Dictionary<string, TrainerPointer> GetEmulatorPointer(this TrainerType trainerType, bool? is64bits = null,
                                                        PSXEmulator psxEmulator = PSXEmulator.Null,
                                                        NESEmulator nesEmulator = NESEmulator.Null,
                                                        SNESEmulator snesEmulator = SNESEmulator.Null,
                                                        N64Emulator n64Emulator = N64Emulator.Null,
                                                        SMSEmulator smsEmulator = SMSEmulator.Null,
                                                        GenesisEmulator genesisEmulator = GenesisEmulator.Null) =>
            trainerType switch
            {
                TrainerType.PSX     => GetPSXEmulatorOffset(psxEmulator, is64bits),
                //TrainerType.PS2     => new TrainerPointer("pcsx2", 0x0), // todo
                //TrainerType.PS3     => new TrainerPointer("rpcsx3", 0x0), // todo
                //TrainerType.NES     => GetNESEmulatorOffset(nesEmulator),
                //TrainerType.SNES    => GetSNESEmulatorOffset(snesEmulator),
                //TrainerType.N64   => GetN64EmulatorOffset(n64Emulator), //todo
                //TrainerType.SMS     => GetSMSEmulatorOffset(smsEmulator),
                //TrainerType.Genesis => GetGenesisEmulatorOffset(genesisEmulator),
                _                   => null
            };

        #endregion


        /* * * * * * * * * * 
         * PSX Methods
         * * * * * * * * * */
        #region PSX Methods

        private static Dictionary<string,TrainerPointer> GetPSXEmulatorOffset(PSXEmulator psxEmulator, bool? is64bits = null) =>
            psxEmulator switch
            {
                //PSXEmulator.pSXfin      => new TrainerPointer("psxfin", 0x00171A5C),
                //PSXEmulator.ePSXe_1_5_2 => new TrainerPointer("ePSXe", 0x00175BA0),
                //PSXEmulator.ePSXe_1_9_0 => new TrainerPointer("ePSXe", 0x00019D48),
                //PSXEmulator.ePSXe_2_0_5 => new TrainerPointer("ePSXe", 0x00032378),
                //PSXEmulator.pcsxr       => new TrainerPointer("pcsxr", 0x00095144),
                //PSXEmulator.mednafen    => new TrainerPointer("mednafen", 0x00000000), // to edit


                // pSXfin is 32 Bits only
                PSXEmulator.pSXfin => new Dictionary<string, TrainerPointer>
                {
                    { "MainRAM",new TrainerPointer("psxfin", 0x200000, processOffset: 0x00171A5C, processOffsetList: new List<int> { 0 }) }
                    //{ "GPURAM", new TrainerPointer("psxfin", 0x100000, processOffset: ((long?)0x0029ECF0-0x005D6F14)) }//,  TODO: Find GPU RAM Pointer
                },

                // PSXjin if 32 Bits only
                PSXEmulator.PSXjin => new Dictionary<string, TrainerPointer>
                {
                    { "MainRAM",new TrainerPointer("psxjin", 0x200000, processOffset: 0x0018412C) }
                    //{ "GPURAM",new TrainerPointer("psxjin",0x100000, processOffset:0x00) }  TODO: Find GPU RAM Pointer
                },

                // BizHawk 2.3.2 is 64 Bits only
                PSXEmulator.BizHawk_2_3_2 => new Dictionary<string, TrainerPointer>
                {
                    { "MainRAM",    new TrainerPointer("EmuHawk", 0x200000, is64bits:true, moduleName: "octoshock.dll", moduleOffset: 0x0011D880) },  // to simplify usage since there is only 64bits
                    { "MainRAM_64", new TrainerPointer("EmuHawk", 0x200000, is64bits:true, moduleName: "octoshock.dll", moduleOffset: 0x0011D880) },
                    { "GPURAM",     new TrainerPointer("EmuHawk", 0x100000, is64bits:true, moduleName: "octoshock.dll", moduleOffset: 0x002EE96C) },  // to simplify usage since there is only 64bits
                    { "GPURAM_64",  new TrainerPointer("EmuHawk", 0x100000, is64bits:true, moduleName: "octoshock.dll", moduleOffset: 0x002EE96C) }, 
                },

                // BizHawk 1.13.2 is 32 Bits only
                PSXEmulator.BizHawk_1_13_2 => new Dictionary<string, TrainerPointer>
                {
                    { "MainRAM",new TrainerPointer("EmuHawk",0x200000, moduleName: "octoshock.dll", moduleOffset: 0x000EC4A8) },
                    { "GPURAM", new TrainerPointer("EmuHawk",0x100000, moduleName: "octoshock.dll", moduleOffset: 0x002BD594) }
                },
                _  => null
            };

        #endregion


        /* * * * * * * * * * 
         * NES Methods
         * * * * * * * * * */
        #region NES Methods

        private static TrainerPointer GetNESEmulatorOffset(NESEmulator nesEmulator) =>
            nesEmulator switch
            {
                NESEmulator.FCEUX    => new TrainerPointer("fceux", 0x0), // todo
                NESEmulator.puNES    => new TrainerPointer("punes", 0x0), // todo
                NESEmulator.Nestopia => new TrainerPointer("nestopia", 0x0), // todo
                NESEmulator.Mesen    => new TrainerPointer("mesen", 0x0), // todo
                NESEmulator.Higan    => new TrainerPointer("higan", 0x0), // todo
                NESEmulator.BizHawk  => new TrainerPointer("bizhawk", 0x0), // todo
                _                    => null
            };

        #endregion


        /* * * * * * * * * * 
         * SNES Methods
         * * * * * * * * * */
        #region SNES Methods

        private static TrainerPointer GetSNESEmulatorOffset(SNESEmulator snesEmulator) =>
            snesEmulator switch
            {
                SNESEmulator.bsnes      => new TrainerPointer("bsnes", 0x0), // todo
                SNESEmulator.bsnes_plus => new TrainerPointer("bsnes_plus", 0x0), // todo
                SNESEmulator.Snes9x     => new TrainerPointer("Snes9x", 0x0), // todo
                SNESEmulator.ZSNES      => new TrainerPointer("ZSNES", 0x0), // todo               
                SNESEmulator.Higan      => new TrainerPointer("Higan", 0x0), // todo
                SNESEmulator.BizHawk    => new TrainerPointer("BizHawk", 0x0), // todo
                _                       => null
            };

        #endregion


        /* * * * * * * * * * 
         * SMS Methods
         * * * * * * * * * */
        #region Sega Master System Methods

        private static TrainerPointer GetSMSEmulatorOffset(SMSEmulator smsEmulator) =>
            smsEmulator switch
            {
                SMSEmulator.KegaFusion  => new TrainerPointer("kegafusion", 0x0), // todo
                SMSEmulator.Mednafen    => new TrainerPointer("mednafen", 0x0), // todo
                SMSEmulator.Higan       => new TrainerPointer("higan", 0x0), // todo
                SMSEmulator.BizHawk     => new TrainerPointer("bizhawk", 0x0), // todo
                _                       => null
            };

        #endregion


        /* * * * * * * * * * 
         * Genesis/MegaDrive Methods
         * * * * * * * * * */
        #region Genesis/Megadrive Methods

        private static TrainerPointer GetGenesisEmulatorOffset(GenesisEmulator genesisEmulator) =>
            genesisEmulator switch
            {
                GenesisEmulator.KegaFusion  => new TrainerPointer("kegafusion", 0x0), // todo
                GenesisEmulator.Gens        => new TrainerPointer("gens", 0x0), // todo
                GenesisEmulator.Higan       => new TrainerPointer("higan", 0x0), // todo
                GenesisEmulator.BizHawk     => new TrainerPointer("bizhawk", 0x0), // todo
                _                           => null
            };

        #endregion




    }
}

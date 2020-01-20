using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trainer
{
    public class TrainerPointer
    {
        private readonly bool _is64bits;
        #region Class members
        /// <summary>
        /// Name of process to hook
        /// </summary>
        public string ProcessName { get; private set; }

        /// <summary>
        /// Name of the Module to hook
        /// </summary>
        public string? ModuleName { get; private set; }

        /// <summary>
        /// Offset 0 for Process pointer (for exemple "psxfin.exe"+00171A5C)
        /// </summary>
        public long? ProcessOffset { get; private set; }

        /// <summary>
        /// Offset 0 for Module pointer (for exemple "octoshock.dll"+0011D880)
        /// </summary>
        public long? ModuleOffset { get; private set; }

        /// <summary>
        /// Size of the RAM section (useful for Emulator)
        /// </summary>
        public ulong? RAMSize { get; private set; }

        /// <summary>
        /// Offset 1->n for multiple offset pointer (Process)
        /// </summary>
        public List<int> OffsetListProcess { get; private set; }

        /// <summary>
        /// Offset 1->n for multiple offset pointer (Module)
        /// </summary>
        public List<int> OffsetListModule { get; private set; }

        public bool Is64bits { get => _is64bits;}

        /// <summary>
        /// The process has modules
        /// </summary>
        public bool UseModule { get; private set; }

        /// <summary>
        /// Check if offset list is used
        /// </summary>
        public bool UseOffset { get => UseModule ? this.OffsetListModule != null && this.OffsetListModule.Count > 0 : this.OffsetListProcess != null && this.OffsetListProcess.Count > 0; }

        #endregion

        /// <summary>
        /// Main Constructor
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="is64bits"></param>
        /// <param name="processOffset"></param>
        /// <param name="processOffsetList"></param>
        /// <param name="moduleName"></param>
        /// <param name="moduleOffset"></param>
        /// <param name="moduleOffsetList"></param>
        public TrainerPointer(string processName, bool is64bits = false, long? processOffset = null, List<int> processOffsetList = null, string moduleName = null, long? moduleOffset = null, List<int> moduleOffsetList = null)
        {
            _is64bits = is64bits;
            this.ProcessName = processName;
            if (UseModule = moduleName != null)
            {
                this.ModuleName = moduleName;
                this.ModuleOffset = moduleOffset;
                this.OffsetListModule = moduleOffsetList;
            }

            this.ProcessOffset = processOffset;
            this.OffsetListProcess = processOffsetList;
        }

        /// <summary>
        /// Alternative Constructeur for Emulator
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="ramSize"></param>
        /// <param name="is64bits"></param>
        /// <param name="processOffset"></param>
        /// <param name="processOffsetList"></param>
        /// <param name="moduleName"></param>
        /// <param name="moduleOffset"></param>
        /// <param name="moduleOffsetList"></param>

        public TrainerPointer(string processName, ulong ramSize, bool is64bits = false, long? processOffset = null, List<int> processOffsetList = null, string moduleName = null, long? moduleOffset = null, List<int> moduleOffsetList = null)
        {
            _is64bits = is64bits;
            this.ProcessName = processName;
            if (UseModule = moduleName != null)
            {
                this.ModuleName = moduleName;
                this.ModuleOffset = moduleOffset;
                this.OffsetListModule = moduleOffsetList;
            }
            if (ramSize > 0)
                this.RAMSize = ramSize;
            this.ProcessOffset = processOffset;
            this.OffsetListProcess = processOffsetList;
        }
    }
}

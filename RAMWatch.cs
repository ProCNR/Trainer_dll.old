using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trainer
{

    public enum WatchMode
    {
        SingleThreaded,
        MultiThreaded_A,
        MultiThreaded_N,
        MultiThreaded_S
    }

    public class RAMWatch
    {

        /// <summary>
        /// Single Watch list (2 threads: Read / Write / Frozen  + Enqueue)
        /// </summary>
        List<RAMValue> WatchList { get; set; }

        /// <summary>
        /// Multiple Watch List (4/n threads: 1/n Read + 1/n Write + 1/n Frozen + 1 Enqueue) TODO!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        ConcurrentDictionary<string,List<RAMValue>> ThreadedWatchLists { get; set; }

        /// <summary>
        /// Read (Update) WatchList (for MultiThreaded_A Mode) [To be replaced by ThreadedWatchLists when implemented]
        /// </summary>
        List<RAMValue> ReadWatchList { get; set; }

        /// <summary>
        /// Write (Poke) WatchList (for MultiThreaded_A Mode) [To be replaced by ThreadedWatchLists when implemented]
        /// </summary>
        List<RAMValue> WriteWatchList { get; set; }

        /// <summary>
        /// Freeze WatchList (for MultiThreaded_A Mode) [To be replaced by ThreadedWatchLists when implemented]
        /// </summary>
        List<RAMValue> FreezeWatchList { get; set; }

        /// <summary>
        /// Queue of RAM Value Type
        /// </summary>
        ConcurrentQueue<RAMValue> RAMValueQueue { get; set; }

        /// <summary>
        /// List of threads
        /// </summary>
        Dictionary<string,Thread> _threads;

        /// <summary>
        /// Memory Reader
        /// </summary>
        MemoryReader _memoryReader;
        
        /// <summary>
        /// Memory Writer
        /// </summary>
        MemoryWriter _memoryWriter;


        // Break and End for all threads
        private bool _breakGlobal;
        private bool _endGlobal;

        // Break and End of Thread READ (TO DO)
        private bool _breakRead;
        private bool _endRead;

        // Break and End of Thread WRITE (TO DO)
        private bool _breakWrite;
        private bool _endWrite;

        // Break and End of Thread FREEZE (TO DO)
        private bool _breakFreeze;
        private bool _endFreeze;

        private WatchMode _watchMode;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="memoryAccess"></param>
        public RAMWatch(ref MemoryAccess memoryAccess,WatchMode watchMode,int dispatchTimeout = 1000)
        {
            _memoryReader = memoryAccess.MemoryReader;
            _memoryWriter = memoryAccess.MemoryWriter;
     
            _threads = new Dictionary<string, Thread>();
            _watchMode = watchMode;

            RAMValueQueue = new ConcurrentQueue<RAMValue>();

            switch (_watchMode)
            {
                // Single Thread
                case WatchMode.SingleThreaded:
                    WatchList = new List<RAMValue>();
                    _threads.Add("rwf_0", new Thread(new ThreadStart(delegate () { SingleThreadWatch(); })));
                    _threads["rwf_0"].Start();
                    break;
                // Multi Thread (1 per action)
                case WatchMode.MultiThreaded_A:

                    ReadWatchList = new List<RAMValue>();
                    WriteWatchList = new List<RAMValue>();
                    FreezeWatchList = new List<RAMValue>();

                    _threads.Add("read_0", new Thread(new ThreadStart(delegate () { ThreadedWatchRead(); })));
                    _threads.Add("write_0", new Thread(new ThreadStart(delegate () { ThreadedWatchWrite(); })));
                    _threads.Add("freeze_0", new Thread(new ThreadStart(delegate () { ThreadedWatchFreeze(); })));
                    break;

                // Multi Thread (N Thread per action)
                case WatchMode.MultiThreaded_N:
                    // TODO
                    break;

                // Multi Thread (N Single Thread)
                case WatchMode.MultiThreaded_S:
                    // TODO
                    break;
            }

            // Start Queue Manager thread
            _threads.Add("queue_0", new Thread(new ThreadStart(delegate () { Dispatch(dispatchTimeout); })));
            _threads["queue_0"].Start();
        }

        /// <summary>
        /// Add RAM Value Type to the watch list
        /// </summary>
        /// <param name="rvt"></param>
        public void Add(RAMValue rvt)
        {
            RAMValueQueue.Enqueue(rvt);
        }

        /// <summary>
        /// Add RAM Value Type to the watch list
        /// </summary>
        /// <param name="rvt"></param>
        public void Add(Dictionary<string,RAMValue> rvtDict)
        {
            foreach(var kvp in rvtDict)
            {
                Add(kvp.Value);
            }
        }

        /// <summary>
        /// Get RAM Value Type using address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public RAMValue GetRAMValue(UIntPtr address) => WatchList.Where(p => p.Address == address).ToList().FirstOrDefault();

        public RAMValue GetRAMValue(string key)
        {
            switch(_watchMode)
            {
                case WatchMode.SingleThreaded:
                    return WatchList.Where(p => p.Key == key).SingleOrDefault();
                case WatchMode.MultiThreaded_A:
                    List<RAMValue> tmpList = ReadWatchList.Union(WriteWatchList).Union(FreezeWatchList).ToList();
                    return tmpList.Where(p => p.Key == key).SingleOrDefault();
                case WatchMode.MultiThreaded_N:
                    return null;
                case WatchMode.MultiThreaded_S:
                    return null;
            }
            return null;
        }

        /// <summary>
        /// Threads start
        /// </summary>
        public void RAMWatchStartThreads()
        {
            if (_watchMode == WatchMode.SingleThreaded)
                throw new InvalidOperationException("RAMWatchStartThreads methods is not allowed on single threaded watch mode");

            _threads["read_0"].Start();
            _threads["write_0"].Start();
            _threads["freeze_0"].Start();
        }

        /// <summary>
        /// Add the value from the queue in the correct watch list
        /// </summary>
        /// <param name="dispatchTimeout"></param>
        private void Dispatch(int dispatchTimeout)
        {
            RAMValue rvt;
            while(!_endGlobal)
            {
                while(!_endGlobal && !_breakGlobal)
                {
                    if (RAMValueQueue.Count > 0)
                    {

                        switch(_watchMode)
                        {
                            // MONO THREAD MODE
                            case WatchMode.SingleThreaded:
                                while (!Monitor.TryEnter(WatchList))
                                {
                                    Thread.Sleep(1);
                                }
                                while (RAMValueQueue.Count > 0)
                                {
                                    if(RAMValueQueue.TryDequeue(out rvt))
                                        WatchList.Add(rvt);
                                }
                                Monitor.Exit(WatchList); 
                                break;


                            // MULTI THREAD MODE 1 ( 1 THREAD PER ACTION )
                            case WatchMode.MultiThreaded_A:
                                while (RAMValueQueue.Count > 0)
                                {
                                    RAMValueQueue.TryDequeue(out rvt);
                                    if (rvt.Update)
                                    {
                                        while (!Monitor.TryEnter(ThreadedWatchLists["read_0"]))
                                        {
                                            Thread.Sleep(1);
                                        }
                                        ThreadedWatchLists["read_0"].Add(rvt);
                                    }
                                        
                                    if (rvt.Write)
                                        ThreadedWatchLists["write_0"].Add(rvt);
                                    if (rvt.Freeze)
                                        ThreadedWatchLists["freeze_0"].Add(rvt);
                                }
                                break;

                            // MULTI THREAD MODE N ( N THREAD PER ACTION )
                            case WatchMode.MultiThreaded_N:
                                //TODO
                                break;

                            // MULTI THREAD MODE S ( N SINGLE THREAD )
                            case WatchMode.MultiThreaded_S:
                                //TODO
                                break;
                        }
                    }
                    
                    Thread.Sleep(dispatchTimeout);
                }
            }
        }

        private void SingleThreadWatch(int individualTimeout = 0,int globalTimeout = 167, int breakTimeout = 2000)
        {
            while(!_endGlobal)
            {
                while (!_endGlobal && !_breakGlobal)
                {
                    foreach (RAMValue rvt in WatchList)
                    {
                        while (!Monitor.TryEnter(WatchList))
                        {
                            Thread.Sleep(1);
                        }
                        // Read (Update)
                        if (rvt.Update)
                            rvt.ReadValue(ref _memoryReader);
                        // Write (Poke)
                        if (rvt.Write)
                        {
                            rvt.WriteValue(ref _memoryWriter);
                            WatchList.Remove(rvt);
                        }
                        // Freeze
                        if(rvt.Freeze)
                            rvt.WriteValue(ref _memoryWriter);

                        Monitor.Exit(WatchList);
                        Thread.Sleep(individualTimeout);
                    }
                    Thread.Sleep(globalTimeout);
                }
                Thread.Sleep(breakTimeout);
            }
        }

        /// <summary>
        /// Read Values (Update)
        /// </summary>
        /// <param name="individualReadTimeout"></param>
        /// <param name="globalReadTimeout"></param>
        /// <param name="breakTimeout"></param>
        private void ThreadedWatchRead(int individualReadTimeout = 0, int globalReadTimeout = 167, int breakTimeout = 2000)
        {
            _breakRead = false;
            _endRead = false;

            while (!_endRead && !_endGlobal)
            {
                while (!_breakRead && !_endRead && !_endGlobal)
                {
                    foreach(RAMValue rvt in ReadWatchList)
                    {
                        while (!Monitor.TryEnter(ReadWatchList))
                        {
                            Thread.Sleep(1);
                        }
                        rvt.ReadValue(ref _memoryReader);
                        Monitor.Exit(ReadWatchList);
                        Thread.Sleep(individualReadTimeout);
                    }
                    Thread.Sleep(globalReadTimeout);
                }
                Thread.Sleep(breakTimeout);
            }
        }

        /// <summary>
        /// Write Values
        /// </summary>
        /// <param name="individualWriteTimeout"></param>
        /// <param name="globalWriteTimeout"></param>
        /// <param name="breakTimeout"></param>
        private void ThreadedWatchWrite(int individualWriteTimeout = 0, int globalWriteTimeout = 167, int breakTimeout = 2000)
        {
            _breakWrite = false;
            _endWrite = false;

            while (!_endWrite && !_endGlobal)
            {
                while (!_breakWrite && !_endWrite && !_endGlobal)
                {
                    foreach (RAMValue rvt in WriteWatchList)
                    {
                        while (!Monitor.TryEnter(WriteWatchList))
                        {
                            Thread.Sleep(1);
                        }

                        if (rvt.WriteValue(ref _memoryWriter))
                            RemoveWatch("WriteWatchList",rvt);

                        Monitor.Exit(WriteWatchList);
                        Thread.Sleep(individualWriteTimeout);
                    }
                    Thread.Sleep(globalWriteTimeout);
                }
                Thread.Sleep(breakTimeout);
            }
        }

        /// <summary>
        /// Freeze Values (Keeps Writing)
        /// </summary>
        /// <param name="individualFreezeTimeout"></param>
        /// <param name="globalFreezeTimeout"></param>
        /// <param name="breakTimeout"></param>
        private void ThreadedWatchFreeze(int individualFreezeTimeout = 0, int globalFreezeTimeout = 167, int breakTimeout = 2000)
        {
            _breakFreeze = false;
            _endFreeze = false;

            while (!_endFreeze && ! _endGlobal)
            {
                while (!_breakFreeze && !_endFreeze && !_endGlobal)
                {
                    foreach (RAMValue rvt in FreezeWatchList)
                    {
                        rvt.WriteValue(ref _memoryWriter);
                        Thread.Sleep(individualFreezeTimeout);                   
                    }
                    Thread.Sleep(globalFreezeTimeout); 
                }
                Thread.Sleep(breakTimeout);
            }
        }

        /// <summary>
        /// Stop threads
        /// </summary>
        public void StopThreads()
        {
            _breakGlobal = _endGlobal = true;
            foreach (var kvp in _threads)
            {
                kvp.Value.Join();
            }
        }

        private void RemoveWatch(string wlName, RAMValue rvt)
        {
            rvt.RemoveKey();
            if (wlName == "ReadWatchList")
                ReadWatchList.Remove(rvt);
            else if (wlName == "WriteWatchList")
                WriteWatchList.Remove(rvt);
            else if (wlName == "FreezeWatchList")
                FreezeWatchList.Remove(rvt);
            else if (wlName == "ALL")
            {
                ReadWatchList.Remove(rvt);
                WriteWatchList.Remove(rvt);
                FreezeWatchList.Remove(rvt);
            }
                
        }
    }
}

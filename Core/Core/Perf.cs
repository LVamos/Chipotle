using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Perf : DebugSO
    {
        public static readonly PerfInfo ClientInput = new PerfInfo(5);
        public static readonly PerfInfo ClientTick = new PerfInfo(100);
        public static readonly PerfInfo InputToSoundPipeline = new PerfInfo(1);
        public static readonly PerfInfo SimInput = new PerfInfo(5);
        public static readonly PerfInfo SimTick = new PerfInfo(100);
        public static readonly PerfInfo SoundTick = new PerfInfo(100);
        public static SystemPerfInfo ClientSystem = new SystemPerfInfo();
        public static SystemPerfInfo SimSystem = new SystemPerfInfo();
        public static SystemPerfInfo SoundSystem = new SystemPerfInfo();
        public static SystemPerfInfo SpeechSystem = new SystemPerfInfo();
        private static int[] _gcCounts;
        private static bool _gcWatcherRunning;
        private static bool _running;
        private static StreamWriter _streamWriter;
        private static ManualResetEvent _waitEvent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="elapsedMS"></param>
        /// <returns></returns>
        public static bool RecordMethod(PerfInfo pi, double elapsedMS)
        { 
            // returns true if RunsToSkip was 0, and so the recording actually occurred.
            if (!_running)
                return false;
            // we can skip a number of initial runs to ensure things get jitted.
            if (pi.RunsToSkip > 0)
            {
                pi.RunsToSkip--;
                return false;
            }
            pi.LastTime = elapsedMS;
            pi.NumberOfRuns++;
            pi.TotalTime += elapsedMS;
            if (elapsedMS < pi.Shortest)
                pi.Shortest = elapsedMS;
            if (elapsedMS > pi.Longest)
                pi.Longest = elapsedMS;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void StartTiming()
        {
            if (_running)
                return; // already enabled
            _running = true;
            _waitEvent = new ManualResetEvent(false);
            var t = new Thread(() =>
            {
                var sw = Stopwatch.StartNew();
                int msToSleep = 10000;
                // we use a wait event so if StopTiming is called we can close our thread immediately instead of waiting the rest of the 10 seconds.
                _waitEvent.WaitOne(msToSleep);
                while (_running)
                {
                    int seconds = (int)Math.Round(sw.Elapsed.TotalSeconds);
                    WriteSystemsPerf(seconds);
                    int msAlreadyPassed = (int)Math.Floor(sw.Elapsed.TotalMilliseconds) % msToSleep;
                    _waitEvent.WaitOne(msToSleep - msAlreadyPassed);
                } // end while running
                _waitEvent.Dispose();
                _waitEvent = null;
            });
            t.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void StopTiming()
        {
            _running = false;
            _waitEvent.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void StopWatchingGC()
        { _gcWatcherRunning = false; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="spi"></param>
        /// <param name="command"></param>
        public static void TimeIt(PerfInfo pi, SystemPerfInfo spi, Action command)
        { 
            // a big benefit of this method is that by wrapping the command in a closure, we ensure we time the runs that break out early with a return keyword.
            var sw = Stopwatch.StartNew();
            command();
            double elapsedMS = sw.Elapsed.TotalMilliseconds;
            if (pi != null)
                RecordMethod(pi, elapsedMS);
            if (spi != null)
                RecordSystem(spi, elapsedMS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pi"></param>
        /// <param name="spi"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static T TimeIt<T>(PerfInfo pi, SystemPerfInfo spi, Func<T> command)
        { 
            // a big benefit of this method is that by wrapping the command in a closure, we ensure we time the runs that break out early with a return keyword.
            var sw = Stopwatch.StartNew();
            var returnValue = command();
            double elapsedMS = sw.Elapsed.TotalMilliseconds;
            if (pi != null)
                RecordMethod(pi, elapsedMS);
            if (spi != null)
                RecordSystem(spi, elapsedMS);
            return returnValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void WatchGC()
        {
            if (_gcWatcherRunning)
                throw new Exception("Called WatchGC while it was already running.");
            _gcWatcherRunning = true;
            _gcCounts = new int[GC.MaxGeneration];
            var t = new Thread(() =>
                {
                    while (_gcWatcherRunning)
                    {
                        for (int i = 0; i < GC.MaxGeneration; i++)
                        {
                            int count = GC.CollectionCount(i);
                            if (count != _gcCounts[i])
                            {
                                _gcCounts[i]++; // we increment instead of assigning so if 2 collections occurred quickly, they will both get reported.
                                Say("Generation {0} GC occurred.", i);
                            }
                        } // end for loop
                        Thread.Sleep(10);
                    } // end while running
                });
            t.Start();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        public static void WriteSystemsPerf(int seconds)
        {
            if (_streamWriter == null)
            {
                Directory.CreateDirectory(DataPath + "Logs");
                _streamWriter = new StreamWriter(DataPath + @"logs\Systems perf.log");
            }
            _streamWriter.WriteLine("## {0} seconds", seconds);
            WritePerfLine("client input", ClientInput);
            WritePerfLine("client tick", ClientTick);
            WritePerfLine("sim input", SimInput);
            WritePerfLine("sim tick", SimTick);
            WritePerfLine("sound tick", SoundTick);
            WritePerfLine("input to sound pipeline", InputToSoundPipeline);

            double total = SpeechSystem.TotalTime + SoundSystem.TotalTime + ClientSystem.TotalTime + SimSystem.TotalTime;
            double speechPercent = Math.Round(100 * SpeechSystem.TotalTime / total);
            double soundPercent = Math.Round(100 * SoundSystem.TotalTime / total);
            double clientPercent = Math.Round(100 * ClientSystem.TotalTime / total);
            double simPercent = Math.Round(100 * SimSystem.TotalTime / total);
            int totalPerSecond = (int)Math.Round(total / seconds);
            _streamWriter.WriteLine("{0}% sim, {1}% sound, {2}% client, {3}% speech, {4}ms total per second", simPercent, soundPercent, clientPercent, speechPercent, totalPerSecond);

            int simMS = (int)Math.Round(SimSystem.TotalTime / seconds);
            int clientMS = (int)Math.Round(ClientSystem.TotalTime / seconds);
            int soundMS = (int)Math.Round(SoundSystem.TotalTime / seconds);
            int speechMS = (int)Math.Round(SpeechSystem.TotalTime / seconds);
            _streamWriter.WriteLine("{0}ms sim, {1}ms sound, {2}ms client, {3}ms speech, {4}ms total per second", simMS, soundMS, clientMS, speechMS, totalPerSecond);

            _streamWriter.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spi"></param>
        /// <param name="elapsedMS"></param>
        private static void RecordSystem(SystemPerfInfo spi, double elapsedMS)
        {
            if (!_running)
                return;
            spi.TotalTime += elapsedMS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="pi"></param>
        private static void WritePerfLine(string systemName, PerfInfo pi)
        {
            var average = pi.GetAverageTime();
            if (average < .05 || double.IsNaN(average))
                return; // don't log the method if it is taking less than 50 microseconds to complete.
            _streamWriter.WriteLine("{0} {1}", systemName, pi.GetMicrosecondsMessage());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="spi"></param>
        /// <param name="seconds"></param>
        private static void WriteSystemPerfLine(string systemName, SystemPerfInfo spi, int seconds)
          =>  _streamWriter.WriteLine("{0} {1}ms per second", systemName, Math.Round(spi.TotalTime / seconds)); 
    } // cls

    /// <summary>
    /// 
    /// </summary>
    public sealed class PerfInfo
    { // all times are in milliseconds
        public double LastTime;
        public double Longest;
        public int NumberOfRuns;
        public double Shortest = double.MaxValue;
        public double TotalTime;
        internal int RunsToSkip;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="initialRunsToSkip"></param>
        public PerfInfo(int initialRunsToSkip)
        => this.RunsToSkip = initialRunsToSkip; 

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetAverageTime()
        => TotalTime / NumberOfRuns; 

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetLastMicrosecondsMessage()
        => String.Format("{0} microseconds, {1} average over {2} runs, shortest is {3}, longest is {4}", Math.Round(LastTime * 1000), Math.Round(GetAverageTime() * 1000), NumberOfRuns, Math.Round(Shortest * 1000), Math.Round(Longest * 1000)); 

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetMicrosecondsMessage()
        => String.Format("{0} microseconds average over {1} runs, shortest is {2}, longest is {3}", Math.Round(GetAverageTime() * 1000), NumberOfRuns, Math.Round(Shortest * 1000), Math.Round(Longest * 1000)); 

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetMillisecondsMessage()
        => String.Format("{0}ms average over {1} runs, shortest is {2}, longest is {3}", Math.Round(GetAverageTime()), NumberOfRuns, Math.Round(Shortest), Math.Round(Longest)); 
    } // cls

    /// <summary>
    /// 
    /// </summary>
    public sealed class SystemPerfInfo
    {
        public double TotalTime;
    } // cls
}
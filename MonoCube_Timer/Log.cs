using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MonoCube_Timer
{
    public enum Level { Debug = 1, Info = 2, Warn = 3, System = 4, Error = 5, Fatal = 9, None = 10 }
    public static class Log
    {
        public static Level LogCutoff = Level.Info;
        public static string LogFilePath = Path.Combine(DataProcessing.GetRootFolder(), "Logs");


        /// <summary>
        /// Gets the header for the log message.
        /// </summary>
        /// <param name="l">The log level.</param>
        /// <returns></returns>
        public static string GetLogHeader(Level l)
        {
            return $"{DateTime.Now.ToString("HH:mm:ss")}  {(l.ToString() + ":").PadRight(7)}  ";
        }
        /// <summary>
        /// Saves a log message to file.
        /// </summary>
        /// <param name="message">The full text of the log message to save.</param>
        public static void LogMessage(string message)
        {
            File.AppendAllText(Path.Combine(LogFilePath, "current.log"), message + "\n");
        }

        public static void Debug(string message)
        {
            if (Level.Debug >= LogCutoff)
            {
                LogMessage(GetLogHeader(Level.Debug) + message);
            }
        }
        public static void Info(string message)
        {
            if (Level.Info >= LogCutoff)
            {
                LogMessage(GetLogHeader(Level.Info) + message);
            }
        }
        public static void Warn(string message)
        {
            if (Level.Warn >= LogCutoff)
            {
                LogMessage(GetLogHeader(Level.Warn) + message);
            }
        }
        public static void System(string message)
        {
            if (Level.System >= LogCutoff)
            {
                LogMessage(GetLogHeader(Level.System) + message);
            }
        }

        public static void Error(string message)
        {
            if (Level.Error >= LogCutoff)
            {
                LogMessage(GetLogHeader(Level.Error) + message + "\n    No exception information available\n\n");
            }
        }
        public static void Error(Exception e)
        {
            if (Level.Error >= LogCutoff)
            {
                LogMessage(GetLogHeader(Level.Error) + e.Message + "\n    " + e.StackTrace + "\n");
            }
        }

        public static void Fatal(string message)
        {
            if (Level.Fatal >= LogCutoff)
            {
                LogMessage(GetLogHeader(Level.Fatal) + message + "\n    No exception information available\n\n");
            }
        }
        public static void Fatal(Exception e)
        {
            if (Level.Fatal >= LogCutoff)
            {
                LogMessage(GetLogHeader(Level.Fatal) + e.Message + "\n    " + e.StackTrace + "\n");
            }
        }


        /// <summary>
        /// Called on startup to save the current log as previous.log
        /// </summary>
        public static void SavePreviousLog()
        {
            string currentPath = Path.Combine(LogFilePath, "current.log");
            string prevPath = Path.Combine(LogFilePath, "previous.log");
            if (File.Exists(currentPath))
            {
                File.Delete(prevPath);
                File.Copy(currentPath, prevPath);
                File.Delete(currentPath);
            }
        }
    }
}

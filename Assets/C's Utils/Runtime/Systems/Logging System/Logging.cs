/* Logging.cs - C's Utils
 * 
 * A full-fledged logging system, based on Minecraft's logging system meant to scalable and customised with ease
 * 
 * 
 * Creation Date: 03/12/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [03/12/2023] - Initial implementation (C137)
 *      [04/12/2023] - File logging and compression support (C137)
 *      
 *      [05/12/2023] - Stack trace support (C137)
 *                   - Attribute support to hide a method from the logging stack trace (C137)
 *                   - Fixed logging file being overridden at each log (C137)
 *                   - Logging file is now overridden only at the first log (C137)
 *                   - Log level is now shown in the log (C137)
 */
using CsUtils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CsUtils.Systems.Logging
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class HideFromStackTraceAttribute : Attribute { }

    /// <summary>
    /// All of the available log levels
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }

    /// <summary>
    /// The different types of timestamps that can be displayed
    /// </summary>
    public enum Timestamp
    {
        DateAndTime,
        TimeAndDate,
        DateOnly,
        TimeOnly
    }

    /// <summary>
    /// Contains data for how the different log levels should be color-coded
    /// </summary>
    public struct LogColors
    {
        /// <summary>
        /// The color to use for Debug level
        /// </summary>
        public Color debug;

        /// <summary>
        /// The color to use for Info level
        /// </summary>
        public Color info;

        /// <summary>
        /// The color to use for Info level
        /// </summary>
        public Color warning;

        /// <summary>
        /// The color to use for Info level
        /// </summary>
        public Color error;

        /// <summary>
        /// The color to use for Info level
        /// </summary>
        public Color fatal;

        public readonly Color GetLevelColor(int level)
        {
            return level switch
            {
                0 => debug,
                1 => info,
                2 => warning,
                3 => error,
                4 => fatal,
                _ => debug,
            };
        }
    }

    /// <summary>
    /// Contains all the info of a log
    /// </summary>
    public struct LogInfo
    {
        /// <summary>
        /// The actual log
        /// </summary>
        public string log;

        /// <summary>
        /// The level of the log (How important it is)
        /// </summary>
        public LogLevel level;

        /// <summary>
        /// The stack trace from which the log originates
        /// </summary>
        public string stackTrace;
    }

    /// <summary>
    /// The main interface for handling the logging system
    /// </summary>
    public interface ILogger
    {
        public string Log(string log, LogLevel level, UnityEngine.Object context = null, Timestamp timestamp = Timestamp.TimeOnly, bool formatLog = true, bool forceShowInConsole = false, bool writeToFile = true, bool forceStackTrace = true, bool colorCode = true, params object[] parameters);

    }

    public class Logging : Singleton<Logging>, ILogger
    {
        /// <summary>
        /// The color-coding for each of the logs
        /// </summary>
        public LogColors logColors;

        /// <summary>
        /// The log level which will be displayed to the console
        /// </summary>
        public LogLevel consoleLogLevel = LogLevel.Info;

        /// <summary>
        /// Whether to do console logging, can be forced despite this value
        /// </summary>
        public bool doConsoleLogging = true;

        /// <summary>
        /// Whether to do file logging, can be forced despite this value
        /// </summary>
        public bool doFileLogging = true;

        /// <summary>
        /// The queue used to log any pending logs to file
        /// </summary>
        public Queue<string> logQueue = new();

        /// <summary>
        /// Whether the log file was successfully compressed
        /// </summary>
        public bool logFileCompressed = false;

        /// <summary>
        /// Whether the log file can still be compressed
        /// </summary>
        protected bool canCompressLog = true;

        private void Start()
        {
            CompressLogFile();
        }

        protected virtual void CompressLogFile(bool showError = true)
        {
            if (!Directory.Exists(Path.GetDirectoryName(CsSettings.singleton.loggingFilePath)))
                return;

            if (!File.Exists(CsSettings.singleton.loggingFilePath))
                return;

            try
            {
                using FileStream stream = File.Open(CsSettings.singleton.loggingFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)

                //Manually log to file so as to avoid recursion
                if (showError)
                    LogToFile(Log("Could not compress log file as it is currently in use. It will be compressed upon being released", LogLevel.Error, writeToFile: false), true);
                return;
            }


            string zipFileName = DateTime.Now.ToString("dd-MM-yyyy");

            int count = 0;
            while (File.Exists(Path.Combine(Path.GetDirectoryName(CsSettings.singleton.loggingFilePath), zipFileName + ".zip")))
            {
                if (count == int.MaxValue)
                {
                    //Manually log to file so as to avoid recursion
                    if (showError)
                        LogToFile(Log("Could not create log zip archive. Archive name is already in use", LogLevel.Error, writeToFile: false), true);

                    return;
                }
                zipFileName = DateTime.Now.ToString("dd-MM-yyyy") + $"-{count++}";
            }

            try
            {
                using (var zipArchive = ZipFile.Open(Path.Combine(Path.GetDirectoryName(CsSettings.singleton.loggingFilePath), zipFileName + ".zip"), ZipArchiveMode.Create))
                {
                    // Add the source file to the zip archive
                    zipArchive.CreateEntryFromFile(CsSettings.singleton.loggingFilePath, Path.GetFileName(CsSettings.singleton.loggingFilePath));
                }

                logFileCompressed = true;
            }
            catch (IOException)
            {
                if (File.Exists(Path.Combine(Path.GetDirectoryName(CsSettings.singleton.loggingFilePath), zipFileName + ".zip")))
                    File.Delete(Path.Combine(Path.GetDirectoryName(CsSettings.singleton.loggingFilePath), zipFileName + ".zip"));

                //Manually log to file so as to avoid recursion
                if(showError)
                    LogToFile(Log("Could not compress log file as it is currently in use. It will be compressed upon being released", LogLevel.Error, writeToFile: false), true);
            }

        }

        private void Update()
        {
            //Check if the queue contains any items to log. If so log them to file
            if(logQueue.Any())
            {
                LogToFile(string.Empty);
            }
        }

        [HideInCallstack, HideFromStackTrace]
        public virtual string Log(string log, LogLevel level, UnityEngine.Object context = null, Timestamp timestamp = Timestamp.TimeOnly, bool formatLog = true, bool forceShowInConsole = false, bool writeToFile = true, bool forceStackTrace = true, bool colorCode = true, params object[] parameters)
        { 
            string timeStampValue = $"[{GetTimestamp()}]";
            string actualLog = $"{timeStampValue} [{Enum.GetName(typeof(LogLevel), level)}] {(formatLog ? FormatLog(log, parameters) : log)}";

            if(forceShowInConsole || (level >= consoleLogLevel && doConsoleLogging))
            {
                LogToConsole($"<color={ColorUtility.ToHtmlStringRGBA(logColors.GetLevelColor((int)level))}> {actualLog}</color>", level);

                if (writeToFile)
                {
                    string fileLog = actualLog;
                    if(forceShowInConsole || level >= LogLevel.Error) 
                    {
                        string stackTrace =
                                StaticUtils.BreakAndIndent(GetStackTrace()
                                            .Remove(0, 2) /*Removes the whitespace that the stack trace trace produces for some reason*/
                                            .Replace("C's Utils", "C's-Utils")/*Prevents the indenter from indenting the file path as it contains a space*/,
                                            timeStampValue.Length + 4, 200) /*Indent according to the stack trace*/;

                        fileLog += "\n" + StaticUtils.BreakAndIndent(GetStackTrace().Remove(0, 2).Replace("C's Utils", "C's-Utils")/*Replace*/, timeStampValue.Length + 4, 100);
                    }

                    LogToFile(fileLog + "\n");
                }
            }

            return actualLog;
                
            string GetTimestamp()
            {
                return timestamp switch
                {
                    Timestamp.DateAndTime => DateTime.Now.ToString("dd/MM/yyyy-HH:m:ss"),
                    Timestamp.TimeAndDate => DateTime.Now.ToString("HH:m:ss-dd/MM/yyyy"),
                    Timestamp.DateOnly => DateTime.Now.ToString("dd/MM/yyyy"),
                    Timestamp.TimeOnly => DateTime.Now.ToString("HH:m:ss"),
                    _ => DateTime.Now.ToString("dd/MM/yyyy-HH:m:ss"),
                };
            }

            [HideInCallstack, HideFromStackTrace]
            string GetStackTrace()
            {
                return 
                    string.Concat(new System.Diagnostics.StackTrace(true)
                                    .GetFrames()
                                    .Where(frame => !frame.GetMethod().ShouldHideFromStackTrace())
                                    .Select(frame => new System.Diagnostics.StackTrace(frame).ToString())
                                    .ToArray());
            }
        }

        #region Logging Utilities
        /// <summary>
        /// Formats a log with the given parameters
        /// </summary>
        /// <param name="log">The log to format</param>
        /// <param name="parameters">The parameters to fromat it with</param>
        /// <returns></returns>
        public virtual string FormatLog(string log, object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                log = log.Replace("{" + i + "}", FormatType(parameters[i]));
            }
            return log;
        }

        /// <summary>
        /// Formats certain types into a better display format
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual string FormatType<T>(T type)
        {
            if (type is IEnumerable)
            {
                List<object> obj = new();
                foreach (var item in type as IEnumerable)
                {
                    obj.Add(item);
                }

                return obj.ToArray().Format();
            }

            return type.ToString();
        }
        #endregion

        /// <summary>
        /// Shortcut to actually do the logging in the console with the proper unity log format
        /// </summary>
        /// <param name="log">The log to log</param>
        /// <param name="level">The log level</param>
        /// <param name="context">The context if any</param>
        protected virtual void LogToConsole(string log, LogLevel level, UnityEngine.Object context = null)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    Debug.Log(log, context); 
                    break;

                case LogLevel.Info:
                    Debug.Log(log, context);
                    break;

                case LogLevel.Warning:
                    Debug.LogWarning(log, context);
                    break;

                case LogLevel.Error:
                    Debug.LogError(log, context);
                    break;

                case LogLevel.Fatal:
                    Debug.LogError(log, context);
                    break;
            }
        }

        protected virtual void LogToFile(string log, bool skipCompressionCheck = false)
        {
            if (canCompressLog && !logFileCompressed && !skipCompressionCheck)
                CompressLogFile(false);

            try
            {
                FileStream fs = GetLoggingFileStream();

                //Log file can no longer be compressed as its contents have been overridden
                canCompressLog = false;

                string fullLog = string.Empty;

                for (int i = 0; i < logQueue.Count; i++)
                {
                    fullLog += logQueue.Dequeue();
                }

                //Add the current log at the bottom since the queued ones were created before this one
                fullLog += log;

                byte[] bytes = Encoding.UTF8.GetBytes(fullLog);

                fs.Seek(0, SeekOrigin.End);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();

                logQueue.Clear();
            }
            catch(Exception)
            {
                logQueue.Enqueue(log);
            }
        }

        protected virtual FileStream GetLoggingFileStream()
        {
            if(!Directory.Exists(Path.GetDirectoryName(CsSettings.singleton.loggingFilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(CsSettings.singleton.loggingFilePath));

            if (File.Exists(CsSettings.singleton.loggingFilePath) && canCompressLog)
                    return File.Open(CsSettings.singleton.loggingFilePath, FileMode.Create, FileAccess.ReadWrite);

            return File.Open(CsSettings.singleton.loggingFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
    }
}

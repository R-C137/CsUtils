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
 *    
 *      [08/12/2023] - Improved stack trace handling (C137)
 *                   - Added compatibility with new indentation system (C137)
 *      
 *      [09/12/2023] - Added automatic error logging (C137)
 *                   - Log colors are now serialized in the inspector
 *                   - Default log colors addition (C137)
 *                   - Removed timestamp from console logs (C137)
 *                   - Timestamp is now optional (C137)
 *                   - Moved queue check from Update() to FixedUpdate() to ignore the timescale (C137)
 *                   - Moved queue logging from a queue to a string (C137)
 *                   - Fixed bug where console logging had to be enabled to do file logging (C137)
 *      
 *      [10/12/2023] - Updated accessibility modifiers (C137)
 *                   - Removed temporary code (C137)
 *                   - File logging can now be disabled and is not available for WebGL builds (C137)
 *                   - Fixed null reference exception with 'previousLogs' (C137)
 */
using CsUtils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CsUtils.Systems.Logging
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class HideFromStackTraceAttribute : Attribute { }

    /// <summary>
    /// All of the available log levels
    /// </summary>
    public enum LogSeverity
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
        TimeOnly,
        None
    }

    /// <summary>
    /// Contains data for how the different log levels should be color-coded
    /// </summary>
    [Serializable]
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
        /// The severity of the log 
        /// </summary>
        public LogSeverity level;

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
        public string Log(string log,
            LogSeverity level,
            UnityEngine.Object context = null, 
            Timestamp timestamp = Timestamp.TimeOnly,
            bool formatLog = true,
            bool forceShowInConsole = false,
            bool writeToFile = true,
            bool forceStackTrace = false,
            string customStackTrace = null,
            params object[] parameters);
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
        public LogSeverity consoleLogLevel = LogSeverity.Info;

        /// <summary>
        /// Whether to do console logging, can be forced despite this value
        /// </summary>
        public bool doConsoleLogging = true;

        /// <summary>
        /// Whether to do file logging, cannot be forced if set to false
        /// </summary>
        public bool doFileLogging = true;

        /// <summary>
        /// Whether to automatically log exceptions
        /// </summary>
        public bool doExceptionLogging = true;

        /// <summary>
        /// The previous logs awaiting to be logged
        /// </summary>
        protected string previousLogs;

        /// <summary>
        /// Whether the log file was successfully compressed
        /// </summary>
        protected bool logFileCompressed = false;

        /// <summary>
        /// Whether the log file can still be compressed
        /// </summary>
        protected bool canCompressLog = true;

        private void Start()
        {
            CompressLogFile();

            if(doExceptionLogging)
                Application.logMessageReceived += HandleErrors;
        }

        protected virtual void HandleErrors(string condition, string stackTrace, LogType type)
        {
            if(type == LogType.Exception)
            {
                Log(condition.Remove(0,11), LogSeverity.Fatal, stackTrace: stackTrace, forceShowInConsole: true);
            }
        }

        /// <summary>
        /// Compresses the latest log file so as to save space
        /// </summary>
        /// <param name="showError">Whether to debug any errors that occur(internal use only)</param>
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
                    LogToFile(Log("Could not compress log file as it is currently in use. It will be compressed upon being released", LogSeverity.Error, writeToFile: false), true);
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
                        LogToFile(Log("Could not create log zip archive. Archive name is already in use", LogSeverity.Error, writeToFile: false), true);

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
                    LogToFile(Log("Could not compress log file as it is currently in use. It will be compressed upon being released", LogSeverity.Error, writeToFile: false), true);
            }

        }

        private void FixedUpdate()
        {
            //Check if the queue contains any items to log. If so log them to file
            if(!string.IsNullOrEmpty(previousLogs))
            {
                LogToFile(string.Empty);
            }
        }

        /// <summary>
        /// Logs a given value to the console and to a file
        /// </summary>
        /// <param name="log">The value to log</param>
        /// <param name="severity">The severity of the log</param>
        /// <param name="context">The context in which to display the log</param>
        /// <param name="timestamp">How the timestamp should be displayed</param>
        /// <param name="formatLog">Whether to format the log before displaying it</param>
        /// <param name="forceShowInConsole">Whether to ignore the console log level and forcefully display the log</param>
        /// <param name="writeToFile">Whether to log this value to a file</param>
        /// <param name="forceStackTrace">Whether to display a stacktrace, ignoring the log severity</param>
        /// <param name="stackTrace">The stacktrace to use, set to null to automatically evaluate it</param>
        /// <param name="parameters">The parameters to format the log with, if formatting is allowed</param>
        /// <returns>The log that was(or should have been) written to the log file</returns>
        [HideInCallstack, HideFromStackTrace]
        public virtual string Log(string log, LogSeverity severity, UnityEngine.Object context = null, Timestamp timestamp = Timestamp.TimeOnly, bool formatLog = true, bool forceShowInConsole = false, bool writeToFile = true, bool forceStackTrace = false, string stackTrace = null, params object[] parameters)
        { 
            string timeStampValue = $"[{GetTimestamp()}]";
            string formattedLog = $"[{Enum.GetName(typeof(LogSeverity), severity)}] {(formatLog ? FormatLog(log, parameters) : log)}";
            string timeStampedLog = $"{timeStampValue} {formattedLog}";

            if(forceShowInConsole || (severity >= consoleLogLevel && doConsoleLogging))
                LogToConsole($"<color=#{ColorUtility.ToHtmlStringRGB(logColors.GetLevelColor((int)severity))}> {formattedLog}</color>", severity);

            if (doFileLogging && writeToFile && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                string fileLog = timeStampedLog;
                if (forceStackTrace || severity >= LogSeverity.Error)
                {
                    stackTrace =
                            (stackTrace ?? GetStackTrace())
                                        .Replace("C's Utils", "C's-Utils") /*Prevents the indenter from indenting the file path as it contains a space*/
                                        .BreakAndIndent(timeStampValue.Length + 4, 200) /*Indent according to the stack trace*/;

                    fileLog += "\n" + stackTrace[..^2]; //Remove the break line added by the stack trace for some reason
                }

                LogToFile(fileLog + "\n");
            }

            return formattedLog;
                
            string GetTimestamp()
            {
                return timestamp switch
                {
                    Timestamp.DateAndTime => DateTime.Now.ToString("dd/MM/yyyy-HH:m:ss"),
                    Timestamp.TimeAndDate => DateTime.Now.ToString("HH:m:ss-dd/MM/yyyy"),
                    Timestamp.DateOnly => DateTime.Now.ToString("dd/MM/yyyy"),
                    Timestamp.TimeOnly => DateTime.Now.ToString("HH:m:ss"),
                    Timestamp.None => "",
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
                                    .ToArray()).Remove(0, 2);/*Removes the whitespace that the stack trace trace produces for some reason*/
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
        protected virtual void LogToConsole(string log, LogSeverity level, UnityEngine.Object context = null)
        {
            switch (level)
            {
                case LogSeverity.Debug:
                    Debug.Log(log, context); 
                    break;

                case LogSeverity.Info:
                    Debug.Log(log, context);
                    break;

                case LogSeverity.Warning:
                    Debug.LogWarning(log, context);
                    break;

                case LogSeverity.Error:
                    Debug.LogError(log, context);
                    break;

                case LogSeverity.Fatal:
                    Debug.LogError(log, context);
                    break;
            }
        }

        /// <summary>
        /// Handles the file logging
        /// </summary>
        /// <param name="log">The value to log</param>
        /// <param name="skipCompressionCheck">Whether the file’s compressibility should be skipped</param>
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

                if (!string.IsNullOrEmpty(previousLogs))
                    fullLog += previousLogs;

                //Add the current log at the bottom since the queued ones were created before this one
                fullLog += log;

                byte[] bytes = Encoding.UTF8.GetBytes(fullLog);

                fs.Seek(0, SeekOrigin.End);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();

                previousLogs = null;
            }
            catch(Exception)
            {
                previousLogs += log;
            }
        }

        /// <summary>
        /// Gets the file stream used for file loggings
        /// </summary>
        /// <returns></returns>
        protected virtual FileStream GetLoggingFileStream()
        {
            if(!Directory.Exists(Path.GetDirectoryName(CsSettings.singleton.loggingFilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(CsSettings.singleton.loggingFilePath));

            if (File.Exists(CsSettings.singleton.loggingFilePath) && canCompressLog)
                    return File.Open(CsSettings.singleton.loggingFilePath, FileMode.Create, FileAccess.ReadWrite);

            return File.Open(CsSettings.singleton.loggingFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Properly set the default values
        /// </summary>
        protected virtual void Reset()
        {
            logColors = new LogColors()
            {
                debug = StaticUtils.FromHex("A6A6A6"),
                info = StaticUtils.FromHex("FBF6EE"),
                warning = StaticUtils.FromHex("EEC759"),
                error = StaticUtils.FromHex("EF4040"),
                fatal = StaticUtils.FromHex("C70039")
            };
        }
    }
}

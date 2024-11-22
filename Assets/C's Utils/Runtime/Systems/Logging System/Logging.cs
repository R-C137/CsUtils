/* Logging.cs - C's Utils
 * 
 * A full-fledged logging system, based on Minecraft's logging system meant to scalable and customised with ease
 * 
 * NOTE: As of 2022.3.14f1 the 'HideInCallstack' attribute doesn't work and will still display the Log() functions in the console call stack.
 *       You can disable this manually in the console settings by enabling 'Strip Logging Callstack'.
 *       However, double-clicking on the log will bring you to the logging class
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
 *      
 *      [16/12/2023] - Removed unnecessary stripping of the exception string (C137)
 *                   - Exceptions are now only logged in the logging file (C137)
 *                   - Stacktrace, console logging and file logging can now be forcefully disabled or enabled when calling the Log() function (C137)
 *                   
 *      [19/12/2023] - Fixed hex colour parsing (C137)
 *      [25/12/2023] - Added a function to access the Log(..) function without implicitly using the singleton (C137)
 *      [26/12/2023] - Prevented string from being treated as an array (C137)
 *      [05/01/2024] - Fixed null reference exception in the log formating (C137)
 *                   - Updated execution order (C137)
 *                   - Added support for logging from within the Editor (C137)
 *                   
 *      [07/03/2024] - Updated function summaries (C137)
 *      [12/05/2024] - Logs no longer need to be strings (C137)
 *      [19/07/2024] - Made 'LogToConsole' protected internal to support the 'StaticUtils.AutoLog' (C137)
 *                   - Default logger is now set directly on Awake (C137)
 *                   
 *      [22/07/2024] - Proper singleton implementation (C137)
 *                   - Updated execution order (C137)
 *
 *      [22/11/2024] - Singleton values are now properly set in the editor (C137)
 *                   - Singleton reference to 'CsSettings' is now cached (C137)
 *      
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
        public string LogDirect(object log,
            LogSeverity level,
            UnityEngine.Object context = null,
            Timestamp timestamp = Timestamp.TimeOnly,
            bool formatLog = true,
            bool? showInConsole = null,
            bool? fileLogging = null,
            bool? forceStackTrace = null,
            string customStackTrace = null,
            params object[] parameters);
    }

    [DefaultExecutionOrder(-30), ExecuteAlways]
    public class Logging : MonoBehaviour, ILogger
    {
        /// <summary>
        /// Reference to the settings class for ease of access
        /// </summary>
        [HideInInspector]
        public CsSettings settings;
        
        /// <summary>
        /// The color-coding for each of the logs
        /// </summary>
        public LogColors logColors;

        /// <summary>
        /// The log level which will be displayed to the console
        /// </summary>
        public LogSeverity consoleLogLevel = LogSeverity.Info;

        /// <summary>
        /// Whether to do console logging
        /// </summary>
        public bool doConsoleLogging = true;

        /// <summary>
        /// Whether to do file logging
        /// </summary>
        public bool doFileLogging = true;

        /// <summary>
        /// Whether to automatically log exceptions in file
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

        /// <summary>
        /// Whether a reference to 'CsSettings' existed previously
        /// </summary>
        private bool settingsExisted;
        
        void Awake()
        {
            Singleton.Create(this);
        }

        private void Start()
        {
            SetupLogging();
        }

        private void OnValidate()
        {
            if(Application.isEditor && !Singleton.HasInstance<Logging>())
                Singleton.Create(this);
        }

        void SetupLogging()
        {
            settings = settings == null ? (Singleton.HasInstance<CsSettings>() ? Singleton.Get<CsSettings>() : null) : settings;
            
            if(settings == null)
                LogToConsole("No instance of 'CsSettings' could be found. Default logger could not be set. Please manually set the reference to 'CsSettings'", LogSeverity.Warning, this);
            
            CompressLogFile();

            if(doExceptionLogging)
            {
                Application.logMessageReceived -= HandleErrors; // Remove any pre-existing calls
                Application.logMessageReceived += HandleErrors;
            }
        }
        
        protected virtual void HandleErrors(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                LogDirect(condition, LogSeverity.Fatal, stackTrace: stackTrace, showInConsole: false);
            }
        }

        /// <summary>
        /// Compresses the latest log file so as to save space
        /// </summary>
        /// <param name="showError">Whether to debug any errors that occur(internal use only)</param>
        protected virtual void CompressLogFile(bool showError = true)
        {
            if (settings == null)
            {
                LogToConsole("No instance of 'CsSettings' could be found. Logs files could not be compressed", LogSeverity.Warning);
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(settings.loggingFilePath)))
                return;

            if (!File.Exists(settings.loggingFilePath))
                return;

            try
            {
                using FileStream stream = File.Open(settings.loggingFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
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
                    LogToFile(LogDirect("Could not compress log file as it is currently in use. It will be compressed upon being released", LogSeverity.Error, fileLogging: false), true);
                return;
            }


            string zipFileName = DateTime.Now.ToString("dd-MM-yyyy");

            int count = 0;
            while (File.Exists(Path.Combine(Path.GetDirectoryName(settings.loggingFilePath), zipFileName + ".zip")))
            {
                if (count == int.MaxValue)
                {
                    //Manually log to file so as to avoid recursion
                    if (showError)
                        LogToFile(LogDirect("Could not create log zip archive. Archive name is already in use", LogSeverity.Error, fileLogging: false), true);

                    return;
                }
                zipFileName = DateTime.Now.ToString("dd-MM-yyyy") + $"-{count++}";
            }

            try
            {
                using (var zipArchive = ZipFile.Open(Path.Combine(Path.GetDirectoryName(settings.loggingFilePath), zipFileName + ".zip"), ZipArchiveMode.Create))
                {
                    // Add the source file to the zip archive
                    zipArchive.CreateEntryFromFile(settings.loggingFilePath, Path.GetFileName(settings.loggingFilePath));
                }   

                logFileCompressed = true;
            }
            catch (IOException)
            {
                if (File.Exists(Path.Combine(Path.GetDirectoryName(settings.loggingFilePath), zipFileName + ".zip")))
                    File.Delete(Path.Combine(Path.GetDirectoryName(settings.loggingFilePath), zipFileName + ".zip"));

                //Manually log to file so as to avoid recursion
                if (showError)
                    LogToFile(LogDirect("Could not compress log file as it is currently in use. It will be compressed upon being released", LogSeverity.Error, fileLogging: false), true);
            }

        }

        private void FixedUpdate()
        {
            //Check if there are any items waiting to be logged to file. If so do the logging
            if (!string.IsNullOrEmpty(previousLogs))
            {
                LogToFile(string.Empty);
            }
        }

        /// <summary>
        /// A shortcut to access the LogDirect(...) function through the singleton without explicitly calling it
        /// </summary>
        [HideInCallstack, HideFromStackTrace]
        public static string Log(object log, LogSeverity severity, UnityEngine.Object context = null, Timestamp timestamp = Timestamp.TimeOnly, bool formatLog = true, bool? showInConsole = null, bool? fileLogging = null, bool? forceStackTrace = null, string stackTrace = null, params object[] parameters)
                                => Singleton.Get<Logging>().LogDirect(log, severity, context, timestamp, formatLog, showInConsole, fileLogging, forceStackTrace, stackTrace, parameters);

        /// <summary>
        /// Display a log to the console and optionally write it to a file
        /// </summary>
        /// <param name="log">The value to log</param>
        /// <param name="severity">The severity of the log</param>
        /// <param name="context">The context in which to display the log</param>
        /// <param name="timestamp">How the timestamp should be displayed</param>
        /// <param name="formatLog">Whether to format the log before displaying it</param>
        /// <param name="showInConsole">Whether to forcefully display the log in the console. Null to use default behaviour</param>
        /// <param name="fileLogging">Whether to forcefully display this log in the logging file. Null to use default behaviour</param>
        /// <param name="forceStackTrace">Whether to forcefully displaay a stacktrace. Null to use default behaviour</param>
        /// <param name="stackTrace">The stacktrace to use. Null to automatically evaluate it</param>
        /// <param name="parameters">The parameters to format the log with, if formatting is allowed</param>
        /// <returns>The log that was(or should have been) written to the log file</returns>
        [HideInCallstack, HideFromStackTrace]
        public virtual string LogDirect(object log, LogSeverity severity, UnityEngine.Object context = null, Timestamp timestamp = Timestamp.TimeOnly, bool formatLog = true, bool? showInConsole = null, bool? fileLogging = null, bool? forceStackTrace = null, string stackTrace = null, params object[] parameters)
        {
            string timeStampValue = $"[{GetTimestamp()}]";
            string formattedLog = $"[{Enum.GetName(typeof(LogSeverity), severity)}] {(formatLog ? FormatLog(log, parameters) : log)}";
            string timeStampedLog = $"{timeStampValue} {formattedLog}";

            //if(forceShowInConsole || (severity >= consoleLogLevel && doConsoleLogging))
            if (showInConsole == true || (showInConsole == null && severity >= consoleLogLevel && doConsoleLogging))
                LogToConsole($"<color=#{ColorUtility.ToHtmlStringRGB(logColors.GetLevelColor((int)severity))}> {formattedLog}</color>", severity);

            //if (doFileLogging && writeToFile && Application.platform != RuntimePlatform.WebGLPlayer)
            if (fileLogging == true || (fileLogging == null && doFileLogging && Application.platform != RuntimePlatform.WebGLPlayer))
            {
                string fileLog = timeStampedLog;
                if (forceStackTrace == true || (forceStackTrace == null && severity >= LogSeverity.Error))
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
                                    .ToArray()).Remove(0, 2);//Removes the whitespace that the stack trace trace produces for some reason
            }
        }

        #region Logging Utilities
        /// <summary>
        /// Formats a log with the given parameters
        /// </summary>
        /// <param name="log">The log to format</param>
        /// <param name="parameters">The parameters to fromat it with</param>
        /// <returns></returns>
        public virtual string FormatLog(object log, object[] parameters)
        {
            if (log is not string)
                return FormatType(log);

            for (int i = 0; i < parameters.Length; i++)
            {
                log = (log as string).Replace("{" + i + "}", FormatType(parameters[i]));
            }

            return log as string;
        }

        /// <summary>
        /// Formats certain types into a better display format
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual string FormatType<T>(T type)
        {
            if (type is IEnumerable && type is not string)
            {
                List<object> obj = new();
                foreach (var item in type as IEnumerable)
                {
                    obj.Add(item);
                }

                return obj.ToArray().Format();
            }
            if (type == null)
                return "null";

            return type.ToString();
        }
        #endregion

        /// <summary>
        /// Shortcut to actually do the logging in the console with the proper unity log format
        /// </summary>
        /// <param name="log">The log to log</param>
        /// <param name="level">The log level</param>
        /// <param name="context">The context if any</param>
        [HideInCallstack]
        protected internal virtual void LogToConsole(string log, LogSeverity level, UnityEngine.Object context = null)
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
        /// <param name="skipCompressionCheck">Whether the file�s compressibility should be skipped</param>
        protected virtual void LogToFile(string log, bool skipCompressionCheck = false)
        {
            if (settings == null)
            {
                LogToConsole("No instance of 'CsSettings' could be found. Logs files could not be created", LogSeverity.Warning);
                return;
            }

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
            catch (Exception)
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
            if (!Directory.Exists(Path.GetDirectoryName(settings.loggingFilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(settings.loggingFilePath));

            if (File.Exists(settings.loggingFilePath) && canCompressLog)
                return File.Open(settings.loggingFilePath, FileMode.Create, FileAccess.ReadWrite);

            return File.Open(settings.loggingFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Properly set the default values
        /// </summary>
        protected virtual void Reset()
        {
            logColors = new LogColors()
            {
                debug = StaticUtils.ColorFromHex("#A6A6A6"),
                info = StaticUtils.ColorFromHex("#FBF6EE"),
                warning = StaticUtils.ColorFromHex("#EEC759"),
                error = StaticUtils.ColorFromHex("#EF4040"),
                fatal = StaticUtils.ColorFromHex("#C70039")
            };
        }

        private void OnDestroy()
        {
            Singleton.Remove(this);
        }
    }
}

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
 */
using CsUtils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CsUtils.Systems.Logging
{
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
        public void Log(string log, LogLevel level, UnityEngine.Object context = null, Timestamp timestamp = Timestamp.DateAndTime, bool formatLog = true, bool forceShowInConsole = false, bool writeToFile = true, bool stackTrace = true, bool colorCode = true, params object[] parameters);

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

        public virtual void Log(string log, LogLevel level, UnityEngine.Object context = null, Timestamp timestamp = Timestamp.DateAndTime, bool formatLog = true, bool forceShowInConsole = false, bool writeToFile = true, bool stackTrace = true, bool colorCode = true, params object[] parameters)
        {
            if(forceShowInConsole || (level >= consoleLogLevel && doConsoleLogging))
            {
                LogToConsole($"<color={ColorUtility.ToHtmlStringRGBA(logColors.GetLevelColor((int)level))}>[{GetTimestamp()}] {FormatLog(log, parameters)}</color>", level);
            }

            string GetTimestamp()
            {
                return timestamp switch
                {
                    Timestamp.DateAndTime => DateTime.Now.ToString("dd/MM/yyyy:HH:m:ss"),
                    Timestamp.TimeAndDate => DateTime.Now.ToString("HH:m:ss:dd/MM/yyyy"),
                    Timestamp.DateOnly => DateTime.Now.ToString("dd/MM/yyyy"),
                    Timestamp.TimeOnly => DateTime.Now.ToString("HH:m:ss:"),
                    _ => DateTime.Now.ToString("dd/MM/yyyy:HH:m:ss"),
                };
            }
        }

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
    }
}

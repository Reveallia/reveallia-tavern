using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Tools
{
    public enum LogType
    {
        Info,
        Warning,
        Error,
        Important,
        Temporary,
        GameLoop,
        Events,
        PassCheck,
    }
    
    public static class CustomLogger
    {
        public static Dictionary<LogType, string> LogTypeColors = new Dictionary<LogType, string>
        {
            {LogType.Info, "#808080"},      
            {LogType.Warning, "#FFA500"},
            {LogType.Error, "#DC143C"},        
            {LogType.Important, "#9441e0"},
            {LogType.Temporary, "#FFA500"},
            {LogType.GameLoop, "#FFD700"},
            {LogType.Events, "#FFD700"},
            {LogType.PassCheck, "#00FF00"},
        };
        
        public static Dictionary<LogType, bool> LogTypeEnabled = new Dictionary<LogType, bool>
        {
            {LogType.Info, true},
            {LogType.Warning, true},
            {LogType.Error, true},
            {LogType.Important, true},
            {LogType.Temporary, true},
            {LogType.GameLoop, true},
            {LogType.Events, true},
            {LogType.PassCheck, true},
        };
        
        public static void Log(LogType logType, string message, Exception exception = null)
        {
            if (!LogTypeEnabled[logType]) return;
            
            string color = LogTypeColors[logType];
            string logMessage = $"<color={color}>[{logType}]: {message}</color>";

            switch (logType)
            {
                case LogType.Error:
                    if (exception != null)
                    {
                        exception.Data.Add("LogMessage", logMessage);
                        Debug.LogException(exception);
                    }
                    else
                    {
                        Debug.LogError(logMessage);
                    }
                    break;
                default:
                    Debug.Log(logMessage);
                    break;
            }
        }
        
        public static void LogObject(object obj, string label = null, bool saveInFile = false, bool saveInFileOnOverflow = false)
        {
            if (obj == null)
            {
                Debug.LogWarning("LogObject: null");
                return;
            }

            string output;

            try
            {
                output = JsonConvert.SerializeObject(obj, Formatting.Indented);
                if (saveInFile || (output.Length > 10000 && saveInFileOnOverflow))
                {
                    string objType = obj.GetType().Name;
                    string fileName = $"{objType}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    System.IO.File.WriteAllText(fileName, output);
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError("JsonConvert failed: " + e.Message);
                output = obj.ToString();
            }
            Debug.Log(string.IsNullOrEmpty(label) ? output : $"{label}:\n{output}");
        }
        
        public static void LogInfo(string message)
        {
            Log(LogType.Info, message);
        }
        
        public static void LogWarning(string message)
        {
            Log(LogType.Warning, message);
        }
        
        public static void LogError(string message, Exception exception = null)
        {
            Log(LogType.Error, $"{message}", exception);
        }
        
        public static void LogImportant(string message)
        {
            Log(LogType.Important, message);
        }
        
        public static void LogTemporary(string message)
        {
            Log(LogType.Temporary, message);
        }

        public static void LogGameLoop(string message)
        {
            Log(LogType.GameLoop, message);
        }
        
        
        public static void LogEvents<T>(T eventData)
        {
            Log(LogType.Events, $"EventType: {typeof(T).Name}, Data: {JsonConvert.SerializeObject(eventData, Formatting.Indented)}");
        }
        
        public static void LogPassCheck(string message)
        {
            Log(LogType.PassCheck, message);
        }

        public static void CheckNull(object obj, string label = null)
        {
            if (obj == null)
            {
                LogError($"Object is null: {label ?? "Unknown"}");
            }
            else
            {
                LogInfo($"Object is not null: {label ?? "Unknown"}");
            }
        }


    }
}
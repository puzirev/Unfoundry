using UnityEngine;

namespace Unfoundry
{
    public class LogSource
    {
        private string _modName;

        public LogSource(string modName)
        {
            _modName = modName;
        }

        public void Log(object message)
        {
            Debug.Log($"{_modName}: {message}");
        }

        public void Log(object message, Object context)
        {
            Debug.Log($"{_modName}: {message}", context);
        }

        public void LogFormat(string format, params object[] args)
        {
            Debug.LogFormat($"{_modName}: {format}", args);
        }

        public void LogError(object message)
        {
            Debug.LogError($"{_modName}: {message}");
        }

        public void LogError(object message, Object context)
        {
            Debug.LogError($"{_modName}: {message}", context);
        }

        public void LogErrorFormat(string format, params object[] args)
        {
            Debug.LogErrorFormat($"{_modName}: {format}", args);
        }

        public void LogErrorFormat(Object context, string format, params object[] args)
        {
            Debug.LogErrorFormat(context, $"{_modName}: {format}", args);
        }

        public void LogWarning(object message)
        {
            Debug.LogWarning($"{_modName}: {message}");
        }

        public void LogWarning(object message, Object context)
        {
            Debug.LogWarning($"{_modName}: {message}", context);
        }

        public void LogWarningFormat(string format, params object[] args)
        {
            Debug.LogWarningFormat($"{_modName}: {format}", args);
        }

        public void LogWarningFormat(Object context, string format, params object[] args)
        {
            Debug.LogWarningFormat(context, $"{_modName}: {format}", args);
        }
    }
}

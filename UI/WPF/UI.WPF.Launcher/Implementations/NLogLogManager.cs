#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NLog;
using Splat;
using LogLevel = Splat.LogLevel;

#endregion

namespace UI.WPF.Launcher.Implementations
{
    [Export(typeof(ILogManager))]
    public class NLogLogManager : ILogManager
    {
        private readonly Dictionary<Type, IFullLogger> _loggerCache;

        public NLogLogManager()
        {
            _loggerCache = new Dictionary<Type, IFullLogger>();
        }

        #region ILogManager Members

        public IFullLogger GetLogger(Type type)
        {
            lock (_loggerCache)
            {
                IFullLogger value;
                if (_loggerCache.TryGetValue(type, out value))
                    return value;

                value = new WrappingFullLogger(new NLoggerAdapter(LogManager.GetLogger(type.FullName)), type);
                _loggerCache[type] = value;

                return value;
            }
        }

        #endregion
    }

    public class NLoggerAdapter : ILogger
    {
        private readonly Logger _logger;

        public NLoggerAdapter(Logger logger)
        {
            _logger = logger;
        }

        #region ILogger Members

        public void Write(string message, LogLevel logLevel)
        {
            if (logLevel < Level)
            {
                return;
            }

            NLog.LogLevel level;
            switch (logLevel)
            {
                case LogLevel.Debug:
                    level = NLog.LogLevel.Debug;
                    break;
                case LogLevel.Info:
                    level = NLog.LogLevel.Info;
                    break;
                case LogLevel.Warn:
                    level = NLog.LogLevel.Warn;
                    break;
                case LogLevel.Error:
                    level = NLog.LogLevel.Error;
                    break;
                case LogLevel.Fatal:
                    level = NLog.LogLevel.Fatal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("logLevel");
            }

            _logger.Log(level, message);
        }

        public LogLevel Level { get; set; }

        #endregion
    }
}

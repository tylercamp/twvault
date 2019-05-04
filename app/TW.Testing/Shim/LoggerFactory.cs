using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TW.Testing.Shim
{
    class LoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger(categoryName);
        }

        public void Dispose()
        {
            
        }
    }

    class ConsoleLogger : ILogger
    {
        String category;

        public ConsoleLogger(String category)
        {
            this.category = category;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var now = DateTime.Now;
            var time = $"{now.Hour}:{now.Minute}:{now.Second}:{now.Millisecond}";

            Console.WriteLine("{3} {0}::{1}: {2}", logLevel, category, state, time);
        }
    }
}

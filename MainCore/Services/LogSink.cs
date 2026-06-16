using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace MainCore.Services
{
    [RegisterSingleton<LogSink>]
    public sealed class LogSink : ILogEventSink
    {
        private ConcurrentDictionary<AccountId, LinkedList<LogEvent>> Logs { get; } = new();

        private readonly IRxQueue _rxQueue;

        public LogSink(IRxQueue rxQueue)
        {
            _rxQueue = rxQueue;
        }

        public LinkedList<LogEvent> GetLogs(AccountId accountId)
        {
            return Logs.GetOrAdd(accountId, _ => new LinkedList<LogEvent>());
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Level < LogEventLevel.Information) return;
            var logEventPropertyValue = logEvent.Properties.GetValueOrDefault("AccountId");
            if (logEventPropertyValue is null) return;
            if (logEventPropertyValue is not ScalarValue scalarValue) return;
            var value = scalarValue.Value as string;
            if (value is null || !int.TryParse(value, out var parsed)) return;
            var accountId = new AccountId(parsed);

            var logs = GetLogs(accountId);
            lock (logs)
            {
                logs.AddFirst(logEvent);
                // keeps 200 message
                if (logs.Count > 200)
                {
                    logs.RemoveLast();
                }
            }

            _rxQueue.Enqueue(new LogEmitted(accountId, logEvent));
        }
    }

    public static class LogSinkExtensions
    {
        public static LoggerConfiguration LogSink(
                  this LoggerSinkConfiguration loggerConfiguration)
        {
            return loggerConfiguration.Sink(Locator.Current.GetService<LogSink>()!);
        }
    }
}
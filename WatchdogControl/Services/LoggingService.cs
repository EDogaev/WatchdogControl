using Microsoft.Extensions.Logging;
using WatchdogControl.Interfaces;

namespace WatchdogControl.Services;

public class LoggingService<T>(ILogger<T> logger, IMemoryLogStore memoryLogStore) : ILoggingService<T>
{
    public ILogger<T> Logger { get; } = logger;
    public IMemoryLogStore MemoryLogStore { get; } = memoryLogStore;

}
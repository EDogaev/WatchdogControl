using Microsoft.Extensions.Logging;

namespace WatchdogControl.Interfaces;

public interface ILoggingService<out T>
{
    ILogger<T> Logger { get; }
    IMemoryLogStore MemoryLogStore { get; }
}
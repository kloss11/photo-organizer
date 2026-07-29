using PhotoOrganizer.Application.Abstractions;

namespace PhotoOrganizer.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

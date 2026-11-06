namespace Claims.Tests.Fakes;

/// <summary>A clock that always returns the same time, so date rules don't depend on when tests run.</summary>
public class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

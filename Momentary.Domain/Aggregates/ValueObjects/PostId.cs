namespace Momentary.Domain.ValueObjects;
public record PostId(string Value)
{
    public static PostId NewId() => new(Guid.NewGuid().ToString("N"));
}
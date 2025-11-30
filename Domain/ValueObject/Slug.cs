namespace Domain.ValueObject;

public record Slug(string Value)
{
    public string SlugValue => Value.Replace(PermanentId, "");
    public string PermanentId { get; init; } = Value.Split('-').Last();
}
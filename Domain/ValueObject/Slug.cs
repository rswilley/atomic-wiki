namespace Domain.ValueObject;

public record Slug(string? Value)
{
    public string SlugValue => GetSlug(Value);
    
    private static string GetSlug(string? value)
    {
        if (string.IsNullOrEmpty(value)) 
            return string.Empty;
        
        var result = string.Empty;
        for (var i = 0; i < value.Length; i++)
            if (char.IsLetterOrDigit(value[i]) || value[i] == ' ' || value[i] == '-')
                result += value[i];
        return result.ToLower().Replace(" ", "-");
    }
}
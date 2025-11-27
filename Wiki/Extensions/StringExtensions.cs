namespace Wiki.Extensions;

public static class StringExtensions
{
    public static string ToSlug(this string value)
    {
        var result = string.Empty;
        for (var i = 0; i < value.Length; i++)
            if (char.IsLetterOrDigit(value[i]) || value[i] == ' ' || value[i] == '-')
                result += value[i];
        return result.ToLower().Replace(" ", "-");
    }
}
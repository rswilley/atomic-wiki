using Sqids;

namespace Wiki.Services;

public interface IIdService
{
    string Generate(long number);
}

public class IdService : IIdService
{
    private readonly SqidsEncoder<long> _squid = new(new SqidsOptions
    {
        Alphabet = "0lofzZ198PJQuFOdsSCGMVX6ApDRqvmbihgIt3TBU4eLy7NcrKY5a2xjHwWkEn",
        MinLength = 6,
    });

    public string Generate(long number)
    {
        return _squid.Encode(number);
    }
}
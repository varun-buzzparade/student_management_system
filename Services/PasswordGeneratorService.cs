using System.Security.Cryptography;
using System.Text;

namespace StudentManagementSystem.Services;

public sealed class PasswordGeneratorService : IPasswordGenerator
{
    private static readonly char[] Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] Lowercase = "abcdefghijkmnpqrstuvwxyz".ToCharArray();
    private static readonly char[] Digits = "23456789".ToCharArray();
    private static readonly char[] Symbols = "!@$%*?_-".ToCharArray();

    public string Generate(int length = 12)
    {
        if (length < 8)
            length = 8;

        var chars = new List<char>
        {
            GetRandomChar(Uppercase),
            GetRandomChar(Lowercase),
            GetRandomChar(Digits),
            GetRandomChar(Symbols)
        };

        var all = Uppercase.Concat(Lowercase).Concat(Digits).Concat(Symbols).ToArray();
        while (chars.Count < length)
            chars.Add(GetRandomChar(all));

        Shuffle(chars);
        return new string(chars.ToArray());
    }

    private static char GetRandomChar(char[] source)
    {
        var idx = RandomNumberGenerator.GetInt32(source.Length);
        return source[idx];
    }

    private static void Shuffle(IList<char> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

using System.IO.Hashing;
using System.Text;

namespace OMA.Core;

internal class OmaUtil
{
    public static string HashString(string value)
    {
        var hashBytes = XxHash128.Hash(Encoding.UTF8.GetBytes(value));
        return Encoding.UTF8.GetString(hashBytes);
    }
}

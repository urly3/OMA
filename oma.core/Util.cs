using System.IO.Hashing;

namespace OMA.Core;

class OMAUtil {
    public static string HashString(string value) {
        var hashBytes = XxHash128.Hash(System.Text.Encoding.UTF8.GetBytes(value));
        return System.Text.Encoding.UTF8.GetString(hashBytes);
    }
}

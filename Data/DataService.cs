using OMA.Models;
using System.IO.Hashing;

namespace OMA.Data;

class DataService
{
    private OMAContext _context;

    public DataService(OMAContext context)
    {
        _context = context;
    }

    public Alias? GetAlias(string name)
    {
        var hashBytes = XxHash128.Hash(System.Text.Encoding.UTF8.GetBytes(name));
        var hash = System.Text.Encoding.UTF8.GetString(hashBytes);

        return _context.Aliases.Where(a => a.Hash == hash).FirstOrDefault();
    }
}

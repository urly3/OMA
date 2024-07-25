using OMA.Data;
using OMA.Models;

namespace OMA.Services;

class OMAService
{
    private OMADataService _dataService;

    public OMAService(OMADataService dataService)
    {
        _dataService = dataService;
    }

    public Alias? GetAlias(string name)
    {
        return _dataService.GetAlias(name);
    }

    public Alias? CreateAlias(string name)
    {
        if (_dataService.GetAlias(name) != null)
        {
            return null;
        }

        if (!_dataService.CreateAlias(name))
        {
            return null;
        }

        return _dataService.GetAlias(name);
    }

    public Alias? GetOrCreateAlias(string name)
    {
        Alias? alias = _dataService.GetAlias(name);
        if (alias == null)
        {
            _ = _dataService.CreateAlias(name);
            alias = _dataService.GetAlias(name);
        }

        return alias;
    }
}

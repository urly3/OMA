using OMA.Core.Models.Dto;

namespace OMA.Core.Models;


class BlankViewModel
{
    public AliasDto? Dto { get; set; }

    public BlankViewModel(AliasDto? dto)
    {
        Dto = dto;
    }
}

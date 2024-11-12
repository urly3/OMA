using OMA.Core.Models.Dto;

namespace OMA.Core.Models;

internal class BlankViewModel
{
    public BlankViewModel(AliasDto? dto)
    {
        Dto = dto;
    }

    public AliasDto? Dto { get; set; }
}
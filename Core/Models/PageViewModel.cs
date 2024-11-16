using OMA.Core.Models.Dto;

namespace OMA.Core.Models;

internal class PageViewModel(AliasDto dto)
{
    public AliasDto Dto { get; set; } = dto;
    public bool AliasCookieSet { get; set; } = false;
    public bool ValidAlias => Dto.Id != -1;
}
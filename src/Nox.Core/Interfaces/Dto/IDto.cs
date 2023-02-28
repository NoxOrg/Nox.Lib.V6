using Nox.Core.Models;

namespace Nox.Core.Interfaces.Dto;

public interface IDto: IMetaBase
{
    string Name { get; set; }
    string PluralName { get; set; }
    ICollection<DtoAttribute> Attributes { get; set; }
    bool ApplyDefaults();
}
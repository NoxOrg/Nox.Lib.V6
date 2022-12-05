namespace Nox.Core.Interfaces;

public interface IApi: IMetaBase
{
    string Name { get; set; }
    string Description { get; set; }
    ICollection<IApiRoute>? Routes { get; set; }
}
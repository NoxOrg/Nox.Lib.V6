namespace Nox.Core.Interfaces.VersionControl;

public interface ITeamMember: IMetaBase
{
    public string Name { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsProductOwner { get; set; }
}
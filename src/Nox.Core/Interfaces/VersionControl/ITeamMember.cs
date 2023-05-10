namespace Nox.Core.Interfaces.VersionControl;

public interface ITeamMember: IMetaBase
{
    string Name { get; set; }
    string UserName { get; set; }
    string Email { get; set; }
    string MobilePhoneNumber { get; set; }
    bool IsAdmin { get; set; }
    bool IsProductOwner { get; set; }
    
}
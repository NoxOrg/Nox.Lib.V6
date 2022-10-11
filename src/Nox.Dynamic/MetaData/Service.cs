using ETLBoxOffice.LicenseManager;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData;

public sealed class Service : MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string KeyVaultUri { get; set; } = "https://we-key-Nox-02.vault.azure.net/";
    public ServiceDatabase Database { get; set; } = new();
    public ICollection<Entity> Entities { get; set; } = null!;
    public ICollection<Loader> Loaders { get; set; } = null!;
    public ICollection<Api> Apis { get; set; } = null!;

    public void Validate(IReadOnlyDictionary<string,string> configurationVariables)
    {
        var validationInfo = new ServiceValidationInfo(Name,configurationVariables);

        var validator = new ServiceValidator(validationInfo);

        validator.ValidateAndThrow(this);

        LicenseCheck.LicenseKey = configurationVariables["EtlBox:LicenseKey"];

    }
}

internal class ServiceValidator : AbstractValidator<Service>
{
    public ServiceValidator(ServiceValidationInfo info)
    {
        RuleForEach(service => service.Entities)
            .SetValidator(new EntityValidator(info));

        RuleForEach(service => service.Loaders)
            .SetValidator(new LoaderValidator(info));

        RuleForEach(service => service.Apis)
            .SetValidator(new ApiValidator(info));

        RuleFor(service => service.Database)
            .SetValidator(new ServiceDatabaseValidator(info));

        // TODO: Calc SortOrder for entities

    }
}

internal class ServiceValidationInfo
{
    public string ServiceName { get; private set; }
    public IReadOnlyDictionary<string,string> ConfigurationValiables { get; private set; }

    public ServiceValidationInfo(string serviceName, IReadOnlyDictionary<string, string> configurationVariales)
    {
        ServiceName = serviceName;

        ConfigurationValiables = configurationVariales;
    }
}


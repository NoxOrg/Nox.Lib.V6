using FluentValidation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nox.Dynamic.MetaData;

public sealed class Api : MetaBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<ApiRoute> Routes { get; set; } = new Collection<ApiRoute>();
}

internal class ApiValidator : AbstractValidator<Api>
{
    public ApiValidator()
    {

        RuleFor( api => api.Name)
            .NotEmpty()
            .WithMessage(api => $"The api's name must be specified in {api.DefinitionFileName}");

    }
}


using FluentValidation;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Validation;

public class EntitiesValidator: AbstractValidator<List<IEntity>>
{
    public EntitiesValidator()
    {
        RuleForEach(entities => entities)
            .SetValidator(new EntityValidator());
        
    }

    private bool NotBeDuplicate(IEntity entity)
    {
        return false;
    }
}
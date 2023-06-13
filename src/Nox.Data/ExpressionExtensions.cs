using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace Nox.Data;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> GetByIdExpression<T>(this object id)
    {
        var entityType = typeof(T);

        var propInfo = entityType.GetProperties().Where(x => x.Name == "Id").First();

        var propParam = Parameter(entityType);

        var typedId = System.Convert.ChangeType(id, propInfo.PropertyType);

        var expression = Equal(Property(propParam, propInfo), Constant(typedId));

        var lambda = Lambda<Func<T, bool>>(expression, propParam);

        return lambda;
    }

    public static Expression<Func<T, object>> GetPropertyValueExpression<T>(this string propName)
    {
        var entityType = typeof(T);

        var propInfo = entityType.GetProperties().Where(x => x.Name == propName).First();

        var propParam = Parameter(entityType);

        var expression = Property(propParam, propInfo);

        var lambda = Lambda<Func<T, object>>(expression, propParam);

        return lambda;
    }

}

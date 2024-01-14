using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Scar.Common;

public static class ExpressionExtensions
{
    public static PropertyInfo GetProperty<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        var member = GetMemberExpression(expression).Member;
        var property = member as PropertyInfo;
        if (property == null)
        {
            throw new InvalidOperationException($"Member with Name '{member.Name}' is not a property.");
        }

        return property;
    }

    public static Action<TEntity, TProperty> CreateSetter<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> property)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));

        var propertyInfo = GetProperty(property);

        var instance = Expression.Parameter(typeof(TEntity), "instance");
        var parameter = Expression.Parameter(typeof(TProperty), "param");

        var body = Expression.Call(instance, propertyInfo.GetSetMethod() ?? throw new InvalidOperationException("set method is null"), parameter);
        var parameters = new[]
        {
            instance,
            parameter
        };

        return Expression.Lambda<Action<TEntity, TProperty>>(body, parameters).Compile();
    }

    public static Func<TEntity, TProperty> CreateGetter<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> property)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        var propertyInfo = GetProperty(property);

        var instance = Expression.Parameter(typeof(TEntity), "instance");

        var body = Expression.Call(instance, propertyInfo.GetGetMethod() ?? throw new InvalidOperationException("get method is null"));
        var parameters = new[]
        {
            instance
        };

        return Expression.Lambda<Func<TEntity, TProperty>>(body, parameters).Compile();
    }

    static MemberExpression GetMemberExpression<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> expression)
    {
        MemberExpression? memberExpression = null;
        if (expression.Body.NodeType == ExpressionType.Convert)
        {
            var body = (UnaryExpression)expression.Body;
            memberExpression = body.Operand as MemberExpression;
        }
        else if (expression.Body.NodeType == ExpressionType.MemberAccess)
        {
            memberExpression = expression.Body as MemberExpression;
        }

        return memberExpression ?? throw new ArgumentException("Not a member access", nameof(expression));
    }
}
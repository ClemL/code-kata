using System.Globalization;
using System.Linq.Expressions;

namespace Katas.L5.Expressions;

public static class ExpressionKata
{
    public static Expression<Func<T, bool>> PropertyEquals<T>(string propertyName, object? value)
    {
        var x = Expression.Parameter(typeof(T), "x");
        var member = Expression.Property(x, propertyName);

        var targetType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;
        var converted = value is null ? null : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        var constant = Expression.Constant(converted, member.Type);

        return Expression.Lambda<Func<T, bool>>(Expression.Equal(member, constant), x);
    }

    public static Expression<Func<T, bool>> AndAlso<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = left.Parameters[0];
        var rightBody = new ParameterReplacer(right.Parameters[0], parameter).Visit(right.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, rightBody), parameter);
    }

    public static string MemberPath<T>(Expression<Func<T, object?>> selector)
    {
        var body = selector.Body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary
            ? unary.Operand
            : selector.Body;

        var names = new Stack<string>();
        while (body is MemberExpression member)
        {
            names.Push(member.Member.Name);
            body = member.Expression;
        }

        if (body is not ParameterExpression || names.Count == 0)
        {
            throw new ArgumentException("The selector must be a chain of member accesses on the parameter.", nameof(selector));
        }

        return string.Join('.', names);
    }

    public static Func<T, TValue> CompileGetter<T, TValue>(string propertyName)
    {
        var x = Expression.Parameter(typeof(T), "x");
        var member = Expression.Property(x, propertyName);
        var body = member.Type == typeof(TValue) ? (Expression)member : Expression.Convert(member, typeof(TValue));

        return Expression.Lambda<Func<T, TValue>>(body, x).Compile();
    }

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == from ? to : base.VisitParameter(node);
    }
}

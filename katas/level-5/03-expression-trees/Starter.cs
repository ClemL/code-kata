using System.Globalization;
using System.Linq.Expressions;

namespace Katas.L5.Expressions;

public static class ExpressionKata
{
    public static Expression<Func<T, bool>> PropertyEquals<T>(string propertyName, object? value) =>
        throw new NotImplementedException();

    public static Expression<Func<T, bool>> AndAlso<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right) =>
        throw new NotImplementedException();

    public static string MemberPath<T>(Expression<Func<T, object?>> selector) => throw new NotImplementedException();

    public static Func<T, TValue> CompileGetter<T, TValue>(string propertyName) => throw new NotImplementedException();
}

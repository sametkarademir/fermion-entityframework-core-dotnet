using Fermion.EntityFramework.Core.Models;
using System.Linq.Dynamic.Core;

namespace Fermion.EntityFramework.Core.Extensions;

public static class QueryableSortExtensions
{
    public static IQueryable<T> ToSort<T>(this IQueryable<T> source, IList<Sort> sorts)
    {
        foreach (Sort item in sorts)
        {
            if (string.IsNullOrEmpty(item.Field))
                throw new ArgumentException("Invalid Field");
        }

        if (sorts.Any())
        {
            string ordering = string.Join(separator: ",", values: sorts.Select(s => $"{s.Field} {s.SortType.ToString()}"));
            return source.OrderBy(ordering);
        }

        return source;
    }
}
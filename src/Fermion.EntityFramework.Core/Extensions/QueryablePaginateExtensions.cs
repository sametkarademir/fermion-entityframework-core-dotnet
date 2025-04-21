using Fermion.EntityFramework.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Fermion.EntityFramework.Core.Extensions;

public static class QueryablePaginateExtensions
{
    public static async Task<Paginate<T>> ToPaginateAsync<T>(
        this IQueryable<T> source,
        int index,
        int size,
        CancellationToken cancellationToken = default
    )
    {
        var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);

        if (count == 0)
        {
            return new Paginate<T>
            {
                Size = size,
                Index = index,
                Count = count,
                Pages = 0,
                Items = new List<T>()
            };
        }

        var items = await source.Skip(index * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
        var list = new Paginate<T>
        {
            Size = size,
            Index = index,
            Count = count,
            Pages = (int)Math.Ceiling(count / (double)size),
            Items = items
        };
        return list;
    }

    public static Paginate<T> ToPaginate<T>(
        this IQueryable<T> source,
        int index,
        int size,
        CancellationToken cancellationToken = default
    )
    {
        var count = source.Count();

        if (count == 0)
        {
            return new Paginate<T>
            {
                Size = size,
                Index = index,
                Count = count,
                Pages = 0,
                Items = new List<T>()
            };
        }

        var items = source.Skip(index * size).Take(size).ToList();
        var list = new Paginate<T>
        {
            Size = size,
            Index = index,
            Count = count,
            Pages = (int)Math.Ceiling(count / (double)size),
            Items = items
        };
        return list;
    }
}
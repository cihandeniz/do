using System.Linq.Expressions;

namespace Baked.Orm;

public interface IQueryContext<TEntity>
{
    TEntity SingleById(Guid id, bool throwNotFound = false);
    IQueryable<TEntity> Query();

    public List<TEntity> ByIds(IEnumerable<Guid> ids) =>
        ids.Select(id => SingleById(id)).ToList();

    public bool AnyBy(Expression<Func<TEntity, bool>> where) =>
        Query(where).Any();

    public TEntity? SingleBy(Expression<Func<TEntity, bool>> where) =>
        Query(where).SingleOrDefault();

    public TEntity? FirstBy(Expression<Func<TEntity, bool>> where) =>
        Query(where).FirstOrDefault();

    public TEntity? FirstBy<TOrderBy>(Expression<Func<TEntity, bool>> where,
        Expression<Func<TEntity, TOrderBy>>? orderBy = default,
        Expression<Func<TEntity, TOrderBy>>? orderByDescending = default
    ) => Query(where,
            orderBy: orderBy,
            orderByDescending: orderByDescending
         ).FirstOrDefault();

    public List<TEntity> By(Expression<Func<TEntity, bool>> where,
        int? take = null,
        int? skip = null
    )
    {
        var result = Query(where,
            take: take,
            skip: skip
        );

        return [.. result];
    }

    public List<TEntity> By<TOrderBy>(Expression<Func<TEntity, bool>> where,
        Expression<Func<TEntity, TOrderBy>>? orderBy = default,
        Expression<Func<TEntity, TOrderBy>>? orderByDescending = default,
        int? take = null,
        int? skip = null
    )
    {
        var result = Query(where,
            orderBy: orderBy,
            orderByDescending: orderByDescending,
            take: take,
            skip: skip
         );

        return [.. result];
    }

    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> where,
        int? take = null,
        int? skip = null
    )
    {
        var query = Query().Where(where);

        if (take is not null)
        {
            query = query.Take(take.Value);
        }

        if (skip is not null)
        {
            query = query.Skip(skip.Value);
        }

        return query;
    }

    IQueryable<TEntity> Query<TOrderBy>(Expression<Func<TEntity, bool>> where,
        Expression<Func<TEntity, TOrderBy>>? orderBy = default,
        Expression<Func<TEntity, TOrderBy>>? orderByDescending = default,
        int? take = null,
        int? skip = null
    )
    {
        var query = Query().Where(where);

        if (orderBy is not null)
        {
            query = query.OrderBy(orderBy);
        }

        if (orderByDescending is not null)
        {
            query = query.OrderByDescending(orderByDescending);
        }

        if (take is not null)
        {
            query = query.Take(take.Value);
        }

        if (skip is not null)
        {
            query = query.Skip(skip.Value);
        }

        return query;
    }
}
using FluentResults;
using SqlKata;

namespace FluentCMS.Utils.QueryBuilder;

public static class QueryExt
{
    public static void ApplyPagination(this Query query, Pagination? pagination)
    {
        if (pagination is null)
        {
            return;
        }

        query.Offset(pagination.Offset).Limit(pagination.Limit);
    }
    public static void ApplySorts(this Query query, Sorts? sorts)
    {
        if (sorts is null)
        {
            return;
        }

        foreach (var sort in sorts)
        {
            if (sort.Order == SortOrder.Desc)
            {
                query.OrderByDesc(sort.FieldName);
            }
            else
            {
                query.OrderBy(sort.FieldName);
            }
        }
    }

    public static Result ApplyFilters(this Query query, Filters? filters)
    {
        var result = Result.Ok();
        if (filters is null) return result;
        foreach (var filter in filters)
        {
            query.Where(q =>
            {
                foreach (var c in filter.Constraints)
                {
                    var ret = filter.IsOr
                        ? q.ApplyOrConstraint(filter.FieldName, c.Match, c.ResolvedValues)
                        : q.ApplyAndConstraint(filter.FieldName, c.Match, c.ResolvedValues);
                    if (ret.IsFailed)
                    {
                        result = Result.Fail(ret.Errors);
                        break;
                    }
                }

                return q;
            });
        }

        return result;
    }

    private static Result<Query> ApplyAndConstraint(this Query query, string field, string match, object[] values)
    {
        return match switch
        {
            Matches.StartsWith => query.WhereStarts(field, values[0]),
            Matches.Contains => query.WhereContains(field, values[0]),
            Matches.NotContains => query.WhereNotContains(field, values[0]),
            Matches.EndsWith => query.WhereEnds(field, values[0]),
            Matches.EqualsTo => query.Where(field, values[0]),
            Matches.NotEquals => query.WhereNot(field, values[0]),
            Matches.NotIn => query.WhereNotIn(field, values),
            Matches.In => query.WhereIn(field, values),
            Matches.Lt => query.Where(field, "<", values[0]),
            Matches.Lte => query.Where(field, "<=", values[0]),
            Matches.Gt => query.Where(field, ">", values[0]),
            Matches.Gte => query.Where(field, ">=", values[0]),
            Matches.DateIs => query.WhereDate(field, values[0]),
            Matches.DateIsNot => query.WhereNotDate(field, values[0]),
            Matches.DateBefore => query.WhereDate(field, "<", values[0]),
            Matches.DateAfter => query.WhereDate(field, ">", values[0]),
            Matches.Between => values?.Length == 2
                ? query.WhereBetween(field, values[0], values[1])
                : Result.Fail("show provide two values for between"),
            _ => Result.Fail($"{match} is not support ")
        };
    }

    private static Result<Query> ApplyOrConstraint(this Query query, string field, string match, object[] values)
    {
        return match switch
        {
            Matches.StartsWith => query.OrWhereStarts(field, values[0]),
            Matches.Contains => query.OrWhereContains(field, values[0]),
            Matches.NotContains => query.OrWhereNotContains(field, values[0]),
            Matches.EndsWith => query.OrWhereEnds(field, values[0]),
            Matches.EqualsTo => query.OrWhere(field, values[0]),
            Matches.NotEquals => query.OrWhereNot(field, values[0]),
            Matches.NotIn => query.OrWhereNotIn(field, values),
            Matches.In => query.OrWhereIn(field, values),
            Matches.Lt => query.OrWhere(field, "<", values[0]),
            Matches.Lte => query.OrWhere(field, "<=", values[0]),
            Matches.Gt => query.OrWhere(field, ">", values[0]),
            Matches.Gte => query.OrWhere(field, ">=", values[0]),
            Matches.DateIs => query.OrWhereDate(field, values[0]),
            Matches.DateIsNot => query.OrWhereNotDate(field, values[0]),
            Matches.DateBefore => query.OrWhereDate(field, "<", values[0]),
            Matches.DateAfter => query.OrWhereDate(field, ">", values[0]),
            Matches.Between => values?.Length == 2
                ? query.OrWhereBetween(field, values[0], values[1])
                : Result.Fail("show provide two values for between"),
            _ => Result.Fail($"{match} is not support ")
        };
    }

    public static Result ApplyCursor(this Query? query,  Cursor? cursor,Sorts? sorts)
    {
        if (query is null || cursor?.BoundaryItem is null) return Result.Ok();
        return sorts?.Count switch
        {
            0 => Result.Fail("Sorts was not provided, can not perform cursor filter"),
            1 => HandleOneField(),
            2 => HandleTwoFields(),
            _=> Result.Fail("More than two field in sorts is not supported")
        };

        Result HandleOneField()
        {
            ApplyCompare(query,sorts[0]);
            return Result.Ok();
        }

        Result HandleTwoFields()
        {
            var (first,last) = (sorts.First(),sorts.Last());
            query.Where(q =>
            {
                ApplyCompare(q, first);
                q.Or();
                ApplyEq(q, first);
                ApplyCompare(q, last);
                return q;
            });
            return Result.Ok();
        }

        void ApplyEq(Query q, Sort sort)
        {
            q.Where(sort.FieldName, cursor.BoundaryValue(sort.FieldName));
        }
        void ApplyCompare(Query q, Sort sort)
        {
            q.Where(sort.FieldName, cursor.GetCompareOperator(sort), cursor.BoundaryValue(sort.FieldName));
        }

    }
}
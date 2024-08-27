using FluentCMS.Services;
using FluentCMS.Utils.KateQueryExecutor;
using FluentCMS.Utils.QueryBuilder;
using Microsoft.Extensions.Primitives;
using FluentCMS.Utils.Cache;
using FluentCMS.Utils.HookFactory;
namespace FluentCMS.Cms.Services;

using static InvalidParamExceptionFactory;

public class ViewService(
    KateQueryExecutor kateQueryExecutor,
    ISchemaService schemaService,
    IEntityService entityService,
    ImmutableCache<View> viewCache,
    IServiceProvider provider,
    HookRegistry hookRegistry
) : IViewService
{

    public async Task<RecordViewResult> List(string viewName, Cursor cursor,
        Dictionary<string, StringValues> querystringDictionary, CancellationToken cancellationToken)
    {
        var view = await GetView(viewName, cancellationToken);
        CheckResult(view.Filters.ResolveValues(view.Entity!,  schemaService.CastToDatabaseType, querystringDictionary));
        CheckResult(cursor.ResolveBoundaryItem(view.Entity!, schemaService.CastToDatabaseType));
        var pagination = new Pagination { Limit = view.PageSize + 1 }; //get extra record to check if it's the last page
        var (exit, hookResult) = await TriggerHook(Occasion.BeforeQueryView, view, view.Filters, view.Sorts, cursor, pagination);
        if (exit)
        {
            return BuildRecordViewResult(hookResult.Records, cursor, pagination, view.Sorts);
        }

        var attributes = CheckResult(view.LocalAttributes(InListOrDetail.InList));
        var query = CheckResult(view.Entity!.ListQuery(view.Filters, view.Sorts, pagination, cursor, attributes,
            schemaService.CastToDatabaseType));
        var items = await kateQueryExecutor.Many(query, cancellationToken);
        var results = BuildRecordViewResult(items, cursor, pagination, view.Sorts);
        if (results.Items is not null && results.Items.Length > 0)
        {
            //here use result.Items instead of items, because result.Items omit last record
            await AttachRelatedEntity(view, InListOrDetail.InList, results.Items, cancellationToken);
        }

        return results;
    }

    public async Task<Record[]> Many(string viewName, Dictionary<string, StringValues> querystringDictionary,
        CancellationToken cancellationToken)
    {
        var view = await GetView(viewName, cancellationToken);
        CheckResult(view.Filters.ResolveValues(view.Entity!,  schemaService.CastToDatabaseType, querystringDictionary));

        var (exit, hookResult) = await TriggerHook(Occasion.BeforeQueryManyView,view, view.Filters);
        if (exit)
        {
            return hookResult.Records;
        }

        var attributes = CheckResult(view.LocalAttributes(InListOrDetail.InDetail));
        var pagination = new Pagination { Limit = view.PageSize };
        var query = CheckResult(view.Entity!.ListQuery(view.Filters, null, pagination, null, attributes,
            schemaService.CastToDatabaseType));
        var items = await kateQueryExecutor.Many(query, cancellationToken);
        await AttachRelatedEntity(view, InListOrDetail.InDetail, items, cancellationToken);
        return items;
    }

    public async Task<Record> One(string viewName, Dictionary<string, StringValues> querystringDictionary,
        CancellationToken cancellationToken)
    {
        var view = await GetView(viewName, cancellationToken);
        CheckResult(view.Filters.ResolveValues(view.Entity!,  schemaService.CastToDatabaseType, querystringDictionary));
        var (exit, hookResult) = await TriggerHook(Occasion.BeforeQueryOneView,view, view.Filters);
        if (exit)
        {
            return hookResult.Record;
        }

        var attributes = CheckResult(view.LocalAttributes(InListOrDetail.InDetail));
        var query = CheckResult(view.Entity!.OneQuery(view.Filters, attributes));
        var item = NotNull(await kateQueryExecutor.One(query, cancellationToken)).ValOrThrow("Not find record");
        await AttachRelatedEntity(view, InListOrDetail.InDetail, [item], cancellationToken);
        return item;
    }

    private async Task<(bool, HookReturn)> TriggerHook(Occasion occasion,View view, Filters filters, Sorts? sorts = null,
        Cursor? cursor = null, Pagination? pagination = null)
    {
        var hookParam = new HookParameter
            { Filters = filters, Sorts = sorts, Cursor = cursor, Pagination = pagination };
        var hookReturn = new HookReturn();
        var exit = await hookRegistry.Trigger(provider, occasion, view, hookParam, hookReturn);
        return (exit, hookReturn);
    }

    private async Task AttachRelatedEntity(View view, InListOrDetail scope, Record[] items,
        CancellationToken cancellationToken)
    {
        foreach (var attribute in CheckResult(view.GetAttributesByType(DisplayType.lookup, scope)))
        {
            await entityService.AttachLookup(attribute, items, cancellationToken,
                entity1 => entity1.LocalAttributes(scope));
        }

        foreach (var attribute in CheckResult(view.GetAttributesByType(DisplayType.crosstable, scope)))
        {
            await entityService.AttachCrosstable(attribute, items, cancellationToken,
                entity1 => entity1.LocalAttributes(scope));
        }
    }

    private async Task<View> GetView(string viewName, CancellationToken cancellationToken)
    {
        var view = await viewCache.GetOrSet(viewName,
            async () => await schemaService.GetViewByName(viewName, cancellationToken));
        return view;
    }

    private RecordViewResult BuildRecordViewResult(Record[] items, Cursor cursor, Pagination pagination, Sorts? sorts)
    {
        if (items.Length == 0)
        {
            return new RecordViewResult();
        }
        
        var hasMore = items.Length == pagination.Limit;
        if (hasMore)
        {
            items = cursor.First != ""
                ? items.Skip(1).Take(items.Length - 1).ToArray()
                : items.Take(items.Length - 1).ToArray();
        }

        var nextCursor = CheckResult(cursor.GetNextCursor(items, sorts, hasMore));

        return new RecordViewResult
        {
            Items = items,
            First = nextCursor.First,
            HasPrevious = !string.IsNullOrWhiteSpace(nextCursor.First),
            Last = nextCursor.Last,
            HasNext = !string.IsNullOrWhiteSpace(nextCursor.Last)
        };
    }
}
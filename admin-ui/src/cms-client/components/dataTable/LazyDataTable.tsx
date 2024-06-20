import {DataTable} from "primereact/datatable";

export function LazyDataTable({columns, data, dataKey, lazyState, eventHandlers, createColumn, getFullURL}: {
    data: { items: any[], totalRecords: number }
    dataKey: any
    lazyState: any
    eventHandlers: any
    createColumn: any
    columns: any[]
    getFullURL : (arg:string) =>string
}) {
    const {items, totalRecords} = data ?? {}
    return columns && data && <DataTable
        sortMode="multiple"
        dataKey={dataKey}
        value={items}
        paginator
        totalRecords={totalRecords}
        rows={lazyState.rows}
        lazy
        first={lazyState.first}
        filters={lazyState.filters}
        multiSortMeta={lazyState.multiSortMeta}
        sortField={lazyState.sortField}
        sortOrder={lazyState.sortOrder}
        {...eventHandlers}
    >
        {columns.map((column: any, i: number) => createColumn({column, dataKey,getFullURL}))}
    </DataTable>
}
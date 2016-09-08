using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataTablesNetSS
{
    /// <summary>
    /// Object that DataTables sends, in the request to your endpoint
    /// </summary>
    public class DataTablesIn
    {
        /// <summary>
        /// The MVC Model binder is simply not good enough to parse the structure passed to us by datatables. We make up for it by passing a JSON string
        /// and parsing it using NewtonSoft JSON.
        /// </summary>
        /// <param name="InStr"></param>
        /// <returns></returns>
        public static DataTablesIn ParseJSONString(string InStr)
        {
            return JsonConvert.DeserializeObject<DataTablesIn>(InStr);
        }
        /// <summary>
        /// Do not expose externally, MVC model binder can't bind it anyways...make people use the factory method
        /// </summary>
        internal DataTablesIn()
        {
            columns = new List<DTColumn>();
            order = new List<DTOrder>();
        }
        /// <summary>
        /// Draw counter. This is used by DataTables to ensure that the Ajax returns from server-side processing requests are drawn in sequence by DataTables 
        /// (Ajax requests are asynchronous and thus can return out of sequence). This is used as part of the draw return parameter (see below).DONT MESS WITH THIS IF YOU WANT DATATABLES TO WORK PROPERLY
        /// </summary>
        public int draw { get; set; }
        /// <summary>
        /// Paging first record indicator. This is the start point in the current data set (0 index based - i.e. 0 is the first record).
        /// 
        /// </summary>
        public int start { get; set; }
        /// <summary>
        /// Number of records that the table can display in the current draw. It is expected that the number of records returned will be equal to this number,
        /// unless the server has fewer records to return. 
        /// Note that this can be -1 to indicate that all records should be returned (although that negates any benefits of server-side processing!)
        /// </summary>
        public int length { get; set; }
        /// <summary>
        /// Global search value. To be applied to all columns which have searchable as true.
        /// </summary>
        public DTSearch search { get; set; }
        /// <summary>
        /// List of columns returned in DataTables order.
        /// </summary>
        public List<DTColumn> columns { get; set; }
        public List<DTOrder> order { get; set; }
    }
    /// <summary>
    /// Column sort direction. This is an Enum for security reasons (to prevent n00bs from allowing SQL injection)
    /// </summary>
    public enum SortDirection
    {
        asc, desc
    }
    /// <summary>
    /// Search info.
    /// </summary>
    public class DTSearch
    {
        /// <summary>
        /// search string value. You might want to trim this?
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// Is this search using Regex? (not recommended for large datasets)
        /// </summary>
        public bool regex { get; set; }
    }
    public class DTOrder
    {
        /// <summary>
        /// Column to which ordering should be applied. This is an index reference to the columns array of information that is also submitted to the server.
        /// </summary>
        public int column { get; set; }
        /// <summary>
        /// Ordering direction for this column.
        /// </summary>
        public SortDirection dir { get; set; }
    }
    /// <summary>
    /// Column Array 
    /// </summary>
    public class DTColumn
    {
        /// <summary>
        /// Column's data source, as defined by columns.data Option within DataTables.
        /// </summary>
        public string data { get; set; }
        /// <summary>
        /// Column's name, as defined by columns.name Option within DataTables.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Flag to indicate if this column is searchable (true) or not (false). This is controlled by the columns.searchable Option within Datatables.
        /// </summary>
        public bool searchable { get; set; }
        /// <summary>
        /// Flag to indicate if this column is orderable (true) or not (false). This is controlled by the columns.searchable Option within Datatables.
        /// </summary>
        public bool orderable { get; set; }
        /// <summary>
        /// Search value to apply to this specific column.
        /// </summary>
        public DTSearch search { get; set; }


    }
    
    
}

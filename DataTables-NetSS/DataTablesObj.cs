
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System;
using Newtonsoft.Json;
using System.IO;

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
    public class DataTablesResult : ActionResult
    {
        /// <summary>
        /// The draw counter that this object is a response to - from the draw parameter sent as part of the data request. 
        /// </summary>
        public int? draw { get; set; }
        /// <summary>
        /// Total records, before filtering (i.e. the total number of records in the database)
        /// </summary>
        public int? recordsTotal { get; set; }
        /// <summary>
        /// Total records, after filtering (i.e. the total number of records after filtering has been applied - not just the number of records being returned for this page of data).
        /// </summary>
        public int? recordsFiltered { get; set; }
        /// <summary>
        /// The data to be displayed in the table. This is an array of data source objects, one for each row, which will be used by DataTables. Note that this parameter's name can be changed using the ajax option's dataSrc property. (don't do it when using this library, the model binding will not longer work)
        /// </summary>
        public IList<string[]> data { get; set; }
        /// <summary>
        /// Optional: If an error occurs during the running of the server-side processing script, you can inform the user of this error by passing back the error message to be displayed using this parameter. Do not include if there is no error.
        /// </summary>
        public string error { get; set; }
        public DataTablesResult()
        {
           
        }
        /// <summary>
        /// do some sanity checks to ease debugging
        /// </summary>
        private void CheckValidFields()
        {
            if (draw == null) throw new NullReferenceException("draw is null. Make sure to pass back the same draw value that Datatables passed to you in the DataTablesIn object. Datatables will not work properly without passing this, refusing to continue.");
            if (recordsTotal == null) throw new NullReferenceException("recordsTotal is null. Datatables will not work properly without passing this, refusing to continue. ");
            if (recordsFiltered == null) throw new NullReferenceException("recordsFiltered is null. Datatables will not work properly without passing this, refusing to continue. ");
            if (Object.ReferenceEquals(null, data)) throw new NullReferenceException("data object is null, cannot serialize.");
        }
        public override void ExecuteResult(ControllerContext context)
        {
            CheckValidFields();
            var response=context.HttpContext.Response;
            response.ContentType = "application/json";
            var OutputStream = response.OutputStream;
            _serializeJson(this, OutputStream);
        }
        /// <summary>
        /// copied shamelessly from StackOverflow 
        /// http://stackoverflow.com/questions/31796352/serializing-to-memorystream-causes-an-outofmemoryexception-but-serializing-to-a
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        private static void _serializeJson(DataTablesResult obj, Stream stream)
        {
            try
            {
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                using (JsonWriter jw = new JsonTextWriter(streamWriter))
                {
                    var serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(jw, obj);
                }
            }
            catch (Exception e)
            {
                throw new DataTablesResultSerializationException("Error Serializing Custom Datatype. See InnerException for details.",e);
            }
        }
        internal class DataTablesResultSerializationException : Exception
        {
            public DataTablesResultSerializationException()
            {
            }

            public DataTablesResultSerializationException(string message)
                : base(message)
            {
            }

            public DataTablesResultSerializationException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
    }
    
}

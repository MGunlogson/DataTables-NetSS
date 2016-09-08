using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.IO;

namespace DataTablesNetSS
{
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
            var response = context.HttpContext.Response;
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
                throw new DataTablesResultSerializationException("Error Serializing Custom Datatype. See InnerException for details.", e);
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

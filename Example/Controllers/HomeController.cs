using DataTablesNetSS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataTablesTest.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// returns the datatable html and script
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// The server side ajax call
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public ActionResult DataTable(string myDataTableParameter)
        {
            //we pass in table as a JSON string because the MVC model binder is utterly incapable of parsing datatables output.
            //see Index view for client side changes needed to make this happen
            var TableIn = DataTablesIn.ParseJSONString(myDataTableParameter);
            var Data = DataTableTestData.GetTestData();
            var columns = TableIn.columns;
            //if you're doing LINQ to entities you will want to use AsQueryable to avoid doing actual DB query until you're done building it.
            var DataQry = Data.AsEnumerable();
            for (int ColNum = 0; ColNum < columns.Count(); ColNum++)
            {
                //per-column search. DataTables uses columns without names so we need to do everything by index
                if (columns[ColNum].searchable)
                {
                    var searchvalue = columns[ColNum].search.value.ToLower();
                    if (String.IsNullOrWhiteSpace(searchvalue)) continue;
                    switch (ColNum)
                    {
                        //first name
                        case 0:
                            //nulls won't be an issue in most LINQ to entity configs
                            //also ToLower() is only needed on LINQ to objects...database entities are case insensitive using contains
                            DataQry = DataQry.Where(x => x.FirstName!=null && x.FirstName.ToLower().Contains(searchvalue));
                            break;
                        //last name
                        case 1:
                            DataQry = DataQry.Where(x => x.LastName != null && x.LastName.ToLower().Contains(searchvalue));
                            break;
                        //company 
                        case 2:
                            DataQry = DataQry.Where(x => x.Company != null && x.Company.ToLower().Contains(searchvalue));
                            break;
                        //city
                        case 3:
                            DataQry = DataQry.Where(x => x.City != null && x.City.ToLower().Contains(searchvalue));
                            break;
                        //start date
                        case 4:
                            //these SqlFunctions are great...they let us use Entity framework to treat datatypes as strings...great for searching...possibly slow :-/
                            //DataQry = DataQry.Where(x => SqlFunctions.DateName("mm/dd/yyyy", x.StartDate).Contains(searchvalue));
                            //they only work in LINQ to entities so we won't use them for this test
                            DataQry = DataQry.Where(x => x.StartDate.ToShortDateString().Contains(searchvalue));
                            break;
                        //salary
                        case 5:
                            //again, in LINQ to Entities you need to use SqlFunctions class for conversion                            
                            //DataQry = DataQry.Where(x => SqlFunctions.StringConvert(x.Salary).Contains(searchvalue));
                            DataQry = DataQry.Where(x => x.Salary.ToString().Contains(searchvalue));
                            break;
                    }
                }
            }
            //ordering
            //I'm not sure what the use-case of multi col ordering is in datatables, so just grab first result
            var ordercol = TableIn.order.First();
            var dir = ordercol.dir;
            switch (ordercol.column)
            {
                //you might want to check columns[]Orderable as well if you disable ordering for some client side
                //first name
                case 0:
                    DataQry = (dir == SortDirection.asc) ? DataQry.OrderBy(x => x.FirstName) : DataQry.OrderByDescending(x => x.FirstName);
                    break;
                //last name
                case 1:
                    DataQry = (dir == SortDirection.asc) ? DataQry.OrderBy(x => x.LastName) : DataQry.OrderByDescending(x => x.LastName);
                    break;
                //company 
                case 2:
                    DataQry = (dir == SortDirection.asc) ? DataQry.OrderBy(x => x.Company) : DataQry.OrderByDescending(x => x.Company);
                    break;
                //city
                case 3:
                    DataQry = (dir == SortDirection.asc) ? DataQry.OrderBy(x => x.City) : DataQry.OrderByDescending(x => x.City);
                    break;
                //start date
                case 4:
                    DataQry = (dir == SortDirection.asc) ? DataQry.OrderBy(x => x.StartDate) : DataQry.OrderByDescending(x => x.StartDate);
                    break;
                //salary
                case 5:
                    DataQry = (dir == SortDirection.asc) ? DataQry.OrderBy(x => x.Salary) : DataQry.OrderByDescending(x => x.Salary);
                    break;
            }
            //recordsFiltered should be set to # of records from query before paging is applied
            var recordsFiltered = DataQry.Count();

            //paging support
            DataQry = DataQry.Skip(TableIn.start);
            //need to handle "return all records" (-1)
            if (TableIn.length != -1)
            {
                DataQry = DataQry.Take(TableIn.length);
            }

            var QryResult = DataQry.ToList();
            return new DataTablesResult
            {
                draw = TableIn.draw,
                recordsTotal = Data.Count(),//dataset size before any searching/filtering
                recordsFiltered = recordsFiltered, //after filtering, before paging
                //pay close attention to the data structure. Json parser need an array without property names for each object. Make sure everything is ToStringed()
                data = QryResult.Select(x => new[] { x.FirstName, x.LastName, x.Company, x.City, x.StartDate.ToShortDateString(), x.Salary.ToString() }).ToList()
            };
        }

    }
    public class DataTableTestData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string City { get; set; }
        public DateTime StartDate { get; set; }
        public decimal Salary { get; set; }
        public static List<DataTableTestData> GetTestData()
        {
            if (_TestData == null)
            {
                string b = HttpContext.Current.Request.MapPath("~/data.json");
                // read JSON directly from a file
                _TestData= JsonConvert.DeserializeObject<List<DataTableTestData>>(File.ReadAllText(b));
            }
            return _TestData;
        }
        //singleton
        private static List<DataTableTestData> _TestData {get;set;}



    }
}
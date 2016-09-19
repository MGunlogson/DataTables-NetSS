THE DOCUMENTATION
=======================

Intro
-----------------

This is a set of helpers for communicating with JQuery Datatables. They can be used to access native DataTables server-side Ajax objects, which are normally difficult to use with ASP.NET MVC. This library doesn't have any "magic" inside like other DataTables binders, it's simple by design.

Usage
-------------------
It's easiest to start with the provided example code, but we use some options like per-column search and the code is pretty verbose. Below is the minimum DataTables initialization to forward the correct data to the server, which only supports paging.

Demo and Example Code
------------------
* Example code is included within the solution in this repo.
* For production use you'll probably want to install the [nuget package.](https://www.nuget.org/packages/DataTablesNetSS/)
* Example code demo is running [here.](http://netss.gunlogson.com/)

View (client side)
-------------------
```javascript
var table = $('#example').DataTable({
    "serverSide": true,
    "ajax": {
        //ALWAYS USE POST. Sometimes DataTables will pass you a big object and you will pass query string size limit
        "type": "POST",
        "url": "@Url.Action("Your Controller Action")",
        "data": function (d) {
			//we tell datatables to pass its object inside of a JSON string because MVC can't bind it properly.
            return { myDataTableParameter: JSON.stringify(d) };
        }
    }
});
```

Controller Action
--------------------
```cs
[HttpPost]//always use post, see client side call in index view
public ActionResult AjaxDtAction(string myDataTableParameter)
{
    //we pass in table as a JSON string because the MVC model binder is utterly incapable of parsing datatables output.
    //Parse into Datatables structure
    var TableIn = DataTablesIn.ParseJSONString(myDataTableParameter);
    //load from database or wherever
    IQueryable<DataItem> DataQry= GetData();
	
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
        //DONT MESS WITH THE DRAW PARAMETER
        draw = TableIn.draw,
        recordsTotal = DataQry.Count(),//dataset size before any searching/filtering
        recordsFiltered = recordsFiltered, //size after filtering, before paging.
        //pay close attention to the data structure. Json parser need an array without property names for each object. Make sure everything is ToStringed()
        data = QryResult.Select(x => new[] {x.field1 ,x.field2 }).ToList()
    };
}
```

Caveats
--------------------
* Make sure to pass the Datatables object to MVC through a JSON string! This means overriding the default Ajax call done by datatables, see above.
* Be mindful of the return datastructure back to datatables. The data parameter of your DataTablesResult should be a a list of arrays, with each array element one of your table fields.


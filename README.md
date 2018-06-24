# Postulate.Lite

[![Build status](https://ci.appveyor.com/api/projects/status/ug7499knw4ut33yj/branch/master?svg=true)](https://ci.appveyor.com/project/adamosoftware/postulate-lite/branch/master)

I wanted to build an ORM that has no particular root type dependency (i.e. [Record](https://github.com/adamosoftware/Postulate.Orm/blob/master/Core/Abstract/Record.cs)) and that would work as `IDbConnection` extension methods, the way [Dapper.SimpleCRUD](https://github.com/ericdc1/Dapper.SimpleCRUD) does.

I intend to support [Model Merge](https://github.com/adamosoftware/Postulate.Orm/wiki/Model-Merge) capability eventually, but at the moment I'm just getting my new core Crud methods working.

Postulate.Lite will support both SQL Server and MySQL.

Please see the [Wiki](https://github.com/adamosoftware/Postulate.Lite/wiki) for more info.

## Nuget

- SQL Server package: **Postulate.Lite.SqlServer**
- MySql package: *coming soon*

## How to Use

- Create any number of model classes that correspond to your database tables. They can be POCO, but added functionality is available if you inherit from [Postulate.Lite.Core.Record](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/Record.cs). The only design requirement for Postulate.Lite model classes is that either they have an [[Identity]](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/Attributes/IdentityAttribute.cs) attribute that defines the primary key property, or they have a property called **Id** with no particular attribute decoration.

- Open your `IDbConnection` object in whatever way is appropriate for your application, normally within a `using` block.

- Use any of Postulate.Lite Crud extension methods of `IDbConnection` defined in [ConnectionExtensions](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.SqlServer/ConnectionExtensions.cs): Find, FindWhere, Save, Insert, Update, Delete. They all accept a `TModel` generic argument corresponding to your model class.

- All of the Crud methods accept an `IUser` optional argument you can use to pass the current user name and access to the user's local time. This argument takes effect if your model class is based on `Record` (see above), which offers a number of overrides for checking permissions and executing row-level events, among other things.

## Examples

A simple find using the [Find](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/CommandProvider.cs#L246) method:

```
using (var cn = GetConnection())
{
  var e = cn.Find<Employee>(2322);
  Console.WriteLine(e.FirstName);
}
```

Find using criteria with the [FindWhere](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/CommandProvider.cs#L262) method:
```
using (var cn = GetConnection())
{
  var e = cn.FindWhere(new Employee() { OrganizationId = 12, Number = 3988 });
  Console.WriteLine(e.FirstName);
}
```

Create and save a record with the [Save](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/CommandProvider.cs#L225) method.
```
using (var cn = GetConnection())
{
  var e = new Employee()
  {
    FirstName = "Thomas",
    LastName = "Whoever",
    HireDate = new DateTime(2012, 1, 1)
  };
  cn.Save<Employee>(e);
  Console.WriteLine(e.Id.ToString());
}
```

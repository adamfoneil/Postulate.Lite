# Postulate.Lite ORM

[![Build status](https://ci.appveyor.com/api/projects/status/ug7499knw4ut33yj/branch/master?svg=true)](https://ci.appveyor.com/project/adamosoftware/postulate-lite/branch/master)

Postulate.Lite is an ORM built around [Dapper](https://github.com/StackExchange/Dapper) that performs CRUD operations on your model types. It's an evolution of my [Postulate.Orm](https://github.com/adamosoftware/Postulate.Orm) project that is
- more POCO-friendly, having no base type dependency
- easier to use thanks to extension methods (inspired by [Dapper.SimpleCRUD](https://github.com/ericdc1/Dapper.SimpleCRUD))
- has a more robust, general-purpose schema merge capability, using my [SchemaSync project](https://github.com/adamosoftware/SchemaSync)
- has a free app [Postulate Query Helper](https://github.com/adamosoftware/Postulate.Zinger) and a commercial app [SQL Model Merge](https://aosoftware.net/Project/SqlModelMerge) for merging model class and database changes

Postulate.Lite is not a Linq replacement. In my applications, I use inline SQL with Postulate.Lite's [Query&lt;T&gt;](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/Query.cs) type. Please see the [wiki](https://github.com/adamosoftware/Postulate.Lite/wiki/Using-the-Query-class) page on this.

## Nuget

- SQL Server package: **Postulate.Lite.SqlServer**
- MySql package: **Postulate.Lite.MySql**

## How to Use

- Create any number of model classes that correspond to your database tables. They can be POCO, but added functionality is available if you inherit from [Postulate.Lite.Core.Record](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/Record.cs). The only design requirement for Postulate.Lite model classes is that either they have an [[Identity]](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/Attributes/IdentityAttribute.cs) attribute that defines the primary key property, or they have a property called **Id** with no particular attribute decoration. You can also use the [[PrimaryKey]](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/Attributes/PrimaryKeyAttribute.cs) attribute on select properties to define an explicit primary key.

- Use the [[References]](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/Attributes/ReferencesAttribute.cs) attribute to define foreign keys on select properties.

- For Sql Server, Postulate.Lite supports `int`, `Guid`, and `long` identity types. MySql currently supports `int`. When creating your model classes, decide on an identity type and be consistent across all your model classes.

- Open your `IDbConnection` object in whatever way is appropriate for your application, normally within a `using` block.

- Use any of the Postulate.Lite Crud extension methods of `IDbConnection`: Find, FindWhere, Save, Insert, Update, Delete, Exists, and ExistsWhere. They all accept a `TModel` generic argument corresponding to your model class. In the SQL Server package, there are three different namespaces with a static `ConnectionExtensions` class that provides the crud methods: [Postulate.Lite.SqlServer.IntKey](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.SqlServer/IntKey/ConnectionExtensions.cs), [LongKey](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.SqlServer/LongKey/ConnectionExtensions.cs), and [GuidKey](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.SqlServer/GuidKey/ConnectionExtensions.cs), so use the namespace appropriate to the identity type you chose above.

- All of the Crud methods accept an `IUser` optional argument you can use to pass the current user name and access to the user's local time. This argument takes effect if your model class is based on `Record` (see above), which offers a number of overrides for checking permissions and executing row-level events, looking up foreign keys, among other things.

## Examples

A simple find using the [Find](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/CommandProvider_Crud.cs#L489) method:

```
using (var cn = GetConnection())
{
  var e = cn.Find<Employee>(2322);
  Console.WriteLine(e.FirstName);
}
```

Find using criteria with the [FindWhere](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/CommandProvider_Crud.cs#L541) method:
```
using (var cn = GetConnection())
{
  var e = cn.FindWhere(new Employee() { OrganizationId = 12, Number = 3988 });
  Console.WriteLine(e.FirstName);
}
```

Create and save a record with the [Save](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/CommandProvider_Crud.cs#L425) method.
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

## Extending Postulate.Lite

To implement Postulate.Lite for a particular database, inherit from abstract class [CommandProvider&lt;TKey&gt;](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.Core/CommandProvider_Crud.cs) and implement its various abstract methods that generate SQL for Crud actions. The `TKey` generic argument specifies the identity (primary key) type used with your model classes. The [default MySQL implementation](https://github.com/adamosoftware/Postulate.Lite/blob/master/Postulate.Lite.MySql/MySqlProvider_Crud.cs) assumes an `int` primary key type. To implement `long` or `Guid` primary key types, you'd need to derive a new class from `CommandProvider` with your desired key type.

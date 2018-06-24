# Postulate.Lite

[![Build status](https://ci.appveyor.com/api/projects/status/ug7499knw4ut33yj/branch/master?svg=true)](https://ci.appveyor.com/project/adamosoftware/postulate-lite/branch/master)

I wanted to build an ORM that has no particular root type dependency (i.e. [Record](https://github.com/adamosoftware/Postulate.Orm/blob/master/Core/Abstract/Record.cs)) and that would work as `IDbConnection` extension methods, the way [Dapper.SimpleCRUD](https://github.com/ericdc1/Dapper.SimpleCRUD) does.

I intend to support [Model Merge](https://github.com/adamosoftware/Postulate.Orm/wiki/Model-Merge) capability eventually, but at the moment I'm just getting my new core Crud methods working.

Postulate.Lite will support both SQL Server and MySQL.

Please see the [Wiki](https://github.com/adamosoftware/Postulate.Lite/wiki) for more info.

## Status (as of 6/24/18)

- SQL Server package: **Postulate.Lite.SqlServer**
- MySql package: *coming soon*

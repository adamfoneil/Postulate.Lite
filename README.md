# Postulate.Lite

I wanted to build an ORM that has no particular root type dependency (i.e. [Record](https://github.com/adamosoftware/Postulate.Orm/blob/master/Core/Abstract/Record.cs)) and that would work as `IDbConnection` extension methods, the way [Dapper.SimpleCRUD](https://github.com/ericdc1/Dapper.SimpleCRUD) does.

I intend to support [Model Merge](https://github.com/adamosoftware/Postulate.Orm/wiki/Model-Merge) capability eventually, but at the moment I'm just getting my new core Crud methods working.

Postulate.Lite will support both SQL Server and MySQL.

## Status (as of 6/24/18)

- Nuget package: Not available yet
- SQL Server support: basic crud methods
- My SQL support: none yet

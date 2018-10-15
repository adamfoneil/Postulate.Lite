﻿using Postulate.Lite.Core;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Models;
using System;
using System.Data;

namespace Postulate.Lite.SqlServer
{
	public partial class SqlServerProvider<TKey> : CommandProvider<TKey>
	{
		protected override bool SchemaExists(IDbConnection connection, TableInfo table)
		{
			return connection.Exists("[sys].[schema] WHERE [name]=@schema", table);
		}

		protected override bool TableExists(IDbConnection connection, TableInfo table)
		{
			return connection.Exists("[sys].[tables] WHERE SCHEMA_NAME([schema_id])=@schema AND [name]=@name", table);
		}

		protected override string CreateTableScript(TableInfo tableName, Type modelType)
		{
			return CreateTableCommandInner(modelType, tableName.ToString());
		}

		protected override string SqlSelectNextVersion(string tableName)
		{
			return $"SELECT MAX([NextVersion]) FROM {tableName} WHERE [RecordId]=@id";
		}

		protected override string SqlUpdateNextVersion(string tableName)
		{
			return $"UPDATE {tableName} SET [NextVersion]=[NextVersion]+1 WHERE [RecordId]=@id";
		}
	}
}
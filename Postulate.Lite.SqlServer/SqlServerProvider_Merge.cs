using Postulate.Lite.Core;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.SqlServer
{
	public partial class SqlServerCommandProvider<TKey> : CommandProvider<TKey>
	{
		public override string CommentPrefix => "--";

		public override string CreateTableCommand(Type modelType)
		{
			var columns = MappedColumns(modelType);
			var pkColumns = GetPrimaryKeyColumns(modelType, columns, out bool identityIsPrimaryKey);
			var identityName = modelType.GetIdentityName();

			string constraintName = TableName(modelType).Replace(".", "_");

			List<string> members = new List<string>();
			members.AddRange(columns.Select(pi => SqlColumnSyntax(pi, (identityName.Equals(pi.Name)))));
			members.Add(PrimaryKeySyntax(constraintName, pkColumns));
			if (!identityIsPrimaryKey) members.Add(UniqueIdSyntax(constraintName, modelType.GetIdentityProperty()));

			return
				$"CREATE TABLE {ApplyDelimiter(TableName(modelType))} (" +
					string.Join(",\r\n\t", members) +
				")";
		}

		public override string AddForeignKeyCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddColumnCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AlterColumnCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropColumnCommand(ColumnInfo columnInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropForeignKeyCommand(ForeignKeyInfo columnInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropTableCommand(TableInfo tableInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropPrimaryKeyCommand(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string AddPrimaryKeyCommand(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string AddForeignKeyCommand(ForeignKeyInfo foreignkeyInfo)
		{
			throw new NotImplementedException();
		}
	}
}
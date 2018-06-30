using Postulate.Lite.Core;
using Postulate.Lite.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Postulate.Lite.MySql
{
	public partial class MySqlProvider<TKey> : CommandProvider<TKey>
	{
		public override string CommentPrefix => "# ";

		public override string CreateTableCommand(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string DropColumnCommand(ColumnInfo columnInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropForeignKeyCommand(ForeignKeyInfo foreignKeyInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropPrimaryKeyCommand(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string DropTableCommand(TableInfo tableInfo)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<ForeignKeyInfo> GetDependentForeignKeys(IDbConnection connection, TableInfo tableInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddColumnCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddForeignKeyCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddForeignKeyCommand(ForeignKeyInfo foreignkeyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddPrimaryKeyCommand(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string AlterColumnCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}
	}
}
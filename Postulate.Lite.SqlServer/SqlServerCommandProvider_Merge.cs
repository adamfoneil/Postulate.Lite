using Postulate.Lite.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.SqlServer
{
	public partial class SqlServerCommandProvider<TKey>
	{
		protected override string CreateTableCommand(Type modelType)
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

		protected override string CreateForeignKeyCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}
	}
}
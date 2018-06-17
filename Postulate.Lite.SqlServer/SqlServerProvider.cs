using Postulate.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Postulate.Lite.SqlServer
{
	public class SqlServerCommandProvider : CommandProvider<int>
	{
		protected override string ApplyDelimiter(string name)
		{
			return $"[{name}]";
		}

		protected override int ConvertIdentity(object value)
		{
			return Convert.ToInt32(value);
		}

		protected override string FindCommand<T>()
		{
			throw new NotImplementedException();
		}

		protected override string InsertCommand<T>()
		{
			var columns = EditableColumns<T>();
			string columnList = string.Join(", ", columns.Select(c => ApplyDelimiter(c.ColumnName)));
			string valueList = string.Join(", ", columns.Select(c => $"@{c.ColumnName}"));
			return $"INSERT INTO {TableName<T>()} ({columnList}) VALUES ({valueList});";
		}

		protected override string UpdateCommand<T>()
		{
			throw new NotImplementedException();
		}
		protected override string DeleteCommand<T>()
		{
			throw new NotImplementedException();
		}
		protected override Dictionary<Type, string> SupportedTypes(int length, int precision, int scale)
		{
			return new Dictionary<Type, string>()
			{
				{ typeof(string), $"nvarchar({length})" },
				{ typeof(int), "int" },
				{ typeof(DateTime), "datetime" },
				{ typeof(bool), "bit" },
				{ typeof(decimal), $"decimal({scale}, {precision})" },
				{ typeof(long), $"bigint" },
				{ typeof(short), "smallint" },
				{ typeof(byte), "tinyint" },
				{ typeof(TimeSpan), "time" },
				{ typeof(double), "float" },
				{ typeof(float), "float" },
				{ typeof(Guid), "uniqueidentifier" },
				{ typeof(char), "char(1)" },
				{ typeof(byte[]), $"varbinary({length})" }
			};
		}

		protected override string TableName<T>()
		{
			throw new NotImplementedException();
		}
	}
}
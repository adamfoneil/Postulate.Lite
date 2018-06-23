using Postulate.Lite.Core;
using Postulate.Lite.Core.Extensions;
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
			var columns = EditableColumns<T>(SaveAction.Insert);
			string columnList = string.Join(", ", columns.Select(c => ApplyDelimiter(c.ColumnName)));
			string valueList = string.Join(", ", columns.Select(c => $"@{c.ColumnName}"));
			return $"INSERT INTO {TableName<T>()} ({columnList}) OUTPUT [inserted].[{typeof(T).GetIdentityName()}] VALUES ({valueList});";
		}

		protected override string UpdateCommand<T>()
		{
			var columns = EditableColumns<T>(SaveAction.Update);
			return $"UPDATE {TableName<T>()} SET {string.Join(", ", columns.Select(col => $"{ApplyDelimiter(col.ColumnName)}=@{col.PropertyName}"))} WHERE {ApplyDelimiter(typeof(T).GetIdentityName())}=@id";
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
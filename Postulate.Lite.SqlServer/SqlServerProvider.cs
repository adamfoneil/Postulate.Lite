using Postulate.Lite.Core;
using Postulate.Lite.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.SqlServer
{
	public class SqlServerCommandProvider : CommandProvider<int>
	{
		protected override string ApplyDelimiter(string name)
		{
			return string.Join(".", name.Split('.').Select(part => $"[{name}]"));			
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
			string valueList = string.Join(", ", columns.Select(c => $"@{c.PropertyName}"));
			return $"INSERT INTO {ApplyDelimiter(TableName<T>())} ({columnList}) OUTPUT [inserted].[{typeof(T).GetIdentityName()}] VALUES ({valueList});";
		}

		protected override string UpdateCommand<T>()
		{
			var columns = EditableColumns<T>(SaveAction.Update);
			return $"UPDATE {ApplyDelimiter(TableName<T>())} SET {string.Join(", ", columns.Select(col => $"{ApplyDelimiter(col.ColumnName)}=@{col.PropertyName}"))} WHERE {ApplyDelimiter(typeof(T).GetIdentityName())}=@id";
		}

		protected override string DeleteCommand<T>()
		{
			return $"DELETE {ApplyDelimiter(TableName<T>())} WHERE {ApplyDelimiter(typeof(T).GetIdentityName())}=@id";
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
			var type = typeof(T);
			
			Dictionary<string, string> parts = new Dictionary<string, string>()
			{
				{ "schema", string.Empty },
				{ "name", type.Name }
			};

			var tblAttr = type.GetCustomAttribute<TableAttribute>();
			if (tblAttr != null)
			{
				if (!string.IsNullOrEmpty(tblAttr.Schema)) parts["schema"] = tblAttr.Schema;
				if (!string.IsNullOrEmpty(tblAttr.Name)) parts["name"] = tblAttr.Name;
			}

			return string.Join(".", parts.Where(kp => !string.IsNullOrEmpty(kp.Value)).Select(kp => kp.Value));
		}
	}
}
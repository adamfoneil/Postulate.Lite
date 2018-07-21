using Postulate.Lite.Core;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.SqlServer
{
	public partial class SqlServerProvider<TKey> : CommandProvider<TKey>
	{
		public SqlServerProvider(Func<object, TKey> identityConverter, string identitySyntax) : base(identityConverter, identitySyntax)
		{
		}

		protected override string ApplyDelimiter(string name)
		{
			return string.Join(".", name.Split('.').Select(part => $"[{part}]"));			
		}

		protected override string FindCommand<T>(string whereClause)
		{
			var type = typeof(T);
			var props = GetMappedColumns(type);
			var columns = props.Select(pi => new ColumnInfo(pi));
			return $"SELECT {string.Join(", ", columns.Select(col => ApplyDelimiter(col.ColumnName)))} FROM {ApplyDelimiter(TableName(typeof(T)))} WHERE {whereClause}";			
		}

		protected override string InsertCommand<T>()
		{
			var columns = EditableColumns<T>(SaveAction.Insert);
			string columnList = string.Join(", ", columns.Select(c => ApplyDelimiter(c.ColumnName)));
			string valueList = string.Join(", ", columns.Select(c => $"@{c.PropertyName}"));
			return $"INSERT INTO {ApplyDelimiter(TableName(typeof(T)))} ({columnList}) OUTPUT [inserted].[{typeof(T).GetIdentityName()}] VALUES ({valueList});";
		}

		protected override string UpdateCommand<T>()
		{
			var columns = EditableColumns<T>(SaveAction.Update);
			return $"UPDATE {ApplyDelimiter(TableName(typeof(T)))} SET {string.Join(", ", columns.Select(col => $"{ApplyDelimiter(col.ColumnName)}=@{col.PropertyName}"))} WHERE {ApplyDelimiter(typeof(T).GetIdentityName())}=@id";
		}

		protected override string DeleteCommand<T>()
		{
			return $"DELETE {ApplyDelimiter(TableName(typeof(T)))} WHERE {ApplyDelimiter(typeof(T).GetIdentityName())}=@id";
		}

		protected override Dictionary<Type, string> SupportedTypes(int length, int precision, int scale)
		{			
			return new Dictionary<Type, string>()
			{
				{ typeof(string), $"nvarchar({((length == 0) ? "max" : length.ToString())})" },
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

		protected override string TableName(Type modelType)
		{
			var tbl = GetTableInfo(modelType);
			return $"{tbl.Schema}.{tbl.Name}";
		}

		private string UniqueIdSyntax(string constraintName, PropertyInfo propertyInfo)
		{
			return $"CONSTRAINT [U_{constraintName}] UNIQUE ({string.Join(", ", ApplyDelimiter(propertyInfo.GetColumnName()))})";
		}

		private string PrimaryKeySyntax(string constraintName, IEnumerable<PropertyInfo> pkColumns)
		{
			return $"CONSTRAINT [PK_{constraintName}] PRIMARY KEY ({string.Join(", ", pkColumns.Select(pi => ApplyDelimiter(pi.GetColumnName())))})";
		}

		protected override string SqlColumnSyntax(PropertyInfo propertyInfo, bool isIdentity)
		{
			ColumnInfo col = new ColumnInfo(propertyInfo);
			string result = ApplyDelimiter(col.ColumnName);

			var calcAttr = propertyInfo.GetCustomAttribute<CalculatedAttribute>();
			if (calcAttr != null)
			{
				result += $" AS {calcAttr.Expression}";
			}
			else
			{				
				string nullSyntax = (col.AllowNull) ? "NULL" : "NOT NULL";

				string dataType = (col.HasExplicitType()) ? 
					col.DataType : 
					SupportedTypes(col.Length, col.Precision, col.Scale)[propertyInfo.PropertyType];				

				if (isIdentity) dataType += " " + IdentityColumnSyntax();

				result += $" {dataType} {nullSyntax}";
			}

			return result;
		}
	}
}
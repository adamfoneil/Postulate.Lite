using Postulate.Lite.Core;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Metadata;
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
			return string.Join(".", name.Split('.').Select(part => $"[{name}]"));			
		}

		protected override string FindCommand<T>(string whereClause)
		{
			var type = typeof(T);
			var props = MappedColumns(type);
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

		protected override string TableName(Type modelType)
		{			
			Dictionary<string, string> parts = new Dictionary<string, string>()
			{
				{ "schema", string.Empty },
				{ "name", modelType.Name }
			};

			var tblAttr = modelType.GetCustomAttribute<TableAttribute>();
			if (tblAttr != null)
			{
				if (!string.IsNullOrEmpty(tblAttr.Schema)) parts["schema"] = tblAttr.Schema;
				if (!string.IsNullOrEmpty(tblAttr.Name)) parts["name"] = tblAttr.Name;
			}

			return string.Join(".", parts.Where(kp => !string.IsNullOrEmpty(kp.Value)).Select(kp => kp.Value));
		}

		private string UniqueIdSyntax(string constraintName, PropertyInfo propertyInfo)
		{
			return $"CONSTRAINT [U_{constraintName}] UNIQUE ({string.Join(", ", ApplyDelimiter(propertyInfo.GetColumnName()))})";
		}

		private string PrimaryKeySyntax(string constraintName, IEnumerable<PropertyInfo> pkColumns)
		{
			return $"CONSTRAINT [PK_{constraintName}] PRIMARY KEY ({string.Join(", ", pkColumns.Select(pi => ApplyDelimiter(pi.GetColumnName())))})";
		}

		private IEnumerable<PropertyInfo> GetPrimaryKeyColumns(Type type, IEnumerable<PropertyInfo> columns, out bool identityIsPrimaryKey)
		{
			identityIsPrimaryKey = false;
			var result = columns.Where(pi => HasAttribute<PrimaryKeyAttribute>(pi));

			if (!result.Any())
			{
				identityIsPrimaryKey = true;
				result = new[] { type.GetIdentityProperty() };
			}

			return result;
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
using Postulate.Lite.Core;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.MySql
{
	public partial class MySqlProvider<TKey> : CommandProvider<TKey>
	{
		public MySqlProvider(Func<object, TKey> identityConverter) : base(identityConverter, new MySqlIntegrator(), "auto_increment")
		{
		}

		protected override string ApplyDelimiter(string name)
		{
			return $"`{name}`";
		}

		public override TableInfo GetTableInfo(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override bool IsTableEmpty(IDbConnection connection, Type modelType)
		{
			throw new NotImplementedException();
		}

		protected override string FindCommand<T>(string whereClause)
		{
			var type = typeof(T);
			var props = _integrator.GetMappedColumns(type);
			var columns = props.Select(pi => new ColumnInfo(pi));
			return $"SELECT {string.Join(", ", columns.Select(col => ApplyDelimiter(col.ColumnName)))} FROM {ApplyDelimiter(TableName(typeof(T)))} WHERE {whereClause}";
		}

		protected override string InsertCommand<T>()
		{
			var columns = _integrator.GetEditableColumns(typeof(T), SaveAction.Insert);
			string columnList = string.Join(", ", columns.Select(c => ApplyDelimiter(c.ColumnName)));
			string valueList = string.Join(", ", columns.Select(c => $"@{c.PropertyName}"));
			return $"INSERT INTO {ApplyDelimiter(TableName(typeof(T)))} ({columnList}) VALUES ({valueList}); SELECT LAST_INSERT_ID()";
		}

		protected override string UpdateCommand<T>()
		{
			var columns = _integrator.GetEditableColumns(typeof(T), SaveAction.Update);
			return $"UPDATE {ApplyDelimiter(TableName(typeof(T)))} SET {string.Join(", ", columns.Select(col => $"{ApplyDelimiter(col.ColumnName)}=@{col.PropertyName}"))} WHERE {ApplyDelimiter(typeof(T).GetIdentityName())}=@id";
		}

		protected override string DeleteCommand<T>()
		{
			return $"DELETE FROM {ApplyDelimiter(TableName(typeof(T)))} WHERE {ApplyDelimiter(typeof(T).GetIdentityName())}=@id";
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

				var typeMap = _integrator.SupportedTypes(col.Length, col.Precision, col.Scale);
				Type t = propertyInfo.GetMappedType();
				if (!typeMap.ContainsKey(t)) throw new KeyNotFoundException($"Type name {t.Name} not supported.");

				string dataType = (col.HasExplicitType()) ?
					col.DataType :
					typeMap[t].FormattedName;

				if (isIdentity) dataType += " " + IdentityColumnSyntax();

				result += $" {dataType} {nullSyntax}";
			}

			return result;
		}

		public override string TableName(Type modelType)
		{
			string result = modelType.Name;

			var tblAttr = modelType.GetCustomAttribute<TableAttribute>();
			if (tblAttr != null && !string.IsNullOrEmpty(tblAttr.Name))
			{
				result = tblAttr.Name;
			}

			return result;
		}

		public override string AddColumnCommand(ColumnInfo columnInfo)
		{
			throw new NotImplementedException();
		}
	}
}
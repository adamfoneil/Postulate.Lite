using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Postulate.Lite.Core.Models
{
	public class ColumnInfo
	{
		public ColumnInfo(PropertyInfo propertyInfo)
		{
			PropertyInfo = PropertyInfo;
			PropertyName = propertyInfo.Name;
			ColumnName = propertyInfo.Name;
			AllowNull = propertyInfo.PropertyType.IsNullable();

			var colAccess = propertyInfo.GetCustomAttribute<ColumnAccessAttribute>();
			if (colAccess != null)
			{
				SaveActions = colAccess.Action;
			}
			else
			{
				SaveActions = SaveAction.Insert | SaveAction.Update;
			}

			var colAttr = propertyInfo.GetCustomAttribute<ColumnAttribute>();
			if (colAttr != null)
			{
				if (!string.IsNullOrEmpty(colAttr.Name)) ColumnName = colAttr.Name;
				if (!string.IsNullOrEmpty(colAttr.TypeName)) DataType = colAttr.TypeName;
			}

			var lengthAttr = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
			if (lengthAttr != null) Length = lengthAttr.Length;

			var requiredAttr = propertyInfo.GetCustomAttribute<RequiredAttribute>();
			if (requiredAttr != null) AllowNull = false;

			var precisionAttr = propertyInfo.GetCustomAttribute<DecimalPrecisionAttribute>();
			if (precisionAttr != null)
			{
				Precision = precisionAttr.Precision;
				Scale = precisionAttr.Scale;
			}

			var pkAttr = propertyInfo.GetCustomAttribute<PrimaryKeyAttribute>();
			if (pkAttr != null) AllowNull = false;
		}

		public ColumnInfo()
		{
		}

		public string Schema { get; set; }
		public string TableName { get; set; }
		public string PropertyName { get; set; }
		public string ColumnName { get; set; }
		public string DataType { get; set; }
		public int Length { get; set; }
		public int Precision { get; set; }
		public int Scale { get; set; }
		public bool AllowNull { get; set; }
		public SaveAction SaveActions { get; set; }
		public bool IsCalculated { get; set; }
		public string Collation { get; set; }
		public bool IsNullable { get; set; }
		public string ReferencedSchema { get; set; }
		public string ReferencedTable { get; set; }
		public string ReferencedColumn { get; set; }
		public string ForeignKeyConstraint { get; set; }
		public string Expression { get; set; }
		public int ByteLength { get; set; }

		public Type GetModelType()
		{
			return PropertyInfo.DeclaringType;
		}

		public TableInfo GetTableInfo()
		{
			return new TableInfo() { Schema = Schema, Name = TableName };
		}

		public PropertyInfo PropertyInfo { get; private set; }

		public bool HasExplicitType()
		{
			return !string.IsNullOrEmpty(DataType);
		}

		public override bool Equals(object obj)
		{
			ColumnInfo test = obj as ColumnInfo;
			if (test != null)
			{
				return 
					test.Schema.ToLower().Equals(Schema.ToLower()) && 
					test.TableName.ToLower().Equals(TableName.ToLower()) && 
					test.ColumnName.ToLower().Equals(ColumnName.ToLower());
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Schema.GetHashCode() + TableName.GetHashCode() + ColumnName.GetHashCode();
		}

		public bool IsAlteredFrom(ColumnInfo sc)
		{
			throw new NotImplementedException();
		}
	}
}
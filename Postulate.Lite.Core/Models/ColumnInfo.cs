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
			PropertyInfo = propertyInfo;
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

		public ColumnInfo(string schema, string tableName, string columnName)
		{
			TableInfo.Schema = schema;
			TableInfo.Name = tableName;
			ColumnName = columnName;
		}

		public TableInfo TableInfo { get; set; } = new TableInfo();
		public string Schema { get { return TableInfo.Schema; } set { TableInfo.Schema = value; } }
		public string TableName { get { return TableInfo.Name; } set { TableInfo.Name = value; } }
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
		public ForeignKeyInfo ForeignKeyInfo { get; set; }
		public string ReferencedSchema { get { return ForeignKeyInfo.Parent.Schema; } set { ForeignKeyInfo.Parent.Schema = value; } }
		public string ReferencedTable { get { return ForeignKeyInfo.Parent.TableName; } set { ForeignKeyInfo.Parent.TableName = value; } }
		public string ReferencedColumn { get { return ForeignKeyInfo.Parent.ColumnName; } set { ForeignKeyInfo.Parent.ColumnName = value; } }
		public string ForeignKeyConstraint { get { return ForeignKeyInfo.ConstraintName; } set { ForeignKeyInfo.ConstraintName = value; } }
		public bool CascadeDelete { get { return ForeignKeyInfo.CascadeDelete; } set { ForeignKeyInfo.CascadeDelete = value; } }
		public bool IsForeignKey { get { return ForeignKeyInfo != null; } }
		public string Expression { get; set; }
		public int ByteLength { get; set; }

		public Type GetModelType()
		{
			return PropertyInfo.DeclaringType;
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

		public bool IsAlteredFrom(ColumnInfo columnInfo)
		{
			if (this.Equals(columnInfo))
			{
				if (columnInfo.IsCalculated && IsCalculated)
				{
					if ($"({Expression})".Equals(columnInfo.Expression)) return false;
					return !columnInfo.Expression.Equals(Expression);
				}

				// alter the columnInfo.DataType to reflect the size so it's comparable to how data type is reported by PropertyInfo
				if (columnInfo.DataType.Contains("var") && !columnInfo.DataType.Contains("("))
				{
					int divisor = (columnInfo.DataType.Contains("nvar")) ? 2 : 1;
					columnInfo.DataType += $"({columnInfo.ByteLength / divisor})";
				}

				// apply the scale and precision to the data type just like with length
				if (columnInfo.DataType.Contains("decimal") && !columnInfo.DataType.Contains("("))
				{
					columnInfo.DataType += $"({columnInfo.Precision}, {columnInfo.Scale})";
				}

				if (columnInfo.Length.Equals("max") && columnInfo.DataType.Equals("nvarchar(0)"))
				{
					columnInfo.DataType = "nvarchar(max)";
				}

				if (columnInfo.Length.Equals("max") && columnInfo.DataType.Equals("varbinary(-1)"))
				{
					columnInfo.DataType = "varbinary(max)";
				}

				// then any other property diff is considered an alter
				if (!DataType?.Replace(" ", string.Empty).Equals(columnInfo.DataType.Replace(" ", string.Empty)) ?? true) return true;
				if (IsNullable != columnInfo.IsNullable) return true;
				if (IsCalculated != columnInfo.IsCalculated) return true;

				DecimalPrecisionAttribute scaleAttr = null;
				if (PropertyInfo?.HasAttribute<DecimalPrecisionAttribute>(out scaleAttr) ?? false)
				{
					if (Precision != columnInfo.Precision) return true;
					if (Scale != columnInfo.Scale) return true;
				}

				// note -- don't compare the ByteLength property because it's not reported by PropertyInfo
			}
			return false;
		}
	}
}
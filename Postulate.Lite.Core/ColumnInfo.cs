using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Postulate.Lite.Core
{
	public class ColumnInfo
	{
		public ColumnInfo(PropertyInfo propertyInfo)
		{
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
			if (pkAttr != null)
			{
				AllowNull = false;
			}
		}

		public ColumnInfo()
		{
		}

		public string PropertyName { get; set; }
		public string ColumnName { get; set; }
		public string DataType { get; set; }
		public int Length { get; set; }
		public int Precision { get; set; }
		public int Scale { get; set; }
		public bool AllowNull { get; set; }
		public SaveAction SaveActions { get; set; }

		public bool HasExplicitType()
		{
			return !string.IsNullOrEmpty(DataType);
		}
	}
}
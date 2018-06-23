using Postulate.Lite.Core.Attributes;
using System;
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
		}

		public ColumnInfo()
		{
		}

		public string PropertyName { get; set; }
		public string ColumnName { get; set; }
		public string DataType { get; set; }		
		public SaveAction SaveActions { get; set; }
	}
}
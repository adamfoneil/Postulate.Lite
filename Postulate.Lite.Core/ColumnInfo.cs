using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.Core
{
	public class ColumnInfo
	{
		public ColumnInfo(PropertyInfo propertyInfo)
		{
			PropertyName = propertyInfo.Name;
			ColumnName = propertyInfo.Name;

			try
			{
				var attr = propertyInfo.GetCustomAttributes<ColumnAttribute>(false).OfType<ColumnAttribute>().First();
				ColumnName = attr.Name;
				DataType = attr.TypeName;
			}
			catch
			{
				// do nothing
			}
		}

		public ColumnInfo()
		{
		}

		public string PropertyName { get; set; }
		public string ColumnName { get; set; }
		public string DataType { get; set; }
	}
}
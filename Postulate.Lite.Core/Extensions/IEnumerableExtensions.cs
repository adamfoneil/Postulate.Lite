using Postulate.Lite.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.Core.Extensions
{
	/* thanks to
		http://www.chinhdo.com/20090402/convert-list-to-datatable/
		https://stackoverflow.com/questions/1253725/convert-ienumerable-to-datatable
		https://github.com/tdietrich513/DataTableProxy
	*/

	public static class IEnumerableExtensions
	{
		public static DataTable ToDataTable<T>(this IEnumerable<T> enumerable, SqlIntegrator integrator, bool excludeIdentity = false) where T : class
		{
			DataTable result = DataTableFromType(typeof(T), integrator, excludeIdentity, properties: out PropertyInfo[] properties);
			foreach (T item in enumerable) result.Rows.Add(ValuesFromItem(properties, result, item));
			return result;
		}

		private static object[] ValuesFromItem(PropertyInfo[] properties, DataTable table, object item)
		{
			object[] values = new object[properties.Length];
			for (int i = 0; i < properties.Length; i++)
			{
				values[i] = properties[i].GetValue(item);
			}
			return values;
		}

		private static DataTable DataTableFromType(Type type, SqlIntegrator integrator, bool excludeIdentity, out PropertyInfo[] properties)
		{						
			DataTable result = new DataTable(integrator.GetTableName(type));
			properties = integrator.GetMappedColumns(type).ToArray();
			if (excludeIdentity)
			{
				var identityProp = type.GetIdentityProperty();
				if (identityProp != null) properties = properties.Except(new PropertyInfo[] { identityProp }).ToArray();
			}

			List<DataColumn> pkColumns = new List<DataColumn>();
			foreach (PropertyInfo pi in properties)
			{
				DataColumn col = result.Columns.Add(pi.GetColumnName(), pi.GetMappedType());
				if (pi.HasAttribute<PrimaryKeyAttribute>()) pkColumns.Add(col);
			}
		
			result.PrimaryKey = pkColumns.ToArray();

			return result;
		}
	}
}
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.Core
{
	/// <summary>
	/// Low-level class and property to table and column mapping services
	/// </summary>
	public abstract class SqlIntegrator
	{
		/// <summary>
		/// Specifies the types and corresponding SQL syntax for CLR types supported in your ORM mapping
		/// </summary>
		public abstract Dictionary<Type, SqlTypeInfo> SupportedTypes(int length = 0, int precision = 0, int scale = 0);		

		public SqlTypeInfo FindTypeInfo(Type type)
		{
			var supportedTypes = SupportedTypes();

			if (supportedTypes.ContainsKey(type)) return SupportedTypes()[type];

			if (type.IsEnum && type.GetEnumUnderlyingType().Equals(typeof(int)) && supportedTypes.ContainsKey(typeof(int))) return supportedTypes[typeof(int)];

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				var genericType = type.GetGenericArguments()[0];
				if (supportedTypes.ContainsKey(genericType)) return supportedTypes[genericType];
			}
					
			return null;
		}

		/// <summary>
		/// Returns the properties of a model class that are mapped to database columns
		/// </summary>
		public IEnumerable<PropertyInfo> GetMappedColumns(Type modelType)
		{
			return modelType.GetProperties().Where(pi => IsMapped(pi) && IsSupportedType(pi.PropertyType));
		}

		/// <summary>
		/// Returns the properties of a model class that may be affected by an INSERT or UPDATE statement.
		/// For example calculated and identity columns are omitted.
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		/// <param name="action">Indicates whether an insert or update is in effect</param>
		public IEnumerable<ColumnInfo> GetEditableColumns(Type modelType, SaveAction action)
		{
			string identity = modelType.GetIdentityName().ToLower();
			var props = modelType.GetProperties().Where(pi => !pi.GetColumnName().ToLower().Equals(identity)).ToArray();
			return props.Where(pi => IsEditable(pi, action)).Select(pi => new ColumnInfo(pi)).ToArray();
		}

		/// <summary>
		/// Returns true if the property is mapped to a database table column
		/// </summary>
		private bool IsMapped(PropertyInfo propertyInfo)
		{
			return propertyInfo.CanWrite && !propertyInfo.HasAttribute<NotMappedAttribute>();
		}

		private bool IsEditable(PropertyInfo propertyInfo, SaveAction action)
		{
			if (!propertyInfo.CanWrite) return false;
			if (!IsSupportedType(propertyInfo.PropertyType)) return false;
			if (!IsMapped(propertyInfo)) return false;
			if (IsCalculated(propertyInfo)) return false;

			var colInfo = new ColumnInfo(propertyInfo);
			return ((colInfo.SaveActions & action) == action);
		}

		/// <summary>
		/// Determines whether a given Type is reflected in a database table
		/// </summary>
		public bool IsSupportedType(Type type)
		{
			return
				SupportedTypes().ContainsKey(type) ||
				(type.IsEnum && type.GetEnumUnderlyingType().Equals(typeof(int))) ||
				(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSupportedType(type.GetGenericArguments()[0]));
		}

		/// <summary>
		/// Returns true if the property has a [Calculated] attribute
		/// </summary>
		private bool IsCalculated(PropertyInfo propertyInfo)
		{
			return propertyInfo.HasAttribute<CalculatedAttribute>();
		}

		public class SqlTypeInfo
		{
			public SqlTypeInfo(string baseName)
			{
				BaseName = baseName;
				FormattedName = baseName;
			}

			public SqlTypeInfo(string baseName, string formattedName)
			{
				BaseName = baseName;
				FormattedName = formattedName;
			}

			public string BaseName { get; private set; }
			public string FormattedName { get; private set; }
		}
	}
}
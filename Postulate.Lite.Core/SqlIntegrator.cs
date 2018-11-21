using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Enums;
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

		public abstract string DefaultSchema { get; }

		public abstract SupportedPlatform SupportedPlatform { get; }

		public TableInfo GetTableInfo(Type type)
		{
			Dictionary<string, string> parts = new Dictionary<string, string>()
			{
				{ "schema", DefaultSchema },
				{ "name", type.Name }
			};

			var tblAttr = type.GetCustomAttribute<TableAttribute>();
			if (tblAttr != null)
			{
				if (!string.IsNullOrEmpty(tblAttr.Schema)) parts["schema"] = tblAttr.Schema;
				if (!string.IsNullOrEmpty(tblAttr.Name)) parts["name"] = tblAttr.Name;
			}

			var schemaAttr = type.GetCustomAttribute<SchemaAttribute>();
			if (schemaAttr != null) parts["schema"] = schemaAttr.Name;

			return new TableInfo() { Name = parts["name"], Schema = parts["schema"], ModelType = type };

		}

		public string GetTableName(Type type)
		{
			var tbl = GetTableInfo(type);
			return (!string.IsNullOrEmpty(tbl.Schema)) ? $"{tbl.Schema}.{tbl.Name}" : tbl.Name;
		}

		public SqlTypeInfo FindTypeInfo(Type type)
		{
			var supportedTypes = SupportedTypes();

			if (supportedTypes.ContainsKey(type)) return supportedTypes[type];

			if (type.IsEnum && type.GetEnumUnderlyingType().Equals(typeof(int)) && supportedTypes.ContainsKey(typeof(int))) return supportedTypes[typeof(int)];

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				var innerType = type.GetGenericArguments()[0];
				if (innerType.IsEnum) return FindTypeInfo(innerType);
				if (supportedTypes.ContainsKey(innerType)) return supportedTypes[innerType];
			}
					
			return null;
		}

		/// <summary>
		/// Returns the properties of a model class that are mapped to database columns
		/// </summary>
		public IEnumerable<PropertyInfo> GetMappedColumns(Type modelType)
		{
			var properties = modelType.GetProperties().Where(pi => IsMapped(pi) && IsSupportedType(pi.PropertyType));
			var identityAttr = modelType.GetAttribute<IdentityAttribute>();

			if ((identityAttr?.Position ?? IdentityPosition.NotSet) != IdentityPosition.NotSet)
			{
				var identityProp = modelType.GetIdentityProperty();
				var otherProps = properties.Where(pi => !pi.Equals(identityProp));

				if (identityAttr.Position == IdentityPosition.FirstColumn) yield return identityProp;

				foreach (var prop in otherProps) yield return prop;

				if (identityAttr.Position == IdentityPosition.LastColumn) yield return identityProp;
			}
			else
			{
				foreach (var prop in properties) yield return prop;
			}
		}

		/// <summary>
		/// Returns the properties of a model class that may be affected by an INSERT or UPDATE statement.
		/// For example calculated and identity columns are omitted.
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		/// <param name="action">Indicates whether an insert or update is in effect</param>
		public IEnumerable<ColumnInfo> GetEditableColumns(Type modelType, SaveAction action)
		{
			bool hasIdentity = false;
			string identity = modelType.TryGetIdentityName(string.Empty, ref hasIdentity).ToLower();
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
using Dapper;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.Core
{
	public abstract class CommandProvider<TKey>
	{
		protected abstract string InsertCommand<T>();

		protected abstract string UpdateCommand<T>();

		protected abstract string DeleteCommand<T>();

		protected abstract string FindCommand<T>();

		protected abstract string CreateTableCommand<T>();

		protected abstract TKey ConvertIdentity(object value);

		protected abstract string ApplyDelimiter(string name);

		protected abstract string TableName<T>();

		protected abstract string SqlColumnSyntax(PropertyInfo propertyInfo, bool isIdentity);

		protected abstract string IdentityColumnSyntax();

		protected IEnumerable<ColumnInfo> EditableColumns<T>(SaveAction action)
		{
			string identity = typeof(T).GetIdentityName().ToLower();
			var props = typeof(T).GetProperties().Where(pi => !pi.GetColumnName().ToLower().Equals(identity)).ToArray();
			return props.Where(pi => IsEditable(pi, action)).Select(pi => new ColumnInfo(pi)).ToArray();
		}

		protected IEnumerable<PropertyInfo> MappedColumns<T>()
		{
			var type = typeof(T);
			return type.GetProperties().Where(pi => IsMapped(pi) && IsSupportedType(pi.PropertyType));
		}

		private bool IsEditable(PropertyInfo pi, SaveAction action)
		{
			if (!IsSupportedType(pi.PropertyType)) return false;
			if (!IsMapped(pi)) return false;
			if (IsCalculated(pi)) return false;
			
			var colInfo = new ColumnInfo(pi);
			return ((colInfo.SaveActions & action) == action);
		}

		protected bool IsSupportedType(Type type)
		{			
			return
				SupportedTypes().ContainsKey(type) ||
				(type.IsEnum && type.GetEnumUnderlyingType().Equals(typeof(int))) ||
				(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSupportedType(type.GetGenericArguments()[0]));
		}

		public bool IsNew<TModel>(TModel @object)
		{
			return GetIdentity(@object).Equals(default(TKey));
		}

		protected void SetIdentity<TModel>(TModel @object, TKey value)
		{
			if (IsNew(@object))
			{
				var identityProp = typeof(TModel).GetIdentityProperty();
				identityProp.SetValue(@object, value);
			}
			else
			{
				throw new InvalidOperationException("Can't set a record's identity more than once.");
			}
		}

		public TKey GetIdentity<TModel>(TModel @object)
		{
			var property = typeof(TModel).GetIdentityProperty();
			return ConvertIdentity(property.GetValue(@object));
		}

		protected abstract Dictionary<Type, string> SupportedTypes(int length = 0, int precision = 0, int scale = 0);

		public TKey Insert<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			var record = @object as Record;
			record?.Validate(connection);
			if (user != null)
			{
				record?.CheckSavePermission(connection, user);
				record?.BeforeSave(connection, user);
			}

			string cmd = InsertCommand<TModel>();
			TKey result = connection.QuerySingleOrDefault<TKey>(cmd, @object);
			SetIdentity(@object, result);

			record?.AfterSave(connection, SaveAction.Insert);

			return result;
		}

		public void Update<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			var record = @object as Record;
			record?.Validate(connection);
			if (user != null)
			{
				record?.CheckSavePermission(connection, user);
				record?.BeforeSave(connection, user);
			}

			string cmd = UpdateCommand<TModel>();
			connection.Execute(cmd, @object);

			record?.AfterSave(connection, SaveAction.Update);
		}

		public TKey Save<TModel>(IDbConnection connection, TModel @object, IUser user = null)
		{
			if (IsNew(@object))
			{
				return Insert(connection, @object, user);
			}
			{
				Update(connection, @object, user);
				return GetIdentity(@object);
			}
		}

		public TModel Find<TModel>(IDbConnection connection, TKey identity, IUser user = null)
		{
			string cmd = FindCommand<TModel>();
			TModel result = connection.QuerySingleOrDefault<TModel>(cmd, new { id = identity });

			var record = result as Record;
			if (user != null)
			{
				record?.CheckFindPermission(connection, user);
			}

			return result;
		}

		public void Delete<TModel>(IDbConnection connection, TKey identity, IUser user = null)
		{
			var deleteMe = Find<TModel>(connection, identity, user);
			var record = deleteMe as Record;
			if (user != null) record?.CheckDeletePermission(connection, user);

			string cmd = DeleteCommand<TModel>();
			connection.Execute(cmd, new { id = identity });

			record?.AfterDelete(connection);
		}

		public void CreateTable<TModel>(IDbConnection connection)
		{
			string cmd = CreateTableCommand<TModel>();
			connection.Execute(cmd);
		}

		protected bool IsMapped(PropertyInfo propertyInfo)
		{
			return !HasAttribute<NotMappedAttribute>(propertyInfo);
		}

		protected bool IsCalculated(PropertyInfo propertyInfo)
		{
			return HasAttribute<CalculatedAttribute>(propertyInfo);
		}

		protected bool HasAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
		{
			var attr = propertyInfo.GetCustomAttribute<T>();
			return (attr != null);
		}
	}
}
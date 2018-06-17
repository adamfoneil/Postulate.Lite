using Dapper;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Interfaces;
using System;
using System.Collections.Generic;
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

		protected abstract TKey ConvertIdentity(object value);

		protected abstract string ApplyDelimiter(string name);

		protected abstract string TableName<T>();

		protected IEnumerable<ColumnInfo> EditableColumns<TModel>()
		{
			var props = typeof(TModel).GetProperties();
			return props.Where(pi => IsEditable(pi)).Select(pi => new ColumnInfo(pi));
		}

		private bool IsEditable(PropertyInfo pi)
		{
			throw new NotImplementedException();
		}

		public bool IsNew<TModel>(TModel @object)
		{
			return GetIdentity(@object).Equals(default(TKey));
		}

		protected void SetIdentity<TModel>(TModel @object, TKey value)
		{
			if (IsNew(@object))
			{
				var identityProp = GetIdentityProperty(typeof(TModel));
				identityProp.SetValue(value, @object);
			}
			else
			{
				throw new InvalidOperationException("Can't set a record's identity more than once.");
			}
		}

		public TKey GetIdentity<TModel>(TModel @object)
		{
			Type t = typeof(TModel);

			try
			{
				var property = GetIdentityProperty(t);
				return ConvertIdentity(property.GetValue(@object));
			}
			catch (Exception exc)
			{
				throw new Exception($"Couldn't determine record identity {t.Name}: {exc.Message}");
			}
		}

		private static PropertyInfo GetIdentityProperty(Type t)
		{
			var identity = t.GetCustomAttributes(typeof(IdentityAttribute), true).OfType<IdentityAttribute>().First();
			var property = t.GetProperty(identity.PropertyName);
			if (property != null) return property;

			property = t.GetProperty("Id");
			if (property != null && property.PropertyType.Equals(typeof(TKey)) && property.CanWrite) return property;

			throw new InvalidOperationException($"Couldn't find an identity property on class {t.Name}");						
		}

		protected abstract Dictionary<Type, string> SupportedTypes(int length, int precision, int scale);

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
		}
	}
}
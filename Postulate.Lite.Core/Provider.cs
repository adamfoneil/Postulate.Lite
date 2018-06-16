using Dapper;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Interfaces;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.Core
{
	public abstract class Provider<TKey>
	{
		public Provider(IUser user)
		{
			User = user;
		}

		protected abstract string InsertCommand<T>();

		protected abstract string UpdateCommand<T>();

		protected abstract string DeleteCommand<T>();

		protected abstract string FindCommand<T>();

		protected abstract TKey ConvertIdentity(object value);

		protected IUser User { get; }		

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
			return property;
		}

		public TKey Insert<TModel>(IDbConnection connection, TModel @object)
		{
			var record = @object as Record;
			record?.Validate(connection);
			record?.CheckSavePermission(connection, User);
			record?.BeforeSave(connection, User);

			string cmd = InsertCommand<TModel>();
			TKey result = connection.QuerySingleOrDefault<TKey>(cmd, @object);
			SetIdentity(@object, result);

			record?.AfterSave(connection, SaveAction.Insert);

			return result;
		}

		public void Update<TModel>(IDbConnection connection, TModel @object)
		{
			var record = @object as Record;
			record?.Validate(connection);
			record?.CheckSavePermission(connection, User);
			record?.BeforeSave(connection, User);

			string cmd = UpdateCommand<TModel>();
			connection.Execute(cmd, @object);

			record?.AfterSave(connection, SaveAction.Update);
		}

		public TKey Save<TModel>(IDbConnection connection, TModel @object)
		{
			if (IsNew(@object))
			{
				return Insert(connection, @object);
			}
			{
				Update(connection, @object);
				return GetIdentity(@object);
			}
		}

		public TModel Find<TModel>(IDbConnection connection, TKey identity)
		{
			string cmd = FindCommand<TModel>();
			TModel result = connection.QuerySingleOrDefault<TModel>(cmd, new { id = identity });

			var record = result as Record;
			record?.CheckFindPermission(connection, User);


			return result;
		}
	}
}
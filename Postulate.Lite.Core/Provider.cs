using Postulate.Core.Attributes;
using System;
using System.Data;
using System.Linq;

namespace Postulate.Core
{	
	public abstract class Provider<TKey>
	{
		protected abstract string InsertCommand<T>();

		protected abstract string UpdateCommand<T>();

		protected abstract string DeleteCommand<T>();

		protected abstract string FindCommand<T>();

		protected abstract TKey ConvertIdentity(object value);		

		public bool IsNew<TModel>(TModel @object)
		{
			return Identity(@object).Equals(default(TKey));
		}

		public TKey Identity<TModel>(TModel @object)
		{
			Type t = typeof(TModel);

			try
			{				
				var identity = t.GetCustomAttributes(typeof(IdentityAttribute), true).OfType<IdentityAttribute>().First();
				var property = t.GetProperty(identity.ColumnName);
				return ConvertIdentity(property.GetValue(@object));
			}
			catch (Exception exc)
			{
				throw new Exception($"Couldn't determine record identity {t.Name}: {exc.Message}");
			}
		}

		public TKey Insert<TModel>(IDbConnection connection, TModel @object)
		{
			throw new NotImplementedException();
		}

		public void Update<TModel>(IDbConnection connection, TModel @object)
		{
			throw new NotImplementedException();
		}

		public TKey Save<TModel>(IDbConnection connection, TModel @object)
		{
			if (IsNew<TModel>(@object))
			{
				return Insert(connection, @object);
			}
			{
				Update(connection, @object);
				return Identity(@object);
			}
		}

		public TModel Find<TModel>(IDbConnection connection, TKey identity)
		{
			throw new NotImplementedException();
		}
	}
}
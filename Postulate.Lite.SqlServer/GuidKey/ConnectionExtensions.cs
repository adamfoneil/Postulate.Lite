using Postulate.Lite.Core.Interfaces;
using System;
using System.Data;

namespace Postulate.Lite.SqlServer.GuidKey
{
	public static class ConnectionExtensions
	{
		private static SqlServerCommandProvider<Guid> GetProvider()
		{
			return new SqlServerCommandProvider<Guid>((obj) => Guid.Parse(obj.ToString()), "DEFAULT newid()");
		}

		public static TModel Find<TModel>(this IDbConnection connection, Guid id, IUser user = null)
		{
			return GetProvider().Find<TModel>(connection, id, user);
		}

		public static TModel FindWhere<TModel>(this IDbConnection connection, TModel criteria, IUser user = null)
		{
			return GetProvider().FindWhere(connection, criteria, user);
		}

		public static Guid Save<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Save(connection, @object, user);
		}

		public static Guid Insert<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Insert(connection, @object, user);
		}

		public static void Update<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			GetProvider().Update(connection, @object, user);
		}

		public static void Delete<TModel>(this IDbConnection connection, Guid id, IUser user = null)
		{
			GetProvider().Delete<TModel>(connection, id, user);
		}

		public static void CreateTable<TModel>(this IDbConnection connection)
		{
			GetProvider().CreateTable<TModel>(connection);
		}
	}
}
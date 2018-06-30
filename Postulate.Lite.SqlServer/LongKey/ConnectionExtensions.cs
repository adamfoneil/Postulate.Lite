using Postulate.Lite.Core.Interfaces;
using System;
using System.Data;

namespace Postulate.Lite.SqlServer.LongKey
{
	public static class ConnectionExtensions
	{
		private static SqlServerProvider<long> GetProvider()
		{
			return new SqlServerProvider<long>((obj) => Convert.ToInt64(obj), "identity(1,1)");
		}

		public static bool Exists<TModel>(this IDbConnection connection, long id, IUser user = null)
		{
			return GetProvider().Exists<TModel>(connection, id, user);
		}

		public static bool ExistsWhere<TModel>(this IDbConnection connection, TModel criteria, IUser user = null)
		{
			return GetProvider().ExistsWhere<TModel>(connection, criteria, user);
		}

		public static TModel Find<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			return GetProvider().Find<TModel>(connection, id, user);
		}

		public static TModel FindWhere<TModel>(this IDbConnection connection, TModel criteria, IUser user = null)
		{
			return GetProvider().FindWhere(connection, criteria, user);
		}

		public static long Save<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Save(connection, @object, user);
		}

		public static long Insert<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Insert(connection, @object, user);
		}

		public static void Update<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			GetProvider().Update(connection, @object, user);
		}

		public static void Delete<TModel>(this IDbConnection connection, long id, IUser user = null)
		{
			GetProvider().Delete<TModel>(connection, id, user);
		}

		public static void CreateTable<TModel>(this IDbConnection connection)
		{
			GetProvider().CreateTable<TModel>(connection);
		}
	}
}
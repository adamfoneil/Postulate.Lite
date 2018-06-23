using Postulate.Lite.Core.Interfaces;
using System.Data;

namespace Postulate.Lite.SqlServer
{
	public static class ConnectionExtensions
	{
		public static TModel Find<TModel>(this IDbConnection connection, int id, IUser user = null)
		{			
			return new SqlServerCommandProvider().Find<TModel>(connection, id, user);
		}

		public static int Save<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{			
			return new SqlServerCommandProvider().Save(connection, @object, user);
		}

		public static int Insert<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{			
			return new SqlServerCommandProvider().Insert(connection, @object, user);
		}

		public static void Update<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			new SqlServerCommandProvider().Update(connection, @object, user);
		}

		public static void Delete<TModel>(this IDbConnection connection, int id, IUser user = null)
		{			
			new SqlServerCommandProvider().Delete<TModel>(connection, id, user);
		}

		public static void CreateTable<TModel>(this IDbConnection connection)
		{
			new SqlServerCommandProvider().CreateTable<TModel>(connection);
		}
	}
}
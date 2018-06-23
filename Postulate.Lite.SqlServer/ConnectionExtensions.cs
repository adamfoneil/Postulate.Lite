using Postulate.Lite.Core.Interfaces;
using System.Data;

namespace Postulate.Lite.SqlServer
{
	public static class ConnectionExtensions
	{
		public static TModel Find<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			var cmd = new SqlServerCommandProvider();
			return cmd.Find<TModel>(connection, id, user);
		}

		public static int Save<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			var cmd = new SqlServerCommandProvider();
			return cmd.Save(connection, @object, user);
		}

		public static int Insert<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			var cmd = new SqlServerCommandProvider();
			return cmd.Insert(connection, @object, user);
		}

		public static void Update<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			var cmd = new SqlServerCommandProvider();
			cmd.Update(connection, @object, user);
		}

		public static void Delete<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			var cmd = new SqlServerCommandProvider();
			cmd.Delete<TModel>(connection, id, user);
		}
	}
}
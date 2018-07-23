using Postulate.Lite.Core.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Postulate.Lite.SqlServer.IntKey
{
	public static class ConnectionExtensions
	{
		private static SqlServerProvider<int> GetProvider()
		{
			return new SqlServerProvider<int>((obj) => Convert.ToInt32(obj), "identity(1,1)");
		}

		public static bool Exists<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			return GetProvider().Exists<TModel>(connection, id, user);
		}

		public async static Task<bool> ExistsAsync<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			return await GetProvider().ExistsAsync<TModel>(connection, id, user);
		}

		public static bool ExistsWhere<TModel>(this IDbConnection connection, TModel criteria, IUser user = null)
		{
			return GetProvider().ExistsWhere(connection, criteria, user);
		}

		public async static Task<bool> ExistsWhereAsync<TModel>(this IDbConnection connection, TModel criteria, IUser user = null)
		{
			return await GetProvider().ExistsWhereAsync(connection, criteria, user);
		}

		public static TModel Find<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			return GetProvider().Find<TModel>(connection, id, user);
		}

		public async static Task<TModel> FindAsync<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			return await GetProvider().FindAsync<TModel>(connection, id, user);
		}

		public static TModel FindWhere<TModel>(this IDbConnection connection, TModel criteria, IUser user = null)
		{
			return GetProvider().FindWhere(connection, criteria, user);
		}

		public async static Task<TModel> FindWhereAsync<TModel>(this IDbConnection connection, TModel criteria, IUser user = null)
		{
			return await GetProvider().FindWhereAsync(connection, criteria, user);
		}

		public static int Merge<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Merge(connection, @object, user);
		}

		public async static Task<int> MergeAsync<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return await GetProvider().MergeAsync(connection, @object, user);
		}

		public static int Save<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Save(connection, @object, user);
		}

		public async static Task<int> SaveAsync<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return await GetProvider().SaveAsync(connection, @object, user);
		}

		public static int Insert<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Insert(connection, @object, user);
		}

		public async static Task<int> InsertAsync<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return await GetProvider().InsertAsync(connection, @object, user);
		}

		public static void Update<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			GetProvider().Update(connection, @object, user);
		}

		public async static Task UpdateAsync<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			await GetProvider().UpdateAsync(connection, @object, user);
		}

		public static void Delete<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			GetProvider().Delete<TModel>(connection, id, user);
		}

		public async static Task DeleteAsync<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			await GetProvider().DeleteAsync<TModel>(connection, id, user);
		}

		public static void CreateTable<TModel>(this IDbConnection connection)
		{
			GetProvider().CreateTable<TModel>(connection);
		}
	}
}
using Postulate.Lite.Core.Interfaces;
using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Postulate.Lite.SqlServer.LongKey
{
	public static class ConnectionExtensions
	{
		private static SqlServerProvider<long> GetProvider()
		{
			return new SqlServerProvider<long>((obj) => Convert.ToInt64(obj), "identity(1,1)");
		}

		public static bool Exists<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			return GetProvider().Exists<TModel>(connection, id, user);
		}

		public async static Task<bool> ExistsAsync<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			return await GetProvider().ExistsAsync<TModel>(connection, id, user);
		}

		public static bool ExistsWhere<TModel>(this IDbConnection connection, object criteria, IUser user = null)
		{
			return GetProvider().ExistsWhere<TModel>(connection, criteria, user);
		}

		public async static Task<bool> ExistsWhereAsync<TModel>(this IDbConnection connection, object criteria, IUser user = null)
		{
			return await GetProvider().ExistsWhereAsync<TModel>(connection, criteria, user);
		}

		public static TModel Find<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			return GetProvider().Find<TModel>(connection, id, user);
		}

		public async static Task<TModel> FindAsync<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			return await GetProvider().FindAsync<TModel>(connection, id, user);
		}

		public static TModel FindWhere<TModel>(this IDbConnection connection, object criteria, IUser user = null)
		{
			return GetProvider().FindWhere<TModel>(connection, criteria, user);
		}

		public async static Task<TModel> FindWhereAsync<TModel>(this IDbConnection connection, object criteria, IUser user = null)
		{
			return await GetProvider().FindWhereAsync<TModel>(connection, criteria, user);
		}

		public static long Merge<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Merge(connection, @object, user);
		}

		public async static Task<long> MergeAsync<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return await GetProvider().MergeAsync(connection, @object, user);
		}

		public static long Save<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Save(connection, @object, user);
		}

		public async static Task<long> SaveAsync<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return await GetProvider().SaveAsync(connection, @object, user);
		}

		public static void PlainInsert<TModel>(this IDbConnection connection, TModel @object, string tableName = null, IUser user = null)
		{
			GetProvider().PlainInsert(connection, @object, tableName, user);
		}

		public static async Task PlainInsertAsync<TModel>(this IDbConnection connection, TModel @object, string tableName = null, IUser user = null)
		{
			await GetProvider().PlainInsertAsync(connection, @object, tableName, user);
		}

		public static long Insert<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
		{
			return GetProvider().Insert(connection, @object, user);
		}

		public async static Task<long> InsertAsync<TModel>(this IDbConnection connection, TModel @object, IUser user = null)
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

		public static void Update<TModel>(this IDbConnection connection, TModel @object, IUser user, params Expression<Func<TModel, object>>[] setColumns)
		{
			GetProvider().Update(connection, @object, user, setColumns);
		}

		public async static Task UpdateAsync<TModel>(this IDbConnection connection, TModel @object, IUser user, params Expression<Func<TModel, object>>[] setColumns)
		{
			await GetProvider().UpdateAsync(connection, @object, user, setColumns);
		}

		public static void Delete<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			GetProvider().Delete<TModel>(connection, id, user);
		}

		public async static Task DeleteAsync<TModel>(this IDbConnection connection, int id, IUser user = null)
		{
			await GetProvider().DeleteAsync<TModel>(connection, id, user);
		}

		public static void CreateTable<TModel>(this IDbConnection connection, string tableName = null)
		{
			GetProvider().CreateTable<TModel>(connection, tableName);
		}
	}
}
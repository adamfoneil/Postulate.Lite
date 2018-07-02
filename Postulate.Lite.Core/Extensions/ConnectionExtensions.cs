using Dapper;
using System.Data;

namespace Postulate.Lite.Core.Extensions
{
	public static class ConnectionExtensions
	{
		public static bool Exists(this IDbConnection connection, string fromWhere, object parameters = null, IDbTransaction transaction = null)
		{
			return ((connection.QueryFirstOrDefault<int?>($"SELECT 1 FROM {fromWhere}", parameters, transaction) ?? 0) == 1);
		}
	}
}
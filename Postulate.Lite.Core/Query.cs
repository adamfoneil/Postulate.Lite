using Dapper;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Postulate.Lite.Core
{
	/// <summary>
	/// Encapsulates a SQL SELECT query with dynamic criteria that returns TResult
	/// </summary>	
	public class Query<TResult>
	{
		public Query(string sql)
		{
			Sql = sql;
		}

		public string Sql { get; private set; }
		public string ResolvedSql { get; private set; }

		public IEnumerable<TResult> Execute(IDbConnection connection)
		{
			ResolvedSql = ResolveQuery(this, out DynamicParameters queryParams);
			return connection.Query<TResult>(ResolvedSql, queryParams);
		}

		public TResult ExecuteSingle(IDbConnection connection)
		{
			ResolvedSql = ResolveQuery(this, out DynamicParameters queryParams);
			return connection.QuerySingle<TResult>(ResolvedSql, queryParams);
		}

		public async Task<TResult> ExecuteSingleAsync(IDbConnection connection)
		{
			ResolvedSql = ResolveQuery(this, out DynamicParameters queryParams);
			return await connection.QuerySingleAsync<TResult>(ResolvedSql, queryParams);
		}

		public async Task<IEnumerable<TResult>> ExecuteAsync(IDbConnection connection)
		{
			ResolvedSql = ResolveQuery(this, out DynamicParameters queryParams);
			return await connection.QueryAsync<TResult>(ResolvedSql, queryParams);
		}

		private static string ResolveQuery(Query<TResult> query, out DynamicParameters queryParams)
		{
			string result = query.Sql;

			List<string> terms = new List<string>();
			var queryProps = GetProperties(query, result, out IEnumerable<string> builtInParams);

			queryParams = new DynamicParameters();
			foreach (var prop in queryProps)
			{
				var value = prop.GetValue(query);
				if (value != null) queryParams.Add(prop.Name, value);
			}

			Dictionary<string, string> whereBuilder = new Dictionary<string, string>()
			{
				{ InternalStringExtensions.WhereToken, "WHERE" }, // query has no WHERE clause, so it will be added
				{ InternalStringExtensions.AndWhereToken, "AND" } // query already contains a WHERE clause, we're just adding to it
			};

			string token;
			if (result.ContainsAny(whereBuilder.Select(kp => kp.Key), out token))
			{
				bool anyCriteria = false;

				// loop through this query's properties, but ignore base properties (like ResolvedSql and TraceCallback) since they are never part of WHERE clause
				foreach (var pi in queryProps)
				{
					object value = pi.GetValue(query);
					if (HasValue(value))
					{
						// built-in params are not part of the WHERE clause, so they are excluded from added terms
						if (!builtInParams.Contains(pi.Name.ToLower()))
						{
							anyCriteria = true;

							var cases = pi.GetCustomAttributes(typeof(CaseAttribute), false).OfType<CaseAttribute>();
							var selectedCase = cases?.FirstOrDefault(c => c.Value.Equals(value));
							if (selectedCase != null)
							{
								terms.Add(selectedCase.Expression);
							}
							else
							{
								WhereAttribute whereAttr = pi.GetAttribute<WhereAttribute>();
								terms.Add(whereAttr.Expression);
							}
						}
					}
				}
				result = result.Replace(token, (anyCriteria) ? $"{whereBuilder[token]} {string.Join(" AND ", terms)}" : string.Empty);
			}

			return result;
		}

		private static bool HasValue(object value)
		{
			if (value != null)
			{
				if (value.Equals(string.Empty)) return false;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns the properties of a query object based on parameters defined in a
		/// SQL statement as well as properties with Where and Case attributes
		/// </summary>
		private static IEnumerable<PropertyInfo> GetProperties(object query, string sql, out IEnumerable<string> builtInParams)
		{
			// this gets the param names within the query based on words with leading '@'
			builtInParams = sql.GetParameterNames(true).Select(p => p.ToLower());
			var builtInParamsArray = builtInParams.ToArray();

			// these are the properties of the Query that are explicitly defined and may impact the WHERE clause
			var queryProps = query.GetType().GetProperties().Where(pi =>
				pi.HasAttribute<WhereAttribute>() ||
				pi.HasAttribute<CaseAttribute>() ||
				builtInParamsArray.Contains(pi.Name.ToLower()));

			return queryProps;
		}

		/// <summary>
		/// Intended for implementing <see cref="Interfaces.ITestableQuery"/> for unit testing, not intended for use on its own
		/// </summary>
		public IEnumerable<dynamic> TestExecuteHelper(IDbConnection connection)
		{
			try
			{
				ResolvedSql = ResolveQuery(this, out DynamicParameters queryParams);
				return connection.Query(ResolvedSql, queryParams);
			}
			catch (Exception exc)
			{
				throw new Exception($"Query {GetType().Name} failed: {exc.Message}", exc);
			}
		}
	}
}
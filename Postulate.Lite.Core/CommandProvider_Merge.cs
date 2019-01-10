using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Postulate.Lite.Core
{
	public abstract partial class CommandProvider<TKey>
	{
		public abstract string CommentPrefix { get; }

		/// <summary>
		/// Indicates whether the backend supports schemas in the SQL Server sense as a naming group
		/// </summary>
		public abstract bool SupportsSchemas { get; }

		/// <summary>
		/// Allows SQL Server to specify "dbo" as the default schema name for created tables
		/// </summary>
		public abstract string DefaultSchema { get; }

		#region command methods

		/// <summary>
		/// Generates a SQL create table statement for a given model class
		/// </summary>
		public abstract string CreateTableCommand(Type modelType, string tableName = null);

		public abstract string CreateSchemaCommand(string schemaName);

		#endregion command methods		

		#region schema inspection
	
		public abstract bool SchemaExists(IDbConnection connection, string schemaName);		

		protected IEnumerable<PropertyInfo> GetPrimaryKeyColumns(Type type, IEnumerable<PropertyInfo> columns, out bool identityIsPrimaryKey)
		{
			identityIsPrimaryKey = false;
			var result = columns.Where(pi => pi.HasAttribute<PrimaryKeyAttribute>());

			if (!result.Any())
			{
				identityIsPrimaryKey = true;
				result = new[] { type.GetIdentityProperty() };
			}

			return result;
		}

		#endregion schema inspection		

		protected abstract string PrimaryKeySyntax(string constraintName, IEnumerable<PropertyInfo> pkColumns);

		protected abstract string UniqueIdSyntax(string constraintName, PropertyInfo identityProperty);

		protected string CreateTableCommandInner(Type modelType, string tableName, bool requireIdentity = true)
		{
			string constraintName = tableName.Replace(".", "_");

			var columns = _integrator.GetMappedColumns(modelType);
			var pkColumns = GetPrimaryKeyColumns(modelType, columns, out bool identityIsPrimaryKey);

			string identityName = null;
			bool hasIdentity = false;
			if (requireIdentity)
			{
				identityName = modelType.GetIdentityName();
				hasIdentity = true;
			}
			else
			{
				identityName = modelType.TryGetIdentityName(string.Empty, ref hasIdentity);
			}

			List<string> members = new List<string>();
			members.AddRange(columns.Select(pi => SqlColumnSyntax(pi, (identityName.Equals(pi.Name)))));
			members.Add(PrimaryKeySyntax(constraintName, pkColumns));
			if (!identityIsPrimaryKey && hasIdentity) members.Add(UniqueIdSyntax(constraintName, modelType.GetIdentityProperty()));

			return
				$"CREATE TABLE {ApplyDelimiter(tableName)} (" +
					string.Join(",\r\n\t", members) +
				")";
		}
	}
}
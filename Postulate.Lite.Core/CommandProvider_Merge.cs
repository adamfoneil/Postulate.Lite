using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

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
		public abstract string CreateTableCommand(Type modelType);

		public abstract string CreateSchemaCommand(string schemaName);
		
		/// <summary>
		/// Generatea a SQL command to drop a table from a database
		/// </summary>
		public abstract string DropTableCommand(TableInfo tableInfo);

		/// <summary>
		/// Generates a SQL command to add a foreign key to a column
		/// </summary>
		public abstract string AddForeignKeyCommand(PropertyInfo propertyInfo);

		public abstract string AddForeignKeyCommand(ForeignKeyInfo foreignkeyInfo);

		/// <summary>
		/// Generates a SQL command to drop a table's primary key
		/// </summary>
		public abstract string DropPrimaryKeyCommand(Type modelType);

		/// <summary>
		/// Generates a SQL command to add a primary key to a table
		/// </summary>
		public abstract string AddPrimaryKeyCommand(Type modelType);

		/// <summary>
		/// Generates a SQL command to remove a foreign key from a column
		/// </summary>
		public abstract string DropForeignKeyCommand(ForeignKeyInfo foreignKeyInfo);

		/// <summary>
		/// Generates a SQL command to add a column to a table
		/// </summary>
		public abstract string AddColumnCommand(PropertyInfo propertyInfo);

		/// <summary>
		/// Generates a SQL command to alter the type, nullability, or size of a column
		/// </summary>
		public abstract string AlterColumnCommand(PropertyInfo propertyInfo);

		/// <summary>
		/// Copies provider-specific information about a foreign key to its corresponding ColumnInfo
		/// </summary>
		public abstract void MapProviderSpecificInfo(PropertyInfo pi, ColumnInfo col);

		/// <summary>
		/// Generates a SQL command to remove a column from a table
		/// </summary>
		public abstract string DropColumnCommand(ColumnInfo columnInfo);

		#endregion

		#region reflection methods
		public abstract TableInfo GetTableInfo(Type modelType);
		#endregion

		#region schema inspection
		public abstract IEnumerable<ForeignKeyInfo> GetDependentForeignKeys(IDbConnection connection, TableInfo tableInfo);

		public abstract bool SchemaExists(IDbConnection connection, string schemaName);

		public abstract bool IsTableEmpty(IDbConnection connection, Type modelType);

		#endregion
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
	}
}
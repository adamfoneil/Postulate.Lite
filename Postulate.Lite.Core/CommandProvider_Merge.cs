using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Metadata;
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
		/// Generates a SQL create table statement for a given model class
		/// </summary>		
		public abstract string CreateTableCommand(Type modelType);

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
		/// Generates a SQL command to remove a column from a table
		/// </summary>
		public abstract string DropColumnCommand(ColumnInfo columnInfo);

		public abstract IEnumerable<ForeignKeyInfo> GetDependentForeignKeys(IDbConnection connection, TableInfo tableInfo);

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
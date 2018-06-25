using Postulate.Lite.Core.Metadata;
using System;
using System.Reflection;

namespace Postulate.Lite.Core
{
	public abstract partial class CommandProvider<TKey>
	{
		/// <summary>
		/// Generates a SQL create table statement for a given model class
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		protected abstract string CreateTableCommand(Type modelType);

		/// <summary>
		/// Generatea a SQL command to drop a table from a database
		/// </summary>
		protected abstract string DropTableCommand(TableInfo tableInfo);

		/// <summary>
		/// Generates a SQL command to add a foreign key to a column
		/// </summary>
		protected abstract string AddForeignKeyCommand(PropertyInfo propertyInfo);

		/// <summary>
		/// Generates a SQL command to remove a foreign key from a column
		/// </summary>
		protected abstract string DropForeignKeyCommand(ForeignKeyInfo columnInfo);

		/// <summary>
		/// Generates a SQL command to add a column to a table
		/// </summary>
		protected abstract string AddColumnCommand(PropertyInfo propertyInfo);

		/// <summary>
		/// Generates a SQL command to alter the type, nullability, or size of a column
		/// </summary>
		protected abstract string AlterColumnCommand(PropertyInfo propertyInfo);

		/// <summary>
		/// Generates a SQL command to remove a column from a table
		/// </summary>
		protected abstract string DropColumnCommand(ColumnInfo columnInfo);
	}
}
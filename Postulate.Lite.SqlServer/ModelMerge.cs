using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Postulate.Lite.Core;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Models;

namespace Postulate.Lite.SqlServer
{
	public class ModelMerge<TKey> : Core.Merge.ModelMerge<TKey>
	{
		/// <summary>
		/// SQL Server schemas to exclude from merge operations
		/// </summary>
		public string[] ExcludeSchemas { get; set; } = new string[] { "changes", "meta", "delete" };

		public static ModelMerge<TKey> FromAssembly(CommandProvider<TKey> commandProvider, Assembly assembly, string @namespace = "")
		{
			var types = GetModelTypesFromAssembly(assembly, @namespace);

			var result = new ModelMerge<TKey>(commandProvider, types);

			var excludeSchemas = assembly.GetCustomAttributes<MergeExcludeSchemaAttribute>();
			if (excludeSchemas?.Any() ?? false)
			{
				result.ExcludeSchemas = excludeSchemas.Select(a => a.Schema).ToArray();
			}

			return result;
		}

		public ModelMerge(CommandProvider<TKey> commandProvider, IEnumerable<Type> modelTypes) : base(commandProvider, modelTypes)
		{
		}

		protected override string SchemaCriteria()
		{
			string schemaCriteria = string.Empty;
			if (ExcludeSchemas?.Any() ?? false)
			{
				schemaCriteria = $" AND SCHEMA_NAME([t].[schema_id]) NOT IN ({string.Join(", ", ExcludeSchemas.Select(s => $"'{s}'"))})";
			}

			return schemaCriteria;
		}

		protected async override Task<IEnumerable<ColumnInfo>> GetSchemaColumnsAsync(IDbConnection connection)
		{
			return await connection.QueryAsync<ColumnInfo>(
				$@"SELECT
	                SCHEMA_NAME([t].[schema_id]) AS [Schema], 
					[t].[name] AS [TableName], [c].[Name] AS [ColumnName],
	                TYPE_NAME([c].[system_type_id]) AS [DataType],
	                [c].[max_length] AS [ByteLength], 
					[c].[is_nullable] AS [IsNullable],
	                [c].[precision] AS [Precision], 
					[c].[scale] as [Scale], [c].[collation_name] AS [Collation], 
					[c].[is_computed] AS [IsCalculated],
	                SCHEMA_NAME([parentTbl].[schema_id]) AS [ReferencedSchema], 
					[parentTbl].[name] AS [ReferencedTable], 
					[parentCol].[name] AS [ReferencedColumn],
	                [fk].[name] AS [ForeignKeyConstraint],
					CONVERT(bit, CASE [fk].[delete_referential_action] WHEN 1 THEN 1 ELSE 0 END) AS [CascadeDelete],
					[ccol].[definition] AS [Expression]
                FROM
	                [sys].[tables] [t] INNER JOIN [sys].[columns] [c] ON [t].[object_id]=[c].[object_id]
	                LEFT JOIN [sys].[foreign_key_columns] [fkcol] ON
		                [c].[object_id]=[fkcol].[parent_object_id] AND
		                [c].[column_id]=[fkcol].[parent_column_id]
                    LEFT JOIN [sys].[foreign_keys] [fk] ON [fkcol].[constraint_object_id]=[fk].[object_id]
					LEFT JOIN [sys].[computed_columns] [ccol] ON [c].[object_id]=[ccol].[object_id] AND [c].[name]=[ccol].[name]
	                LEFT JOIN [sys].[columns] [parentCol] ON
		                [fkcol].[referenced_object_id]=[parentCol].[object_id] AND
		                [fkcol].[referenced_column_id]=[parentCol].[column_id]
	                LEFT JOIN [sys].[tables] [parentTbl] ON
		                [parentCol].[object_id]=[parentTbl].[object_id]
                    WHERE                        
                        ([t].[name] NOT LIKE 'AspNet%' OR [t].[name] LIKE 'AspNetUsers') AND
                        [t].[name] NOT LIKE '__MigrationHistory'{SchemaCriteria()}");
		}		

		protected async override Task<IEnumerable<TableInfo>> GetSchemaTablesAsync(IDbConnection connection)
		{
			// row count trick thanks to https://blogs.msdn.microsoft.com/martijnh/2010/07/15/sql-serverhow-to-quickly-retrieve-accurate-row-count-for-table/

			return await connection.QueryAsync<TableInfo>(
				$@"SELECT
                    SCHEMA_NAME([t].[schema_id]) AS [Schema], [t].[name] AS [Name],
					(SELECT SUM(row_count) FROM sys.dm_db_partition_stats WHERE object_id=[t].[object_id] AND (index_id=0 or index_id=1)) AS [RowCount]
                FROM
                    [sys].[tables] [t]
                WHERE                    
                    [name] NOT LIKE 'AspNet%' AND
                    [name] NOT LIKE '__MigrationHistory'{SchemaCriteria()}");
		}

		protected override Task<IEnumerable<string>> GetNewSchemasAsync(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override Task<IEnumerable<ColumnInfo>> GetAlteredColumnsAsync(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override Task<IEnumerable<ColumnInfo>> GetDeletedColumnsAsync(IEnumerable<TableInfo> dropTables, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override Task<IEnumerable<TableInfo>> GetDeletedTablesAsync(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override Task<IEnumerable<PropertyInfo>> GetNewColumnsAsync(IEnumerable<Type> modelTypes, IEnumerable<Type> omitTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}
	}
}

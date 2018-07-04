using Dapper;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Postulate.Lite.Core.Merge
{
	public enum DifferenceType
	{
		Added,
		Modified,
		Dropped
	}

	public class Engine<TKey>
	{
		public static Engine<TKey> FromAssembly(CommandProvider<TKey> commandProvider, Assembly assembly, string @namespace = "")
		{
			var types = assembly.GetTypes().Where(t =>
					!t.Name.StartsWith("<>") &&
					!t.IsAbstract &&
					!t.IsInterface &&
					t.Namespace.StartsWith(@namespace));			

			var result = new Engine<TKey>(commandProvider, types);

			var excludeSchemas = assembly.GetCustomAttributes<MergeExcludeSchemaAttribute>();
			if (excludeSchemas?.Any() ?? false)
			{
				result.ExcludeSchemas = excludeSchemas.Select(a => a.Schema).ToArray();
			}

			return result;
		}

		public Engine(CommandProvider<TKey> commandProvider, IEnumerable<Type> modelTypes)
		{
			CommandProvider = commandProvider;
			ModelTypes = modelTypes;
			ModelProperties = modelTypes.SelectMany(t => commandProvider.GetMappedColumns(t)).ToLookup(pi => pi.DeclaringType);
		}

		public CommandProvider<TKey> CommandProvider { get; private set; }
		public IEnumerable<Type> ModelTypes { get; private set; }
		public ILookup<Type, PropertyInfo> ModelProperties { get; private set; }
		public Stopwatch Stopwatch { get; private set; }

		/// <summary>
		/// SQL Server schemas to exclude from merge operations
		/// </summary>
		public string[] ExcludeSchemas { get; set; } = new string[] { "changes", "meta", "delete" };

		public async Task<IEnumerable<Action>> CompareAsync(IDbConnection connection)
		{
			Stopwatch = Stopwatch.StartNew();

			List<Action> results = new List<Action>();

			var schemaTables = await GetSchemaTablesAsync(connection);
			var modelTables = ModelTypes.Select(t => CommandProvider.GetTableInfo(t));
			var schemaColumns = await GetSchemaColumnsAsync(connection);
			var modelColumns = ModelTypes.SelectMany(t => CommandProvider.GetMappedColumns(t)).Select(pi =>
			{
				var col = new ColumnInfo(pi);
				CommandProvider.MapForeignKeyInfo(pi, col);
				return col;
			});

			if (CommandProvider.SupportsSchemas)
			{
				var createSchemas = await GetNewSchemasAsync(ModelTypes, connection);
				results.AddRange(createSchemas.Select(s => new CreateSchema(s)));
			}

			var createTables = GetNewTables(modelTables, schemaTables);
			results.AddRange(createTables.Select(tbl => new CreateTable(tbl.ModelType)));

			// when looking for modified columns, don't include tables that were just created
			var existingModelColumns = modelColumns.Where(col => !createTables.Contains(col.GetTableInfo()));

			if (AnyModifiedColumns(schemaTables, schemaColumns, existingModelColumns,
				out IEnumerable<ColumnInfo> added, out IEnumerable<ColumnInfo> modified, out IEnumerable<ColumnInfo> deleted))
			{
				
			}					

			//var addColumns = await GetNewColumnsAsync(ModelTypes, createTables.Concat(rebuiltTables), connection);
			//results.AddRange(addColumns.Select(pi => new AddColumn(pi)));

			var dropTables = await GetDeletedTablesAsync(ModelTypes, connection);
			results.AddRange(dropTables.Select(tbl => new DropTable(tbl)));

			var dropColumns = await GetDeletedColumnsAsync(dropTables, connection);
			results.AddRange(dropColumns.Select(col => new DropColumn(col)));

			var alterColumns = await GetAlteredColumnsAsync(ModelTypes, connection);

			Stopwatch.Stop();

			return results;
		}

		private IEnumerable<TableInfo> GetExistingTables(IEnumerable<TableInfo> modelTables, IEnumerable<TableInfo> schemaTables)
		{
			return from mt in modelTables
				   join st in schemaTables on mt equals st
				   select mt;
		}

		private async Task<IEnumerable<string>> GetNewSchemasAsync(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private async Task<IEnumerable<ColumnInfo>> GetSchemaColumnsAsync(IDbConnection connection)
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

		private async Task<IEnumerable<TableInfo>> GetSchemaTablesAsync(IDbConnection connection)
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

		private string SchemaCriteria()
		{
			string schemaCriteria = string.Empty;
			if (ExcludeSchemas?.Any() ?? false)
			{
				schemaCriteria = $" AND SCHEMA_NAME([t].[schema_id]) NOT IN ({string.Join(", ", ExcludeSchemas.Select(s => $"'{s}'"))})";
			}

			return schemaCriteria;
		}

		private async Task<IEnumerable<ColumnInfo>> GetAlteredColumnsAsync(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private async Task<IEnumerable<ColumnInfo>> GetDeletedColumnsAsync(IEnumerable<TableInfo> dropTables, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private async Task<IEnumerable<TableInfo>> GetDeletedTablesAsync(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private async Task<IEnumerable<PropertyInfo>> GetNewColumnsAsync(IEnumerable<Type> modelTypes, IEnumerable<Type> omitTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private bool AnyModifiedColumns(
			IEnumerable<TableInfo> schemaTables, IEnumerable<ColumnInfo> schemaColumns, IEnumerable<ColumnInfo> modelColumns,
			out IEnumerable<ColumnInfo> added, out IEnumerable<ColumnInfo> modified, out IEnumerable<ColumnInfo> deleted)
		{
			added = modelColumns.Where(col => !schemaColumns.Contains(col));			

			deleted = schemaColumns.Where(col => !modelColumns.Contains(col));			

			modified = from mc in modelColumns
							join sc in schemaColumns on mc equals sc
							where mc.IsAlteredFrom(sc)
							select mc;

			return (added.Any() || modified.Any() || deleted.Any());
		}

		private IEnumerable<TableInfo> GetNewTables(IEnumerable<TableInfo> modelTables, IEnumerable<TableInfo> schemaTables)
		{
			return modelTables.Where(mt => !schemaTables.Contains(mt));
		}

		public StringBuilder GetScript(IEnumerable<Action> actions, IDbConnection connection)
		{
			StringBuilder result = new StringBuilder();

			foreach (var a in actions)
			{
				foreach (var cmd in a.SqlCommands(CommandProvider, connection))
				{
					result.Append(cmd);
					result.AppendLine();
				}
			}

			return result;
		}
	}
}
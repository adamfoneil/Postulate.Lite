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
	public class ModelMerge<TKey>
	{
		public ModelMerge(CommandProvider<TKey> commandProvider, IEnumerable<Type> modelTypes)
		{
			CommandProvider = commandProvider;
			ModelTypes = modelTypes;
			ModelTables = modelTypes.Select(t => CommandProvider.GetTableInfo(t));

			var mappedColumns = modelTypes.SelectMany(t => commandProvider.GetMappedColumns(t));

			ModelProperties = mappedColumns.ToLookup(pi => pi.DeclaringType);

			ModelColumns = mappedColumns.Select(pi =>
			{
				var col = new ColumnInfo(pi);
				CommandProvider.MapProviderSpecificInfo(pi, col);
				return col;
			});
		}

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

		protected static IEnumerable<Type> GetModelTypesFromAssembly(Assembly assembly, string @namespace = "")
		{
			return assembly.GetTypes().Where(t =>
					!t.Name.StartsWith("<>") &&
					!t.IsAbstract &&
					!t.IsInterface &&
					t.Namespace.StartsWith(@namespace));
		}

		/// <summary>
		/// SQL Server schemas to exclude from the merge
		/// </summary>
		public string[] ExcludeSchemas { get; set; }

		public CommandProvider<TKey> CommandProvider { get; private set; }
		public IEnumerable<Type> ModelTypes { get; private set; }
		public IEnumerable<ColumnInfo> ModelColumns { get; private set; }
		public ILookup<Type, PropertyInfo> ModelProperties { get; private set; }
		public IEnumerable<TableInfo> ModelTables { get; private set; }
		public Stopwatch Stopwatch { get; private set; }
		
		public async Task<IEnumerable<Action>> CompareAsync(IDbConnection connection)
		{
			Stopwatch = Stopwatch.StartNew();

			var schemaTables = await CommandProvider.GetSchemaTablesAsync(connection, ExcludeSchemas);
			var schemaColumns = await CommandProvider.GetSchemaColumnsAsync(connection, ExcludeSchemas);

			var results = Compare(schemaTables, schemaColumns);

			Stopwatch.Stop();

			return results;
		}

		public IEnumerable<Action> Compare(IEnumerable<TableInfo> schemaTables, IEnumerable<ColumnInfo> schemaColumns)
		{
			List<Action> results = new List<Action>();

			if (CommandProvider.SupportsSchemas)
			{
				var createSchemas = GetNewSchemas(ModelTables, schemaTables);
				results.AddRange(createSchemas.Select(s => new CreateSchema(s)));
			}

			var createTables = GetNewTables(ModelTables, schemaTables);
			results.AddRange(createTables.Select(tbl => new CreateTable(tbl.ModelType)));

			// when looking for modified columns, don't include tables that were just created
			var existingModelColumns = ModelColumns.Where(col => !createTables.Contains(col.TableInfo));

			if (AnyModifiedColumns(schemaTables, schemaColumns, existingModelColumns,
				out IEnumerable<ColumnInfo> added, out IEnumerable<ColumnInfo> modified, out IEnumerable<ColumnInfo> deleted))
			{
			}

			//var addColumns = await GetNewColumnsAsync(ModelTypes, createTables.Concat(rebuiltTables), connection);
			//results.AddRange(addColumns.Select(pi => new AddColumn(pi)));

			var dropTables = GetDeletedTables(ModelTables, schemaTables);
			results.AddRange(dropTables.Select(tbl => new DropTable(tbl)));

			var newForeignKeys = ModelColumns.Where(col => createTables.Contains(col.TableInfo) && col.IsForeignKey);
			results.AddRange(newForeignKeys.Select(col => new AddForeignKey(col.ForeignKeyInfo)));

			return results;
		}

		private IEnumerable<string> GetNewSchemas(IEnumerable<TableInfo> modelTables, IEnumerable<TableInfo> schemaTables)
		{
			var modelSchemas = SchemasFromTables(modelTables);
			var schemaSchemas = SchemasFromTables(schemaTables);
			return modelSchemas.Where(s => !schemaSchemas.Contains(s) && !s.ToLower().Equals(CommandProvider.DefaultSchema.ToLower()));
		}

		private static IEnumerable<string> SchemasFromTables(IEnumerable<TableInfo> modelTables)
		{
			return modelTables.Select(t => t.Schema).GroupBy(t => t).Select(grp => grp.Key);
		}

		private IEnumerable<TableInfo> GetDeletedTables(IEnumerable<TableInfo> modelTables, IEnumerable<TableInfo> schemaTables)
		{
			return schemaTables.Where(tbl => !modelTables.Contains(tbl));
		}

		private IEnumerable<TableInfo> GetExistingTables(IEnumerable<TableInfo> modelTables, IEnumerable<TableInfo> schemaTables)
		{
			return from mt in modelTables
				   join st in schemaTables on mt equals st
				   select mt;
		}

		/*
		protected abstract Task<IEnumerable<string>> GetNewSchemasAsync(IEnumerable<Type> modelTypes, IDbConnection connection);

		protected abstract Task<IEnumerable<ColumnInfo>> GetSchemaColumnsAsync(IDbConnection connection);

		protected abstract Task<IEnumerable<TableInfo>> GetSchemaTablesAsync(IDbConnection connection);
	
		protected abstract Task<IEnumerable<ColumnInfo>> GetAlteredColumnsAsync(IEnumerable<Type> modelTypes, IDbConnection connection);

		protected abstract Task<IEnumerable<ColumnInfo>> GetDeletedColumnsAsync(IEnumerable<TableInfo> dropTables, IDbConnection connection);

		protected abstract Task<IEnumerable<TableInfo>> GetDeletedTablesAsync(IEnumerable<Type> modelTypes, IDbConnection connection);

		protected abstract Task<IEnumerable<PropertyInfo>> GetNewColumnsAsync(IEnumerable<Type> modelTypes, IEnumerable<Type> omitTypes, IDbConnection connection);
		*/

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
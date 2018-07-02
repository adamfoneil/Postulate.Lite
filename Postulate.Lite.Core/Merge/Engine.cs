using Postulate.Lite.Core.Metadata;
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
	public class Engine<TKey>
	{
		public static Engine<TKey> FromAssembly(CommandProvider<TKey> commandProvider, Assembly assembly, string @namespace = "")
		{
			var types = assembly.GetTypes().Where(t =>
					!t.Name.StartsWith("<>") &&
					!t.IsAbstract &&
					!t.IsInterface &&
					t.Namespace.StartsWith(@namespace));

			return new Engine<TKey>(commandProvider, types);
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

		public async Task<IEnumerable<Action>> CompareAsync(IDbConnection connection)
		{
			Stopwatch = Stopwatch.StartNew();

			List<Action> results = new List<Action>();

			var schemaTables = await GetSchemaTablesAsync(connection);
			var schemaColumns = await GetSchemaColumnsAsync(connection);
			var schemaFKs = await GetSchemaForeignKeysAsync(connection);

			if (CommandProvider.SupportsSchemas)
			{
				var createSchemas = await GetNewSchemasAsync(ModelTypes, connection);
				results.AddRange(createSchemas.Select(s => new CreateSchema(s)));
			}

			var createTables = await GetNewTablesAsync(ModelTypes, connection);
			results.AddRange(createTables.Select(t => new CreateTable(t)));

			var rebuiltTables = await GetModifiedEmptyTablesAsync(ModelTypes, connection);
			results.AddRange(rebuiltTables.Select(t => new RebuildTable(t)));

			var addColumns = await GetNewColumnsAsync(ModelTypes, createTables.Concat(rebuiltTables), connection);
			results.AddRange(addColumns.Select(pi => new AddColumn(pi)));

			var dropTables = await GetDeletedTablesAsync(ModelTypes, connection);
			results.AddRange(dropTables.Select(tbl => new DropTable(tbl)));

			var dropColumns = await GetDeletedColumnsAsync(dropTables, connection);
			results.AddRange(dropColumns.Select(col => new DropColumn(col)));

			var alterColumns = await GetAlteredColumnsAsync(ModelTypes, connection);

			Stopwatch.Stop();

			return results;
		}

		private async Task<IEnumerable<string>> GetNewSchemasAsync(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private async Task<IEnumerable<ForeignKeyInfo>> GetSchemaForeignKeysAsync(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private async Task<IEnumerable<ColumnInfo>> GetSchemaColumnsAsync(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private Task<IEnumerable<TableInfo>> GetSchemaTablesAsync(IDbConnection connection)
		{
			throw new NotImplementedException();
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

		private async Task<IEnumerable<Type>> GetModifiedEmptyTablesAsync(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private async Task<IEnumerable<Type>> GetNewTablesAsync(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
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
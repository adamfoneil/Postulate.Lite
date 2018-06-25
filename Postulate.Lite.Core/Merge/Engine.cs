using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Postulate.Lite.Core.Metadata;

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
		}

		public CommandProvider<TKey> CommandProvider { get; private set; }
		public IEnumerable<Type> ModelTypes { get; private set; }

		public async Task<IEnumerable<Action>> CompareAsync(IDbConnection connection)
		{
			List<Action> results = new List<Action>();

			var createTables = GetNewTables(ModelTypes, connection);
			results.AddRange(createTables.Select(t => new CreateTable(t)));

			var modifiedTables = GetModifiedEmptyTables(ModelTypes, connection);
			results.AddRange(modifiedTables.Select(t => new RebuildTable(t)));

			var addColumns = GetNewColumns(ModelTypes, createTables.Concat(modifiedTables), connection);
			results.AddRange(addColumns.Select(pi => new AddColumn(pi)));

			var dropTables = GetDeletedTables(ModelTypes, connection);
			results.AddRange(dropTables.Select(tbl => new DropTable(tbl)));

			return results;
		}

		private IEnumerable<TableInfo> GetDeletedTables(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private IEnumerable<PropertyInfo> GetNewColumns(IEnumerable<Type> modelTypes, IEnumerable<Type> omitTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private IEnumerable<Type> GetModifiedEmptyTables(IEnumerable<Type> modelTypes, IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		private IEnumerable<Type> GetNewTables(IEnumerable<Type> modelTypes, IDbConnection connection)
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
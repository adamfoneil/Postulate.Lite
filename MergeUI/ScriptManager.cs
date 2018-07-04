using Postulate.Lite.Core;
using Postulate.Lite.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MergeUI
{
	internal class ScriptManager
	{
		private string _assemblyFile = null;
		private SupportedDatabases _databaseType;

		public Assembly Assembly { get; private set; }
		public Configuration Configuration { get; private set; }
		public SupportedDatabases DatabaseType { get; private set; }

		private ScriptManager(string fileName)
		{
			_assemblyFile = fileName;
		}

		public static ScriptManager FromAssemblyFile(string fileName)
		{
			ScriptManager result = new ScriptManager(fileName);

			result.Assembly = TryLoadAssembly(fileName);
			result.Configuration = ConfigurationManager.OpenExeConfiguration(result.Assembly.Location);
			result.DatabaseType = TryGetDatabaseType(result.Assembly);

			return result;
		}		

		private static Assembly TryLoadAssembly(string fileName)
		{
			try
			{
				return Assembly.LoadFile(fileName);
			}
			catch (Exception exc)
			{
				throw new Exception($"Error loading assembly: {exc.Message}", exc);
			}
		}

		private static SupportedDatabases TryGetDatabaseType(Assembly assembly)
		{
			try
			{
				CommandProviderAttribute attr = assembly.GetCustomAttribute<CommandProviderAttribute>();
				return attr.DatabaseType;
			}
			catch (Exception exc)
			{
				throw new Exception($"Error getting database type: {exc.Message}", exc);
			}
		}
	}
}

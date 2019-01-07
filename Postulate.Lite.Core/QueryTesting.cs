using Postulate.Lite.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.Core
{
	public static class QueryTesting
	{
		/// <summary>
		/// Gets the testable queries in an assembly that's accessible from the current assembly
		/// </summary>		
		/// <param name="executing">Normally use Assembly.GetExecutingAssembly()</param>
		/// <param name="assemblyName">Use the name of the assembly in your solution that has queries you want to test</param>
		public static IEnumerable<ITestableQuery> GetTestCases(Assembly executing, string assemblyName)
		{
			var assembly = FindReferencedAssembly(executing, assemblyName);
			var testableQueries = assembly.GetExportedTypes().Where(t => t.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ITestableQuery)));
			return testableQueries.SelectMany(t =>
			{
				var method = t.GetMethod("GetTestCases");
				if (method?.IsStatic ?? false)
				{
					return method.Invoke(null, null) as IEnumerable<ITestableQuery>;
				}
				return Enumerable.Empty<ITestableQuery>();
			});
		}

		private static Assembly FindReferencedAssembly(Assembly executing, string assemblyName)
		{
			var referenced = executing.GetReferencedAssemblies().ToArray();
			try
			{
				var name = referenced.Single(a => a.Name.Equals(assemblyName));
				return Assembly.Load(name);
			}
			catch (InvalidOperationException)
			{
				string available = string.Join(", ", referenced.Select(r => r.Name));
				throw new Exception($"Couldn't find assembly '{assemblyName}' among these: {available}");
			}
		}
	}
}
using Postulate.Lite.Core;
using Postulate.Lite.Core.Merge;
using System;
using System.Data;
using Tests.Models;

namespace Tests
{
	public abstract class MergeBase
	{
		protected abstract IDbConnection GetConnection();

		protected abstract CommandProvider<int> GetIntProvider();

		protected void CreateSomeTables()
		{
			var provider = GetIntProvider();
			var engine = new Engine<int>(provider, new Type[] { typeof(EmployeeInt), typeof(Organization) });
		}
	}
}
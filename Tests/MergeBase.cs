using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core;
using Postulate.Lite.Core.Merge;
using Postulate.Lite.Core.Models;
using System;
using System.Data;
using System.Linq;
using Tests.Models;

namespace Tests
{
	public abstract class MergeBase
	{
		protected abstract IDbConnection GetConnection();

		protected abstract CommandProvider<int> GetIntProvider();

		protected void CreateTwoTablesBase()
		{
			var provider = GetIntProvider();
			var engine = new Engine<int>(provider, new Type[] { typeof(EmployeeInt), typeof(Organization) });
			var schemaTables = Enumerable.Empty<TableInfo>();
			var schemaColumns = Enumerable.Empty<ColumnInfo>();
			var actions = engine.Compare(schemaTables, schemaColumns);
			Assert.IsTrue(actions.Any(a => (a as CreateTable)?.ModelType.Equals(typeof(EmployeeInt)) ?? false), "Employee table not created");
			Assert.IsTrue(actions.Any(a => (a as CreateTable)?.ModelType.Equals(typeof(Organization)) ?? false), "Organization table not created.");
			Assert.IsTrue(actions.Any(a => (a as AddForeignKey)?.ForeignKeyInfo.Parent.Equals(new ColumnInfo("dbo", "Organization", "Id")) ?? false), "Foreign key not created");
		}

		/// <summary>
		/// Create the org table when all that exists already is Employee
		/// </summary>
		protected void CreateOrgTableBase()
		{
			var provider = GetIntProvider();
			var engine = new Engine<int>(provider, new Type[] { typeof(EmployeeInt), typeof(Organization) });
			var schemaTables = new TableInfo[] { new TableInfo("dbo", "Employee") };
			var schemaColumns = Enumerable.Empty<ColumnInfo>();
			var actions = engine.Compare(schemaTables, schemaColumns);
			Assert.IsTrue(
				actions.Count() == 1 && 
				actions.All(a => provider.GetTableInfo((a as CreateTable).ModelType).Equals(new TableInfo("dbo", "Organization"))));
		}		
	}
}
﻿using AdamOneilSoftware;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tests.Models;
using Tests.Queries;

namespace Tests
{
	public abstract partial class TestBase
	{
		protected abstract IDbConnection GetConnection();

		protected abstract CommandProvider<int> GetIntProvider();

		protected void DropTable(IDbConnection cn, string tableName)
		{
			try
			{
				cn.Execute($"DROP TABLE {tableName}");
			}
			catch
			{
				// ignore error
			}
		}

		protected void DropAndCreateEmpTableBase()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, "Employee");
				GetIntProvider().CreateTable<EmployeeInt>(cn);
			}
		}

		protected void DropAndCreateOrgTableBase()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, "Organization");
				GetIntProvider().CreateTable<Organization>(cn);
			}
		}

		protected void InsertEmployeesBase()
		{
			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				DropTable(cn, "Organization");
				provider.CreateTable<Organization>(cn);

				DropTable(cn, "Employee");
				provider.CreateTable<EmployeeInt>(cn);

				var tdg = new TestDataGenerator();
				tdg.Generate<Organization>(5, (record) =>
				{
					record.Name = tdg.Random(Source.UniqueWidget);
				}, (records) =>
				{
					foreach (var record in records) provider.Save(cn, record);
				});

				var orgIds = cn.Query<int>("SELECT Id FROM Organization").ToArray();

				tdg.Generate<EmployeeInt>(100, (record) =>
				{
					record.OrganizationId = tdg.Random(orgIds);
					record.FirstName = tdg.Random(Source.FirstName);
					record.LastName = tdg.Random(Source.LastName);
					record.Email = $"{record.FirstName}.{record.LastName}@nowhere.org";
				}, (records) =>
				{
					foreach (var record in records) provider.Save(cn, record);
				});
			}
		}

		protected void DeleteEmployeeBase()
		{
			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				var emp = new EmployeeInt() { FirstName = "Whoever", LastName = "Nobody" };
				provider.Save(cn, emp);
				provider.Delete<EmployeeInt>(cn, emp.Id);
				Assert.IsTrue(!provider.Exists<EmployeeInt>(cn, emp.Id));
			}
		}

		protected async Task DeleteEmployeeBaseAsync()
		{
			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				var emp = new EmployeeInt() { FirstName = "Whoever", LastName = "Nobody" };
				await provider.SaveAsync(cn, emp);
				await provider.DeleteAsync<EmployeeInt>(cn, emp.Id);
				Assert.IsTrue(!provider.Exists<EmployeeInt>(cn, emp.Id));
			}
		}

		protected void UpdateEmployeeBase()
		{
			InsertEmployeesBase();

			const string name = "Django";

			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				var e = provider.Find<EmployeeInt>(cn, 5);
				e.FirstName = name;
				provider.Save(cn, e);

				e = provider.Find<EmployeeInt>(cn, 5);
				Assert.IsTrue(e.FirstName.Equals(name));
			}
		}

		protected void UpdateEmployeeColumnsBase()
		{
			InsertEmployeesBase();

			const string name = "Django";

			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				var e = provider.Find<EmployeeInt>(cn, 5);
				e.FirstName = name;
				provider.Update(cn, e, null, r => r.FirstName);

				e = provider.Find<EmployeeInt>(cn, 5);
				Assert.IsTrue(e.FirstName.Equals(name));
			}
		}

		protected async Task UpdateEmployeeBaseAsync()
		{
			InsertEmployeesBase();

			const string name = "Django";

			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				var e = await provider.FindAsync<EmployeeInt>(cn, 5);
				e.FirstName = name;
				await provider.SaveAsync(cn, e);

				e = await provider.FindAsync<EmployeeInt>(cn, 5);
				Assert.IsTrue(e.FirstName.Equals(name));
			}
		}

		protected async Task UpdateEmployeeColumnsBaseAsync()
		{
			InsertEmployeesBase();

			const string name = "Django";

			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				var e = await provider.FindAsync<EmployeeInt>(cn, 5);
				e.FirstName = name;
				await provider.UpdateAsync(cn, e, null, r => r.FirstName);

				e = await provider.FindAsync<EmployeeInt>(cn, 5);
				Assert.IsTrue(e.FirstName.Equals(name));
			}
		}

		protected void SaveEmployeeBase()
		{
			var e = new EmployeeInt()
			{
				OrganizationId = 1,
				FirstName = "Adam",
				LastName = "O'Neil",
				HireDate = new DateTime(2012, 1, 1)
			};

			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				provider.Save(cn, e);
				provider.Delete<EmployeeInt>(cn, e.Id);
			}
		}

		protected async Task SaveEmployeeBaseAsync()
		{
			var e = new EmployeeInt()
			{
				OrganizationId = 1,
				FirstName = "Adam",
				LastName = "O'Neil",
				HireDate = new DateTime(2012, 1, 1)
			};

			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				await provider.SaveAsync(cn, e);
				await provider.DeleteAsync<EmployeeInt>(cn, e.Id);
			}
		}

		protected void ForeignKeyLookupBase()
		{
			using (var cn = GetConnection())
			{
				var e = GetIntProvider().Find<EmployeeInt>(cn, 10);
				Assert.IsTrue(e.Organization != null);
			}
		}

		protected async Task ForeignKeyLookupBaseAsync()
		{
			using (var cn = GetConnection())
			{
				var e = await GetIntProvider().FindAsync<EmployeeInt>(cn, 10);
				Assert.IsTrue(e.Organization != null);
			}
		}

		protected void FindWhereEmployeeBase()
		{
			InsertEmployeesBase();

			using (var cn = GetConnection())
			{
				// there has to be an Id = 3 in there, I'm sure
				var e = GetIntProvider().FindWhere<EmployeeInt>(cn, new { Id = 3 });
				Assert.IsTrue(e.Id == 3);
			}
		}

		protected async Task FindWhereEmployeeBaseAsync()
		{
			InsertEmployeesBase();

			using (var cn = GetConnection())
			{
				// there has to be an Id = 3 in there, I'm sure
				var e = await GetIntProvider().FindWhereAsync<EmployeeInt>(cn, new { Id = 3 });
				Assert.IsTrue(e.Id == 3);
			}
		}

		protected void FindEmployeeBase()
		{
			InsertEmployeesBase();

			using (var cn = GetConnection())
			{
				var e = GetIntProvider().Find<EmployeeInt>(cn, 5);
				Assert.IsTrue(e.Id == 5);
			}
		}

		protected async Task FindEmployeeBaseAsync()
		{
			InsertEmployeesBase();

			using (var cn = GetConnection())
			{
				var e = await GetIntProvider().FindAsync<EmployeeInt>(cn, 5);
				Assert.IsTrue(e.Id == 5);
			}
		}

		protected void MergeOrgBase()
		{
			var provider = GetIntProvider();
			using (var cn = GetConnection())
			{
				var org = new Organization() { Name = "Test" };
				var check = provider.FindWhere<Organization>(cn, org);
				if (check != null) provider.Delete<Organization>(cn, check.Id);

				provider.Save(cn, org);

				var orgMerge = new Organization() { Name = "Test", EmployeeCount = 10 };
				provider.Merge(cn, orgMerge, out SaveAction action);
				Assert.IsTrue(action == SaveAction.Update);

				var findOrg = provider.Find<Organization>(cn, orgMerge.Id);
				Assert.IsTrue(findOrg.EmployeeCount == 10);
			}
		}

		protected async Task MergeOrgBaseAsync()
		{
			var provider = GetIntProvider();
			using (var cn = GetConnection())
			{
				var org = new Organization() { Name = "Test" };
				var check = await provider.FindWhereAsync<Organization>(cn, org);
				if (check != null) await provider.DeleteAsync<Organization>(cn, check.Id);

				await provider.SaveAsync(cn, org);

				var orgMerge = new Organization() { Name = "Test" };
				await provider.MergeAsync(cn, orgMerge);
				Assert.IsTrue(!provider.IsNew(orgMerge));
			}
		}

		protected void CreateOrgTableWithCustomNameBase()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, CustomTableName);
				GetIntProvider().CreateTable<Organization>(cn, CustomTableName);
			}
		}		

		protected void CommonCrudWithCustomTableBase()
		{
			const string customEmpTable = "EmpSample";
			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				DropTable(cn, customEmpTable);
				provider.CreateTable<EmployeeInt>(cn, customEmpTable);

				// save
				var e = new EmployeeInt() { FirstName = "Nobody", LastName = "Name", Email = "Whatever@Nowhere.org" };
				provider.Save(cn, e, tableName: customEmpTable);
				int id = e.Id;

				// find
				e = provider.Find<EmployeeInt>(cn, id);
				Assert.IsTrue(e.Id == id);

				// should really do FindWhere and Merge

				// delete
				provider.Delete<EmployeeInt>(cn, e.Id, tableName: customEmpTable);
				Assert.IsTrue(!provider.Exists<EmployeeInt>(cn, id, tableName: customEmpTable));
			}
		}

		protected void CommonAsyncCrudWithCustomTableBase()
		{
			const string customEmpTable = "EmpSample";
			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				DropTable(cn, customEmpTable);
				provider.CreateTable<EmployeeInt>(cn, customEmpTable);

				// save
				var e = new EmployeeInt() { FirstName = "Nobody", LastName = "Name", Email = "Whatever@Nowhere.org" };
				provider.SaveAsync(cn, e, tableName: customEmpTable).Wait();
				int id = e.Id;

				// find
				e = provider.FindAsync<EmployeeInt>(cn, id).Result;
				Assert.IsTrue(e.Id == id);

				// delete
				provider.DeleteAsync<EmployeeInt>(cn, e.Id, tableName: customEmpTable).Wait();
				Assert.IsTrue(!provider.ExistsAsync<EmployeeInt>(cn, id, tableName: customEmpTable).Result);
			}
		}

		protected void EmployeeQueryLastNameBase()
		{
			InsertEmployeesBase();

			var qry = new EmployeesByLastName(GetEmployeeQueryByLastNameSyntax()) { LastName = "a%" };

			using (var cn = GetConnection())
			{
				var results = qry.Execute(cn);
				Assert.IsTrue(results.All(r => r.LastName.ToLower().StartsWith("a")));
			}
		}

		/// <summary>
		/// Query EmployeeInt table with single param WHERE LastName LIKE @lastName
		/// </summary>
		/// <returns></returns>
		protected abstract string GetEmployeeQueryByLastNameSyntax();

		protected abstract string CustomTableName { get; }
	}
}
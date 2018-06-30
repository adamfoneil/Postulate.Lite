﻿using AdamOneilSoftware;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core;
using System;
using System.Data;
using System.Linq;
using Tests.Models;

namespace Tests
{
	public abstract class CrudBase
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

		protected void DropAndCreateTableBase()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, "EmployeeInt");
				GetIntProvider().CreateTable<EmployeeInt>(cn);
			}
		}

		protected void InsertEmployeesBase()
		{
			var provider = GetIntProvider();

			using (var cn = GetConnection())
			{
				DropTable(cn, "Organization");
				provider.CreateTable<Organization>(cn);

				DropTable(cn, "EmployeeInt");
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

		protected void ForeignKeyLookupBase()
		{			
			using (var cn = GetConnection())
			{
				var e = GetIntProvider().Find<EmployeeInt>(cn, 10);
				Assert.IsTrue(e.Organization != null);
			}
		}

		protected void FindWhereEmployeeBase()
		{
			InsertEmployeesBase();

			using (var cn = GetConnection())
			{
				// there has to be an Id = 3 in there, I'm sure
				var e = GetIntProvider().FindWhere(cn, new EmployeeInt() { Id = 3 });
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
	}
}
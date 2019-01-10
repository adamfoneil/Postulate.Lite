﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core;
using Postulate.Lite.Core.Exceptions;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Interfaces;
using Postulate.Lite.SqlServer.IntKey;
using Postulate.Lite.SqlServer;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Tests.Models;
using Dapper;

namespace Tests
{
	[TestClass]
	public class ModelTests
	{
		[TestMethod]
		public void EmployeeIdentityFirstColumn()
		{
			SqlIntegrator integrator = new SqlServerIntegrator();
			var props = integrator.GetMappedColumns(typeof(EmployeeInt));
			var identity = typeof(EmployeeInt).GetIdentityProperty();
			Assert.IsTrue(props.First().Equals(identity));
		}

		[TestMethod]
		public void OrganizationIdentityLastColumn()
		{
			SqlIntegrator integrator = new SqlServerIntegrator();
			var props = integrator.GetMappedColumns(typeof(Organization));
			var identity = typeof(Organization).GetIdentityProperty();
			Assert.IsTrue(props.Last().Equals(identity));
		}

		[TestMethod]
		public void EmployeesToDataTable()
		{
			var employees = new EmployeeInt[]
			{
				new EmployeeInt() { FirstName = "Adam", LastName = "O'Neil", HireDate = new DateTime(2010, 1, 1)},
				new EmployeeInt() { FirstName = "Roan", LastName = "Hastenmeyer", HireDate = new DateTime(2009, 1, 1) }
			};
			var table = employees.ToDataTable(new SqlServerIntegrator());
			Assert.IsTrue(table.Rows.Count == 2 && table.TableName.Equals("dbo.Employee"));
		}

		[TestMethod]
		public void OrgsToDataTable()
		{
			Organization[] orgs = GetSampleOrgs();
			var table = orgs.ToDataTable(new SqlServerIntegrator());
			Assert.IsTrue(table.Rows.Count == 3 && table.PrimaryKey.Length == 1 && table.TableName.Equals("dbo.Organization"));
		}

		[TestMethod]
		public void OrgsExcludeIdentity()
		{
			var orgs = GetSampleOrgs();
			var table = orgs.ToDataTable(new SqlServerIntegrator(), true);
			Assert.IsTrue(table.Columns.Count == 2);
		}

		private static Organization[] GetSampleOrgs()
		{
			return new Organization[]
			{
				new Organization() { Name = "Whatever" },
				new Organization() { Name = "Yes"},
				new Organization() { Name = "Gonglethredix" }
			};
		}
	}
}
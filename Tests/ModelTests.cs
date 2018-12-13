using Microsoft.VisualStudio.TestTools.UnitTesting;
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

		[TestMethod]
		public void InvalidEmpShouldFail()
		{
			EmployeeInt e = new EmployeeInt() { FirstName = "Whoever", HireDate = new DateTime(1960, 1, 1) };
			using (var cn = GetConnection())
			{
				string message;
				Assert.IsTrue(!e.Validate(cn, out message));
				Assert.IsTrue(message.Equals(EmployeeInt.InvalidMessage));
			}
		}

		[TestMethod]
		public void CheckFindPermission()
		{
			using (var cn = GetConnection())
			{
				var e = new EmployeeInt() { FirstName = "Whatever", LastName = "Nobody", HireDate = new DateTime(1980, 1, 1) };
				int empId = cn.Save(e);

				try
				{
					var eFind = cn.Find<EmployeeInt>(empId, new TestUser() { UserName = "adamo" });
				}
				catch (PermissionException exc)
				{
					Assert.IsTrue(exc.Message.Equals("User adamo does not have find permission on a record of EmployeeInt."));
					return;
				}

				Assert.Fail("Find operation should have thrown exception.");
			}
		}

		[TestMethod]
		public void CheckSavePermission()
		{
			using (var cn = GetConnection())
			{
				var e = new EmployeeInt() { FirstName = "Whatever", LastName = "Nobody", HireDate = new DateTime(1980, 1, 1) };

				try
				{
					int empId = cn.Save(e, new TestUser() { UserName = "adamo" });
				}
				catch (PermissionException exc)
				{
					Assert.IsTrue(exc.Message.Equals("User adamo does not have save permission on EmployeeInt."));
					return;
				}

				Assert.Fail("Save operation should have thrown exception.");
			}

		}

		protected IDbConnection GetConnection()
		{
			string connectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;
			return new SqlConnection(connectionString);
		}

		private static IDbConnection GetMasterConnection()
		{
			string masterConnection = ConfigurationManager.ConnectionStrings["SqlServerMaster"].ConnectionString;
			return new SqlConnection(masterConnection);
		}

		[ClassInitialize]
		public static void InitDb(TestContext context)
		{
			try
			{
				using (var cn = GetMasterConnection())
				{
					cn.Execute("CREATE DATABASE [PostulateLite]");
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine("InitDb: " + exc.Message);
				// do nothing, db already exists or something else out of scope is wrong
			}
		}
	}

	public class TestUser : IUser
	{
		public string UserName { get; set; }

		public DateTime LocalTime { get { return DateTime.Now; } }
	}
}
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core;
using Postulate.Lite.Core.Exceptions;
using Postulate.Lite.Core.Interfaces;
using Postulate.Lite.SqlServer;
using Postulate.Lite.SqlServer.IntKey;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Tests.Models;

namespace Tests.SqlServer
{
	[TestClass]
	public partial class TestSqlServer : TestBase
	{
		protected override IDbConnection GetConnection()
		{
			string connectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;
			return new SqlConnection(connectionString);
		}

		protected override CommandProvider<int> GetIntProvider()
		{
			return new SqlServerProvider<int>((obj) => Convert.ToInt32(obj), "identity(1,1)");
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

		[TestMethod]
		public void DropAndCreateEmpTable()
		{
			DropAndCreateEmpTableBase();
		}

		[TestMethod]
		public void DropAndCreateOrgTable()
		{
			DropAndCreateOrgTableBase();
		}

		[TestMethod]
		public void InsertEmployees()
		{
			InsertEmployeesBase();
		}

		[TestMethod]
		public void DeleteEmployee()
		{
			DeleteEmployeeBase();
		}

		[TestMethod]
		public void DeleteEmployeeAsync()
		{
			DeleteEmployeeBaseAsync().Wait();
		}

		[TestMethod]
		public void UpdateEmployee()
		{
			UpdateEmployeeBase();
		}

		[TestMethod]
		public void UpdateEmployeeAsync()
		{
			UpdateEmployeeBaseAsync().Wait();
		}

		[TestMethod]
		public void UpdateEmployeeColumns()
		{
			UpdateEmployeeColumnsBase();
		}

		[TestMethod]
		public void UpdateEmployeeColumnsAsync()
		{
			UpdateEmployeeColumnsBaseAsync().Wait();
		}

		[TestMethod]
		public void SaveEmployee()
		{
			SaveEmployeeBase();
		}

		[TestMethod]
		public void SaveEmployeeAsync()
		{
			SaveEmployeeBaseAsync().Wait();
		}

		[TestMethod]
		public void ForeignKeyLookup()
		{
			ForeignKeyLookupBase();
		}

		[TestMethod]
		public void ForeignKeyLookupAsync()
		{
			ForeignKeyLookupBaseAsync().Wait();
		}

		[TestMethod]
		public void FindWhereEmployee()
		{
			FindWhereEmployeeBase();
		}

		[TestMethod]
		public void FindWhereEmployeeAsync()
		{
			FindWhereEmployeeBaseAsync().Wait();
		}

		[TestMethod]
		public void FindEmployee()
		{
			FindEmployeeBase();
		}

		[TestMethod]
		public void FindEmployeeAsync()
		{
			FindEmployeeBaseAsync().Wait();
		}

		[TestMethod]
		public void MergeOrg()
		{
			MergeOrgBase();
		}

		[TestMethod]
		public void MergeOrgAsync()
		{
			MergeOrgBaseAsync().Wait();
		}

		[TestMethod]
		public void EmployeeQueryLastName()
		{
			EmployeeQueryLastNameBase();
		}

		protected override string GetEmployeeQueryByLastNameSyntax()
		{
			return "SELECT * FROM [Employee] WHERE [LastName] LIKE @lastName";
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

		protected override string CustomTableName => "dbo.HelloOrg";

		[TestMethod]
		public void CreateOrgTableWithCustomName()
		{
			CreateOrgTableWithCustomNameBase();
		}
	}

	public class TestUser : IUser
	{
		public string UserName { get; set; }

		public DateTime LocalTime { get { return DateTime.Now; } }
	}

}
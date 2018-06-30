using AdamOneilSoftware;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core;
using Postulate.Lite.SqlServer;
using Postulate.Lite.SqlServer.IntKey;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Tests.Models;

namespace Tests.SqlServer
{
	[TestClass]
	public class SqlServerCrud : CrudBase
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
			string masterConnection = ConfigurationManager.ConnectionStrings["MasterConnection"].ConnectionString;
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
		public void DropAndCreateTable()
		{
			DropAndCreateTableBase();
		}

		/// <summary>
		/// Drops employee table, creates 10 random employees
		/// </summary>
		[TestMethod]
		public void InsertEmployees()
		{
			InsertEmployeesBase();
		}

		[TestMethod]
		public void FindEmployee()
		{
			InsertEmployees();

			using (var cn = GetConnection())
			{
				var e = cn.Find<EmployeeInt>(5);
				Assert.IsTrue(e.Id == 5);
			}
		}

		[TestMethod]
		public void UpdateEmployee()
		{
			InsertEmployees();

			const string name = "Django";

			using (var cn = GetConnection())
			{
				var e = cn.Find<EmployeeInt>(5);
				e.FirstName = name;
				cn.Save(e);

				e = cn.Find<EmployeeInt>(5);
				Assert.IsTrue(e.FirstName.Equals(name));
			}
		}

		[TestMethod]
		public void DeleteEmployee()
		{
			InsertEmployees();

			using (var cn = GetConnection())
			{
				cn.Delete<EmployeeInt>(5);
				int count = cn.QuerySingle<int>("SELECT COUNT(1) FROM [dbo].[EmployeeInt]");
				Assert.IsTrue(count == 99);
			}
		}

		[TestMethod]
		public void SaveEmployee()
		{
			var e = new EmployeeInt()
			{
				OrganizationId = 1,
				FirstName = "Adam",
				LastName = "O'Neil",
				HireDate = new DateTime(2012, 1, 1)
			};

			using (var cn = GetConnection())
			{
				cn.Save(e);
				cn.Delete<EmployeeInt>(e.Id);
			}
		}

		[TestMethod]
		public void FindWhereEmployee()
		{
			InsertEmployees();

			using (var cn = GetConnection())
			{
				// there has to be an Id = 3 in there, I'm sure
				var e = cn.FindWhere(new EmployeeInt() { Id = 3 });
				Assert.IsTrue(e.Id == 3);
			}
		}

		[TestMethod]
		public void ForeignKeyLookup()
		{
			using (var cn = GetConnection())
			{
				var e = cn.Find<EmployeeInt>(10);
				Assert.IsTrue(e.Organization != null);
			}
		}

		[TestMethod]
		public void DropAndCreateLong()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, "Employee");
				Postulate.Lite.SqlServer.LongKey.ConnectionExtensions.CreateTable<EmployeeLong>(cn);
			}
		}

		[TestMethod]
		public void DropAndCreateGuid()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, "Employee");
				Postulate.Lite.SqlServer.GuidKey.ConnectionExtensions.CreateTable<EmployeeGuid>(cn);
			}
		}
	}
}
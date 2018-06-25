using AdamOneilSoftware;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
	public class Crud
	{
		private static IDbConnection GetConnection()
		{
			string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
			return new SqlConnection(connectionString);
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
		public void DropAndCreateIntTable()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, "Employee");
				cn.CreateTable<Employee>();
			}
		}

		/// <summary>
		/// Drops employee table, creates 10 random employees
		/// </summary>
		[TestMethod]
		public void InsertEmployees()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, "Organization");
				cn.CreateTable<Organization>();

				DropTable(cn, "Employee");
				cn.CreateTable<Employee>();

				var tdg = new TestDataGenerator();
				tdg.Generate<Organization>(5, (record) =>
				{
					record.Name = tdg.Random(Source.UniqueWidget);
				}, (records) =>
				{
					foreach (var record in records) cn.Save(record);
				});

				var orgIds = cn.Query<int>("SELECT [Id] FROM [Organization]").ToArray();

				tdg.Generate<Employee>(100, (record) =>
				{
					record.OrganizationId = tdg.Random(orgIds);
					record.FirstName = tdg.Random(Source.FirstName);
					record.LastName = tdg.Random(Source.LastName);
					record.Email = $"{record.FirstName}.{record.LastName}@nowhere.org";
				}, (records) =>
				{
					foreach (var record in records) cn.Save(record);
				});
			}
		}

		[TestMethod]
		public void FindEmployee()
		{
			InsertEmployees();

			using (var cn = GetConnection())
			{
				var e = cn.Find<Employee>(5);
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
				var e = cn.Find<Employee>(5);
				e.FirstName = name;
				cn.Save(e);

				e = cn.Find<Employee>(5);
				Assert.IsTrue(e.FirstName.Equals(name));
			}
		}

		[TestMethod]
		public void DeleteEmployee()
		{
			InsertEmployees();

			using (var cn = GetConnection())
			{
				cn.Delete<Employee>(5);
				int count = cn.QuerySingle<int>("SELECT COUNT(1) FROM [dbo].[Employee]");
				Assert.IsTrue(count == 99);
			}
		}

		[TestMethod]
		public void SaveEmployee()
		{
			var e = new Employee()
			{
				OrganizationId = 1,
				FirstName = "Adam",
				LastName = "O'Neil",
				HireDate = new DateTime(2012, 1, 1)
			};

			using (var cn = GetConnection())
			{
				cn.Save(e);
				cn.Delete<Employee>(e.Id);
			}
		}

		[TestMethod]
		public void FindWhereEmployee()
		{
			InsertEmployees();

			using (var cn = GetConnection())
			{
				// there has to be an Id = 3 in there, I'm sure
				var e = cn.FindWhere(new Employee() { Id = 3 });
				Assert.IsTrue(e.Id == 3);
			}
		}

		[TestMethod]
		public void ForeignKeyLookup()
		{
			using (var cn = GetConnection())
			{
				var e = cn.Find<Employee>(10);
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

		private void DropTable(IDbConnection cn, string tableName)
		{
			try
			{
				cn.Execute($"DROP TABLE [{tableName}]");
			}
			catch
			{
				// ignore error
			}
		}
	}
}
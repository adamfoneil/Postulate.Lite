using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core;
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
			FindEmployeeBase();
		}

		[TestMethod]
		public void UpdateEmployee()
		{
			UpdateEmployeeBase();
		}

		[TestMethod]
		public void DeleteEmployee()
		{
			DeleteEmployeeBase();
		}

		[TestMethod]
		public void SaveEmployee()
		{
			SaveEmployeeBase();
		}

		[TestMethod]
		public void FindWhereEmployee()
		{
			FindWhereEmployeeBase();
		}

		[TestMethod]
		public void ForeignKeyLookup()
		{
			ForeignKeyLookupBase();
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

		[TestMethod]
		public void EmployeeQueryLastName()
		{
			EmployeeQueryLastNameBase();
		}

		protected override string GetEmployeeQueryByLastNameSyntax()
		{
			return "SELECT * FROM [EmployeeInt] WHERE [LastName] LIKE @lastName";
		}
	}
}
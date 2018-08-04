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
	}
}
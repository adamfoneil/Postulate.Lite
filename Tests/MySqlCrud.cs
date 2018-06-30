using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Postulate.Lite.Core;
using Postulate.Lite.MySql;
using System;
using System.Configuration;
using System.Data;

namespace Tests.MySql
{
	[TestClass]
	public class MySqlCrud : CrudBase
	{
		protected override IDbConnection GetConnection()
		{
			string connectionStr = ConfigurationManager.ConnectionStrings["MySql"].ConnectionString;
			return new MySqlConnection(connectionStr);
		}

		protected override CommandProvider<int> GetIntProvider()
		{
			return new MySqlProvider<int>((obj) => Convert.ToInt32(obj), "auto_increment");
		}

		private static IDbConnection GetMasterConnection()
		{
			string masterConnection = ConfigurationManager.ConnectionStrings["MySqlMaster"].ConnectionString;
			return new MySqlConnection(masterConnection);
		}

		[TestInitialize]
		public void InitDb()
		{
			try
			{
				using (var cn = GetMasterConnection())
				{
					cn.Execute("CREATE SCHEMA `PostulateLite`");
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
		public void UpdateEmployee()
		{
			UpdateEmployeeBase();
		}

		[TestMethod]
		public void SaveEmployee()
		{
			SaveEmployeeBase();
		}

		[TestMethod]
		public void ForeignKeyLookup()
		{
			ForeignKeyLookupBase();
		}

		[TestMethod]
		public void FindWhereEmployee()
		{
			FindWhereEmployeeBase();
		}

		[TestMethod]
		public void FindEmployee()
		{
			FindEmployeeBase();
		}

		[TestMethod]
		public void EmployeeQueryLastName()
		{
			EmployeeQueryLastNameBase();
		}

		protected override string GetEmployeeQueryByLastNameSyntax()
		{
			return "SELECT * FROM `EmployeeInt` WHERE `LastName` LIKE @lastName";
		}
	}
}
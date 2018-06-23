using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using AdamOneilSoftware;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.SqlServer;
using Tests.Models;

namespace Tests.SqlServer
{
	[TestClass]
	public class Crud
	{
		private IDbConnection GetConnection()
		{
			string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
			return new SqlConnection(connectionString);
		}

		[TestMethod]
		public void DropAndCreateTable()
		{			
			using (var cn = GetConnection())
			{
				DropTable(cn, "Employee");
				cn.CreateTable<Employee>();
			}
		}

		[TestMethod]
		public void InsertEmployees()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, "Employee");
				cn.CreateTable<Employee>();

				var tdg = new TestDataGenerator();
				tdg.Generate<Employee>(10, (record) =>
				{
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

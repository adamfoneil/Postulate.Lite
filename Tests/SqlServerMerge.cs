using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core;
using Postulate.Lite.SqlServer;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Tests
{
	[TestClass]
	public class SqlServerMerge : MergeBase
	{
		protected override CommandProvider<int> GetIntProvider()
		{
			return new SqlServerProvider<int>((obj) => Convert.ToInt32(obj), "identity(1,1)");
		}

		protected override IDbConnection GetConnection()
		{
			string connectionString = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;
			return new SqlConnection(connectionString);
		}

		[TestMethod]
		public void CreateTwoTables()
		{
			CreateTwoTablesBase();
		}
	}
}
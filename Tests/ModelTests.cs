using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.SqlServer;
using System.Linq;
using Tests.Models;

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
	}
}
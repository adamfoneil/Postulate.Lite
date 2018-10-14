using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.SqlServer
{
	public partial class TestSqlServer : TestBase
	{
		[TestMethod]
		public void TrackItemChanges()
		{			
			TrackItemChangesBase();
		}
	}
}
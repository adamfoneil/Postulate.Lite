using AdamOneilSoftware;
using System;
using Tests.Models;

namespace Tests
{
	public abstract partial class TestBase
	{
		protected void TrackItemChangesBase()
		{
			InitItemTables();			

			var item = new Item()
			{
				Name = "Whatever",
				Description = "This new thing that is",
				TypeId = 1,
				IsMade = true,
				Cost = 10,
				EffectiveDate = new DateTime(1990, 1, 1)
			};

			using (var cn = GetConnection())
			{
				GetIntProvider().Save(cn, item);
				item.Cost = 12;
				item.TypeId = 2;
				GetIntProvider().Save(cn, item);
			}
		}

		private void InitItemTables()
		{
			using (var cn = GetConnection())
			{
				DropTable(cn, "Item");
				DropTable(cn, "ItemType");
				GetIntProvider().CreateTable<ItemType>(cn);
				GetIntProvider().CreateTable<Item>(cn);

				var tdg = new TestDataGenerator();
				tdg.Generate<ItemType>(10, (it) =>
				{
					it.Name = tdg.Random(Source.WidgetName) + tdg.RandomInRange(100, 999).ToString();
				}, (records) =>
				{
					foreach (var record in records) GetIntProvider().Save(cn, record);
				});
			}
		}
	}
}
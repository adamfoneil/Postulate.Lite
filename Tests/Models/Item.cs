using Postulate.Lite.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Models
{
	[Identity(nameof(Id))]
	[TrackChanges]
	public class Item
	{
		public int Id { get; set; }

		[PrimaryKey]
		[MaxLength(100)]
		public string Name { get; set; }

		[References(typeof(ItemType))]
		public int TypeId { get; set; }

		[MaxLength(255)]
		public string Description { get; set; }

		/// <summary>
		/// Make or buy indicator
		/// </summary>
		public bool IsMade { get; set; }

		[Column(TypeName = "money")]
		public decimal Cost { get; set; }

		public DateTime? EffectiveDate { get; set; }
	}

	[Identity(nameof(Id))]
	[DereferenceId("SELECT [Name] FROM [dbo].[ItemType] WHERE [Id]=@id")]
	public class ItemType
	{
		public int Id { get; set; }

		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }
	}
}
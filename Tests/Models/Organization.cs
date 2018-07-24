using Postulate.Lite.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Tests.Models
{
	[Identity(nameof(Id))]
	public class Organization
	{
		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		public int? EmployeeCount { get; set; }

		public int Id { get; set; }
	}
}
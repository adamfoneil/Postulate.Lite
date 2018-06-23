using Postulate.Lite.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Models
{
	[Identity(nameof(Id))]
	public class Employee
	{
		[MaxLength(50)]
		[Required]
		public string FirstName { get; set; }

		[MaxLength(50)]
		[Required]
		public string LastName { get; set; }

		[MaxLength(50)]
		public string Email { get; set; }

		[Column(TypeName = "date")]
		public DateTime? HireDate { get; set; }

		public bool IsActive { get; set; } = true;

		public int Id { get; set; }
	}
}
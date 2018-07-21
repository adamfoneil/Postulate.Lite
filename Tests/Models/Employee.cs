using Postulate.Lite.Core;
using Postulate.Lite.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Tests.Models
{
	[Identity(nameof(Id))]
	[Table("Employee")]
	public class EmployeeInt : Record
	{
		[References(typeof(Organization))]
		public int OrganizationId { get; set; }

		public Organization Organization { get; set; }

		[MaxLength(50)]
		[Required]
		public string FirstName { get; set; }

		[MaxLength(50)]
		[Required]
		public string LastName { get; set; }

		[MaxLength(50)]
		public string Email { get; set; }

		public string Notes { get; set; }

		[Column(TypeName = "date")]
		public DateTime? HireDate { get; set; }

		public bool IsActive { get; set; } = true;

		public int Id { get; set; }

		public override void LookupIntForeignKeys(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			Organization = commandProvider.Find<Organization>(connection, OrganizationId);
		}
	}

	[Identity(nameof(Id))]
	[Table("Employee")]
	public class EmployeeLong : Record
	{
		[References(typeof(Organization))]
		public int OrganizationId { get; set; }

		public Organization Organization { get; set; }

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

		public long Id { get; set; }

		public override void LookupLongForeignKeys(IDbConnection connection, CommandProvider<long> commandProvider)
		{
			Organization = commandProvider.Find<Organization>(connection, OrganizationId);
		}
	}

	[Identity(nameof(Id))]
	[Table("Employee")]
	public class EmployeeGuid : Record
	{
		[References(typeof(Organization))]
		public int OrganizationId { get; set; }

		public Organization Organization { get; set; }

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

		public Guid Id { get; set; }
	}
}
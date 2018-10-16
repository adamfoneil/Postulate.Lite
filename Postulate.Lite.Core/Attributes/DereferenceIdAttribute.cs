using System;

namespace Postulate.Lite.Core.Attributes
{
	/// <summary>
	/// Defines the query to execute to lookup the name that maps to a foreign key value during <see cref="CommandProvider{TKey}.GetChanges{TModel}(System.Data.IDbConnection, TModel)"/>
	/// Query must accept an @id parameter that represents the FK value, and return a <see cref="Models.IdLookup"/> model
	/// Example: SELECT [Name] FROM [dbo].[Whatever] WHERE [Id]=@id
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DereferenceIdAttribute : Attribute
	{
		public DereferenceIdAttribute(string query)
		{
			Query = query;
		}

		public DereferenceIdAttribute(Type connectionType, string query)
		{
			Query = query;
			ConnectionType = connectionType;
		}

		public string Query { get; }
		public Type ConnectionType { get; }
	}
}
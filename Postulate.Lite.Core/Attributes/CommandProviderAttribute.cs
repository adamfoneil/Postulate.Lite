using System;

namespace Postulate.Lite.Core
{
	public enum SupportedDatabases
	{
		SqlServer,
		MySql
	}

	namespace Attributes
	{
		/// <summary>
		/// Specifies the type of Postulate.Lite.Core.CommandProvider will be used with model merges.
		/// Required by Postulate Merge UI
		/// </summary>
		[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
		public class CommandProviderAttribute : Attribute
		{
			public CommandProviderAttribute(SupportedDatabases databaseType)
			{
				DatabaseType = databaseType;
			}

			public SupportedDatabases DatabaseType { get; private set; }
		}
	}
}
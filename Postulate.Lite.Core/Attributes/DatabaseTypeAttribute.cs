using System;

namespace Postulate.Lite.Core
{
	public enum Databases
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
		public class DatabaseTypeAttribute : Attribute
		{
			public DatabaseTypeAttribute(Databases databaseType)
			{
				DatabaseType = databaseType;
			}

			public Databases DatabaseType { get; private set; }
		}
	}
}
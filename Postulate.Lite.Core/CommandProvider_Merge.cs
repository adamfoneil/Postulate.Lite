using System;
using System.Reflection;

namespace Postulate.Lite.Core
{
	public abstract partial class CommandProvider<TKey>
	{
		/// <summary>
		/// Generates a SQL create table statement for a given model class
		/// </summary>
		/// <typeparam name="T">Model class type</typeparam>
		protected abstract string CreateTableCommand(Type modelType);

		protected abstract string CreateForeignKeyCommand(PropertyInfo propertyInfo);
	}
}
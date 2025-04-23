using System;
using System.Collections.Generic;

namespace CodeGenTool.Services.Mapper
{
	public static class TypeMapperManager
	{
		private static readonly Dictionary<TargetType, ITypeMapper> _mappers = new()
		{
			{ TargetType.DotNet, new MySqlToDotNetTypeMapper() },
			{ TargetType.Postgres, new MySqlToPostgresTypeMapper() }
		};

		public static string Map(string srcType, TargetType target)
		{
			return _mappers.TryGetValue(target, out var mapper)
				? mapper.Map(srcType)
				: throw new NotSupportedException($"Target type {target} is not supported.");
		}
	}
}
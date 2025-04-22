namespace CodeGenTool.Services.Mapper
{
	public partial class TypeMapper
	{
		public static string MapToDotNetType(string dbType)
		{
			dbType = dbType.ToLower();

			if (dbType.Contains("int"))
				return "int";
			if (dbType.Contains("varchar") || dbType.Contains("text") || dbType.Contains("char"))
				return "string";
			if (dbType.Contains("decimal") || dbType.Contains("numeric"))
				return "decimal";
			if (dbType.Contains("double") || dbType.Contains("float"))
				return "double";
			if (dbType.Contains("date") || dbType.Contains("time"))
				return "DateTime";
			if (dbType.Contains("bool") || dbType.Contains("bit"))
				return "bool";

			return "string";
		}
	}
}

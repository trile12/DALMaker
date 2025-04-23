namespace CodeGenTool.Services.Mapper
{
	public interface ITypeMapper
	{
		string Map(string type);
	}

	public enum TargetType
	{
		DotNet,
		Postgres
	}
}
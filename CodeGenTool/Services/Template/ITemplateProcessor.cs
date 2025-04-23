namespace CodeGenTool.Services.Template
{
	public interface ITemplateProcessor
	{
		string ClassName { get; }

		string ProcessTemplate(string template);
	}
}
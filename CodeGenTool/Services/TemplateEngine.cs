using CodeGenTool.Models;
using CodeGenTool.Services.Template;
using System;

namespace CodeGenTool.Services
{
	public static class TemplateEngine
	{
		public static (string result, string className) Render(string template, TableInfo table, string templateType)
		{
			ITemplateProcessor processor = templateType switch
			{
				"csharp_dto.cs.template" => new CSharpDtoTemplateProcessor(table),
				_ => throw new NotSupportedException($"Template type '{templateType}' is not supported.")
			};

			string output = processor.ProcessTemplate(template);
			return (output, processor.ClassName);
		}
	}
}
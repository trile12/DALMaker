using CodeGenTool.Models;
using CodeGenTool.Services.Template;
using System;
using System.Diagnostics;

namespace CodeGenTool.Services
{
	public static class TemplateEngine
	{
		public static (string result, string className) Render(string template, TemplateInput inputTemplate)
		{
			//ITemplateProcessor processor = inputTemplate.TemplateFileName switch
			//{
			//	"csharp_dto.cs.template" => new TemplateProcessor(inputTemplate),
			//	_ => throw new NotSupportedException($"Template type '{inputTemplate.TemplateFileName}' is not supported.")
			//};

			var processor = new TemplateProcessor(inputTemplate);

			string output = processor.ProcessTemplate(template);
			return (output, processor.ClassName);
		}
	}

	public class TemplateInput
	{
		public string TemplateFileName { get; set; }
		public string Namespace { get; set; }
		public TableInfo TableInfo { get; set; }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeGenTool.Services.Template
{
	public class TemplateProcessor : ITemplateProcessor
	{
		private readonly TemplateInput _inputTemplate;
		private readonly StringBuilder _result;
		private readonly Dictionary<string, string> _replacements;
		private readonly ITemplateTokenResolver _tokenResolver;

		public string ClassName { get; private set; }

		public TemplateProcessor(TemplateInput inputTemplate)
		{
			_inputTemplate = inputTemplate;
			_result = new StringBuilder();
			_replacements = new Dictionary<string, string>();
			_tokenResolver = new TemplateTokenResolver();

			ClassName = _tokenResolver.Resolve("{{dbTable2netUpper}}", _inputTemplate);
		}

		public string ProcessTemplate(string template)
		{
			template = ApplyBasicReplacements(template);

			var lines = template.Split(Environment.NewLine);
			bool insideLoop = false;
			var loopLines = new List<string>();

			foreach (var line in lines)
			{
				if (line.Contains("@foreach"))
				{
					insideLoop = true;
					loopLines.Clear();
					continue;
				}

				if (line.Contains("@end"))
				{
					insideLoop = false;
					ProcessColumnLoop(loopLines);
					continue;
				}

				if (insideLoop)
				{
					loopLines.Add(line);
					continue;
				}
				else
				{
					_result.AppendLine(line);
				}
			}

			return _result.ToString();
		}

		private string ApplyBasicReplacements(string templateLine)
		{
			return Regex.Replace(templateLine, "{{[^}]+}}", match =>
			 {
				 var token = match.Value;
				 return _tokenResolver.Resolve(token, _inputTemplate);
			 });
		}

		private void ProcessColumnLoop(List<string> loopLines)
		{
			var selectedColumns = _inputTemplate.TableInfo.Columns.Where(c => c.IsSelected).ToList();
			for (int i = 0; i < selectedColumns.Count; i++)
			{
				var column = selectedColumns[i];
				bool isLast = i == selectedColumns.Count - 1;

				foreach (var templateLine in loopLines)
				{
					string processedLine = Regex.Replace(templateLine, "{{[^}]+}}", match =>
					{
						var token = match.Value;
						return _tokenResolver.Resolve(token, column, isLast);
					});

					_result.AppendLine(processedLine);
				}
			}
		}

	}
}
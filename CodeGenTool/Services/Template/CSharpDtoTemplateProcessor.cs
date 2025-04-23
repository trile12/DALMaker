using CodeGenTool.Models;
using CodeGenTool.Services.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeGenTool.Services.Template
{
	public class CSharpDtoTemplateProcessor : ITemplateProcessor
	{
		private readonly TableInfo _table;
		private readonly StringBuilder _result;
		private readonly Dictionary<string, string> _replacements;

		public string ClassName { get; private set; }

		public CSharpDtoTemplateProcessor(TableInfo table)
		{
			_table = table;
			_result = new StringBuilder();
			_replacements = new Dictionary<string, string>();

			InitializeBasicReplacements();
		}

		private void InitializeBasicReplacements()
		{
			ClassName = FormatToPascalCase(_table.Name);
			string tableCamelCase = FormatToCamelCase(_table.Name);

			_replacements.Add("@dbTable2netUpper", ClassName);
			_replacements.Add("@dbTable2net", tableCamelCase);
			_replacements.Add("@dbTableName", _table.Name);
		}

		public string ProcessTemplate(string template)
		{
			template = ApplyBasicReplacements(template);

			var lines = template.Split(Environment.NewLine);
			bool insideLoop = false;

			foreach (var line in lines)
			{
				if (line.Contains("@foreach"))
				{
					insideLoop = true;
					continue;
				}

				if (insideLoop)
				{
					insideLoop = false;
					ProcessColumnLoop(line);
					continue;
				}
				else
				{
					_result.AppendLine(line);
				}
			}

			return _result.ToString();
		}

		private string ApplyBasicReplacements(string template)
		{
			foreach (var replacement in _replacements)
			{
				template = template.Replace(replacement.Key, replacement.Value);
			}
			return template;
		}

		private void ProcessColumnLoop(string templateLine)
		{
			for (int i = 0; i < _table.Columns.Count; i++)
			{
				var column = _table.Columns[i];
				if (!column.IsSelected) continue;

				bool isLast = (i == _table.Columns.Count - 1);
				string processedLine = templateLine
					.Replace("@dbColmnName2netUpper", FormatToPascalCase(column.Name))
					.Replace("@dbType2net", TypeMapperManager.Map(column.DataType, TargetType.DotNet));

				if (isLast && processedLine.TrimEnd().EndsWith(","))
				{
					processedLine = processedLine.TrimEnd().TrimEnd(',');
				}

				_result.AppendLine(processedLine);
			}
		}

		private string FormatToPascalCase(string input)
		{
			if (string.IsNullOrEmpty(input)) return input;

			if (input.Contains('_') || input.Contains('-'))
			{
				var words = Regex.Split(input, "[_-]")
					.Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower());
				return string.Join("", words);
			}

			return char.ToUpper(input[0]) + input.Substring(1);
		}

		private string FormatToCamelCase(string input)
		{
			string pascal = FormatToPascalCase(input);
			return char.ToLower(pascal[0]) + pascal.Substring(1);
		}
	}
}
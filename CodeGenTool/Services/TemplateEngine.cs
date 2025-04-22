using CodeGenTool.Models;
using CodeGenTool.Services.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenTool.Services
{
	public static class TemplateEngine
	{
		public static string Render(string template, TableInfo table)
		{
			string className = table.Name;
			className = char.ToUpper(className[0]) + className[1..];

			template = template.Replace("@dbTable2netUpper", className);

			var result = new StringBuilder();
			var lines = template.Split(Environment.NewLine);

			bool insideLoop = false;
			var loopTemplate = new List<string>();

			foreach (var line in lines)
			{
				if (line.Contains("@foreach"))
				{
					insideLoop = true;
					continue;
				}

				if (insideLoop)
				{
					if (line.Contains("dbType2net"))
					{
						insideLoop = false;

						foreach (var col in table.Columns)
						{
							string colType = TypeMapper.MapToDotNetType(col.DataType);
							string colName = char.ToUpper(col.Name[0]) + col.Name[1..];

							result.AppendLine(line
								.Replace("@dbType2net", colType)
								.Replace("@dbColmnName2netUpper", colName));
						}

						loopTemplate.Clear();
						continue;
					}

					loopTemplate.Add(line);
				}
				else
				{
					result.AppendLine(line);
				}
			}

			return result.ToString();
		}
	}
}

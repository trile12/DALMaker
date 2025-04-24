using CodeGenTool.Models;
using CodeGenTool.Services.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGenTool.Services.Template
{
	internal class TemplateTokenResolver : ITemplateTokenResolver
	{
		public TemplateTokenResolver()
		{
		}

		public string Resolve(string token, ColumnInfo column, bool isLast)
		{
			return token switch
			{
				"{{db_column}}" => column.Name,
				"{{db_type2PosgreSQL}}" => TypeMapperManager.Map(column.DataType, TargetType.Postgres),
				"{{db_column2netUpper}}" => FormatToUpperCase(column.Name),
				"{{db_type2net}}" => TypeMapperManager.Map(column.DataType, TargetType.DotNet),
				"{{@islast:,:}}" => isLast ? "" : ",",
				_ => token
			};
		}

		public string Resolve(string token, TemplateInput inputTemplate)
		{
			return token switch
			{

				"{{namespace}}" => inputTemplate.Namespace,
				"{{dbTableName}}" => FormatToLowerCase(inputTemplate.TableInfo.Name),
				"{{dbTable2netUpper}}" => FormatToUpperCase(inputTemplate.TableInfo.Name),
				_ => token
			};
		}

		private string FormatToUpperCase(string input)
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

		private string FormatToLowerCase(string input)
		{
			string pascal = FormatToUpperCase(input);
			return char.ToLower(pascal[0]) + pascal.Substring(1);
		}
	}
}

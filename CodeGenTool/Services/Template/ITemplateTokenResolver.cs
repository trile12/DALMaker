using CodeGenTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenTool.Services.Template
{
	public interface ITemplateTokenResolver
	{
		string Resolve(string token, ColumnInfo column, bool isLast);

		string Resolve(string token, TemplateInput inputTemplate);
	}
}

﻿namespace CodeGenTool.Models
{
	public class ColumnInfo
	{
		public string Name { get; set; }
		public string DataType { get; set; }
		public bool IsPrimaryKey { get; set; }
		public bool IsNullable { get; set; }
	}
}
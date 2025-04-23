namespace CodeGenTool.Services.Mapper
{
	public static class TypeMapper
	{
		// Map database type to MySQL type
		public static string MapToMySqlType(string dbType)
		{
			dbType = dbType.ToLowerInvariant();

			return dbType switch
			{
				"int" => "INT",
				"bigint" => "BIGINT",
				"smallint" => "SMALLINT",
				"tinyint" => "TINYINT",
				"bit" => "TINYINT(1)",
				"decimal" => "DECIMAL",
				"numeric" => "DECIMAL",
				"money" => "DECIMAL(19,4)",
				"float" => "FLOAT",
				"real" => "DOUBLE",
				"datetime" => "DATETIME",
				"datetime2" => "DATETIME",
				"date" => "DATE",
				"time" => "TIME",
				"char" => "CHAR",
				"nchar" => "CHAR",
				"varchar" => "VARCHAR",
				"nvarchar" => "VARCHAR",
				"text" => "TEXT",
				"ntext" => "TEXT",
				"binary" => "BINARY",
				"varbinary" => "VARBINARY",
				"image" => "LONGBLOB",
				"uniqueidentifier" => "CHAR(36)",
				// Default
				_ => "VARCHAR(255)"
			};
		}

		// Map database type to MS SQL Server type
		public static string MapToMsSqlType(string dbType)
		{
			dbType = dbType.ToLowerInvariant();

			return dbType switch
			{
				"int" => "INT",
				"bigint" => "BIGINT",
				"smallint" => "SMALLINT",
				"tinyint" => "TINYINT",
				"bit" => "BIT",
				"decimal" => "DECIMAL",
				"numeric" => "NUMERIC",
				"money" => "MONEY",
				"float" => "FLOAT",
				"real" => "REAL",
				"datetime" => "DATETIME",
				"datetime2" => "DATETIME2",
				"date" => "DATE",
				"time" => "TIME",
				"char" => "CHAR",
				"nchar" => "NCHAR",
				"varchar" => "VARCHAR",
				"nvarchar" => "NVARCHAR",
				"text" => "TEXT",
				"ntext" => "NTEXT",
				"binary" => "BINARY",
				"varbinary" => "VARBINARY",
				"image" => "IMAGE",
				"uniqueidentifier" => "UNIQUEIDENTIFIER",
				// Default
				_ => "NVARCHAR(255)"
			};
		}
	}

	public class MySqlToDotNetTypeMapper : ITypeMapper
	{
		public string Map(string mysqlType)
		{
			var type = mysqlType.ToLower().Split('(')[0].Trim();

			return type switch
			{
				"tinyint(1)" => "bool",
				"tinyint" => "byte",
				"smallint" => "short",
				"mediumint" => "int",
				"int" or "integer" => "int",
				"bigint" => "long",
				"float" => "float",
				"double" => "double",
				"decimal" or "dec" => "decimal",
				"char" or "varchar" or "text" or "tinytext" or "mediumtext" or "longtext" => "string",
				"date" or "datetime" or "timestamp" => "DateTime",
				"time" => "TimeSpan",
				"bit" or "bool" or "boolean" => "bool",
				"blob" or "binary" or "varbinary" or "tinyblob" or "mediumblob" or "longblob" => "byte[]",
				"enum" => "string",
				"json" => "string",
				_ => "object"
			};
		}
	}

	public class MySqlToPostgresTypeMapper : ITypeMapper
	{
		public string Map(string mysqlType)
		{
			var type = mysqlType.ToLower().Split('(')[0].Trim();

			return type switch
			{
				"tinyint(1)" => "BOOLEAN",
				"tinyint" => "SMALLINT",
				"smallint" => "SMALLINT",
				"mediumint" => "INTEGER",
				"int" or "integer" => "INTEGER",
				"bigint" => "BIGINT",
				"float" => "REAL",
				"double" => "DOUBLE PRECISION",
				"decimal" or "dec" => "NUMERIC",
				"char" or "varchar" => "VARCHAR",
				"text" or "tinytext" or "mediumtext" or "longtext" => "TEXT",
				"date" => "DATE",
				"datetime" or "timestamp" => "TIMESTAMP",
				"time" => "TIME",
				"blob" or "binary" or "varbinary" or "tinyblob" or "mediumblob" or "longblob" => "BYTEA",
				"enum" => "VARCHAR",
				"json" => "JSONB",
				_ => "VARCHAR(255)"
			};
		}
	}
}
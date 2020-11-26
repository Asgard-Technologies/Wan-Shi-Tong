using NPoco;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSC.Providers.Postgres
{
    public class ColumnViewDBModel
    {
        public const string QUERY = @"select 
	table_schema 
	,table_name 
	,column_name 
	,udt_name as data_type 
from information_schema.""columns"" t 
where table_schema = 'public'";

        [Column("table_schema")]
        public string TableSchema { get; set; }

        [Column("table_name")]
        public string TableName { get; set; }

        [Column("column_name")]
        public string ColumnName { get; set; }

        [Column("data_type")]
        public string DataType { get; set; }
    }
}

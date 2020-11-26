using System;
using System.Collections.Generic;
using System.Text;

namespace SSC.SQLObjects
{
    public class Table
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public IEnumerable<Column> Columns { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SSC.SQLObjects
{
    public class DatabaseConfig
    {
        public string Version { get; set; } = "0.1";
        public IEnumerable<Table> Tables { get; set; }

        public IEnumerable<string> TrackedSchemas { get; set; } = new List<string>() { "public" };
    }
}

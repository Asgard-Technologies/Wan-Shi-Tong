using SSC.Abstractions;
using SSC.SQLObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSC.Providers.Postgres
{
    class PostgresDatabaseTargetImpl : PostgresDatabaseProvider, IDBConfigTarget
    {
        public PostgresDatabaseTargetImpl(string dbString) : base(dbString)
        {
        }

        public Task<bool> TryApplyChanges(DatabaseConfig databaseConfig)
        {
            throw new NotImplementedException();
        }
    }
}

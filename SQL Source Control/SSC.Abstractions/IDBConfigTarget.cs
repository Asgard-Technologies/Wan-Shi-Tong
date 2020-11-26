using SSC.SQLObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSC.Abstractions
{
    public interface IDBConfigTarget : IDBConfigProvider
    {
        Task<Boolean> TryApplyChanges(DatabaseConfig databaseConfig);
    }
}

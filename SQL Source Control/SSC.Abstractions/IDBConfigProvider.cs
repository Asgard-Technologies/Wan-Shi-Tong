using SSC.SQLObjects;
using System;
using System.Threading.Tasks;

namespace SSC.Abstractions
{
    public interface IDBConfigProvider
    {
        Task<DatabaseConfig> GetDatabaseConfig();
    }
}

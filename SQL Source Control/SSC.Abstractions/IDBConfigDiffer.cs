using SSC.SQLObjects;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SSC.Abstractions
{
    public interface IDBConfigDiffer
    {
        Task<Stream> DiffConfigs(DatabaseConfig source, DatabaseConfig target);
    }
}

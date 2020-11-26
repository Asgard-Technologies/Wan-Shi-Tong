using SSC.Abstractions;
using SSC.SQLObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SSC.Providers.JSONFile
{
    public class FileTarget : FileProvider, IDBConfigTarget
    {
        public FileTarget(string path) : base(path)
        {

        }

        public async Task<bool> TryApplyChanges(DatabaseConfig databaseConfig)
        {
            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new JsonStringEnumConverter());
            serializerOptions.WriteIndented = true;

            var serializedJSON = JsonSerializer.Serialize(databaseConfig, serializerOptions);

            Console.WriteLine(serializedJSON);

            await File.WriteAllTextAsync(this.Path, serializedJSON);

            return true;
        }
    }
}

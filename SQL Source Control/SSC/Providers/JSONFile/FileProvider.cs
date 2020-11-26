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
    public class FileProvider : IDBConfigProvider
    {
        public string Path { get; set; }

        public FileProvider(string path)
        {
            this.Path = path;
        }

        public async Task<DatabaseConfig> GetDatabaseConfig()
        {
            using (FileStream fs = File.OpenRead(this.Path))
            {
                var serializerOptions = new JsonSerializerOptions();
                serializerOptions.Converters.Add(new JsonStringEnumConverter());
                serializerOptions.WriteIndented = true;

                var config = await JsonSerializer.DeserializeAsync<DatabaseConfig>(fs, serializerOptions);
                return config;
            }
        }
    }
}

using NPoco;
using SSC.Abstractions;
using SSC.SQLObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSC.Providers.Postgres
{
    public class PostgresDatabaseProvider : IDBConfigProvider
    {
        public string DBConfigString { get; set; }

        public PostgresDatabaseProvider(string dbString)
        {
            this.DBConfigString = dbString;
        }

        async Task<DatabaseConfig> IDBConfigProvider.GetDatabaseConfig()
        {
            using (var db = new NPoco.Database(this.DBConfigString, DatabaseType.PostgreSQL, Npgsql.NpgsqlFactory.Instance))
            {
                var newTables = new List<SQLObjects.Table>();
                var dbConfig = new DatabaseConfig()
                {
                    Tables = newTables,
                };

                try
                {
                    var rowRowData = await db.FetchAsync<ColumnViewDBModel>(ColumnViewDBModel.QUERY);

                    //Console.WriteLine($"Found {rowRowData.Count()} columns");
                    foreach (var row in rowRowData)
                    {
                        //Console.WriteLine($"Found column: {row.ColumnName}");
                    }

                    var tables = rowRowData.GroupBy(col => $"{col.TableSchema}.{col.TableName}");

                    foreach (var tableColumnGrouping in tables)
                    {
                        var table = new SQLObjects.Table()
                        {
                            SchemaName = tableColumnGrouping.First().TableSchema,
                            TableName = tableColumnGrouping.First().TableName,
                            Columns = tableColumnGrouping.ToList().Select(rawCol =>
                            {
                                ColumnType type = rawCol.DataType switch
                                {
                                    "varchar" => ColumnType.STRING,
                                    "uuid" => ColumnType.UUID,
                                    "timestamp" => ColumnType.TIMESTAMP,
                                    "date" => ColumnType.DATE,
                                    _ => ColumnType.OTHER,
                                };

                                return new SQLObjects.Column()
                                {
                                    ColumnName = rawCol.ColumnName,
                                    Type = type
                                };
                            }),
                        };

                        newTables.Add(table);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("opps");
                }


                return dbConfig;
            }
        }
    }
}

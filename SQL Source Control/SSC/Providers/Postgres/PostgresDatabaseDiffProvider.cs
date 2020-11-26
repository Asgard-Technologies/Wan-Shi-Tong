using SSC.Abstractions;
using SSC.SQLObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSC.Providers.Postgres
{
    class PostgresDatabaseDiffProvider : IDBConfigDiffer
    {
        public async Task<Stream> DiffConfigs(DatabaseConfig source, DatabaseConfig target)
        {
            var tablesToDelete = new List<Table>();
            var tablesToAdd = new List<Table>();
            // Item1: Source Table, Item2: Target Table
            var tablesToModify = new List<Tuple<Table, Table>>();
            var tablesUpToDate = new List<Table>();

            foreach (var table in target.Tables)
            {
                var matchingSourceTable = source.Tables.FirstOrDefault(source => source.SchemaName == table.SchemaName && source.TableName == table.TableName);

                if (matchingSourceTable == null)
                {
                    tablesToDelete.Add(table);
                }
                else
                {
                    var columnMismatches = table.Columns.Select(col =>
                    {
                        var previousColumn = matchingSourceTable.Columns.FirstOrDefault(sourceCol => sourceCol.ColumnName == col.ColumnName);

                        return previousColumn == null || previousColumn.Type != col.Type;
                    });

                    var columnsToDelete = matchingSourceTable.Columns.Select(col =>
                    {
                        var previousColumn = table.Columns.FirstOrDefault(sourceCol => sourceCol.ColumnName == col.ColumnName);

                        return previousColumn == null;
                    });

                    if (columnMismatches.Any(col => col) || columnsToDelete.Any(col => col))
                    {
                        tablesToModify.Add(Tuple.Create(matchingSourceTable, table));
                    }
                    else
                    {
                        tablesUpToDate.Add(table);
                    }
                }
            }

            // Check for tables that we can delete
            foreach (var table in source.Tables)
            {
                var matchingTargetTable = target.Tables.FirstOrDefault(source => source.SchemaName == table.SchemaName && source.TableName == table.TableName);

                if (matchingTargetTable == null)
                {
                    tablesToAdd.Add(table);
                }
            }

            // We're not "using" this because we're returning it.
            var memoryStream = new MemoryStream();

            var writer = new StreamWriter(memoryStream);

            if (!tablesToDelete.Any() && !tablesToAdd.Any() && !tablesToModify.Any())
            {
                await writer.WriteLineAsync("-- All tables are up to date, and require no modifications");
                await writer.FlushAsync();
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            //{
                await writer.WriteLineAsync("-- The Following Tables are Up to Date, and do not require modification:");
                foreach (var table in tablesUpToDate)
                {
                    await writer.WriteLineAsync($"--     - \"{table.SchemaName}\".\"{table.TableName}\"");
                }
                await writer.WriteLineAsync($"");
                await writer.WriteLineAsync($"");

                
                await writer.WriteLineAsync("BEGIN TRANSACTION");


            if (tablesToDelete.Any())
            {
                await writer.WriteLineAsync("-- Delete unused tables:");
                foreach (var table in tablesToDelete)
                {
                    await writer.WriteLineAsync($"DROP TABLE \"{table.SchemaName}\".\"{table.TableName}\";");
                }
            }
            else
            {
                await writer.WriteLineAsync("-- No tables need deleting");
            }
                await writer.WriteLineAsync($"");
                await writer.WriteLineAsync($"");

                await writer.WriteLineAsync("-- Add missing tables:");
                foreach (var table in tablesToAdd)
                {
                    await writer.WriteLineAsync($"CREATE TABLE \"{table.SchemaName}\".\"{table.TableName}\" {{");

                    foreach (var col in table.Columns)
                    {
                        var columnTypeSQL = col.Type switch
                        {
                            ColumnType.BOOLEAN => "bool",
                            ColumnType.TIMESTAMP => "timestamp",
                            ColumnType.UUID => "uuid",
                            ColumnType.DATE => "date",
                            ColumnType.DECIMAL => "decimal",
                            ColumnType.STRING => "varchar",
                            _ => "text",
                        };

                        await writer.WriteLineAsync($"    \"{col.ColumnName}\" {columnTypeSQL};");
                    }

                    await writer.WriteLineAsync("}");
                }
                await writer.WriteLineAsync($"");
                await writer.WriteLineAsync($"");


                await writer.WriteLineAsync("-- Modify existing tables:");
                foreach (var tableTuple in tablesToModify)
                {
                    var sourceTable = tableTuple.Item1;
                    var table = tableTuple.Item2;

                    await writer.WriteLineAsync($"-- \"{table.SchemaName}\".\"{table.TableName}\":");

                    foreach (var col in sourceTable.Columns)
                    {
                        var columnTypeSQL = col.Type switch
                        {
                            ColumnType.BOOLEAN => "bool",
                            ColumnType.TIMESTAMP => "timestamp",
                            ColumnType.UUID => "uuid",
                            ColumnType.DATE => "date",
                            ColumnType.DECIMAL => "decimal",
                            ColumnType.STRING => "varchar",
                            _ => "text",
                        };

                        var previousColumn = table.Columns.FirstOrDefault(sourceCol => sourceCol.ColumnName == col.ColumnName);

                        if (previousColumn == null)
                        {
                            await writer.WriteLineAsync($"ALTER TABLE \"{table.SchemaName}\".\"{table.TableName}\" ADD COLUMN \"{col.ColumnName}\" {columnTypeSQL}");
                        }
                        else if (previousColumn.Type != col.Type)
                        {
                            await writer.WriteLineAsync($"ALTER TABLE \"{table.SchemaName}\".\"{table.TableName}\" ALTER COLUMN \"{col.ColumnName}\" {columnTypeSQL}");
                        }
                    //    else
                    //    {
                    //        await writer.WriteLineAsync($"--ALTER TABLE \"{table.SchemaName}\".\"{table.TableName}\" ALTER COLUMN \"{col.ColumnName}\" {columnTypeSQL}");
                    //}
                    }

                    foreach(var col in table.Columns)
                {
                    var currentColumn = sourceTable.Columns.FirstOrDefault(sourceCol => sourceCol.ColumnName == col.ColumnName);

                    if (currentColumn == null)
                    {
                        await writer.WriteLineAsync($"ALTER TABLE \"{table.SchemaName}\".\"{table.TableName}\" DROP COLUMN \"{col.ColumnName}\";");
                    }


                }

                    await writer.WriteLineAsync($"");
                }
                await writer.WriteLineAsync($"");
                await writer.WriteLineAsync($"");


                await writer.WriteLineAsync("ROLLBACK");
            //}

            await writer.FlushAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);
            return (Stream)memoryStream;
        }
    }
}

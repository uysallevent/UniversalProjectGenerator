using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnivesalProjectGenerator.Models;

namespace UnivesalProjectGenerator.Helpers
{
    public class DbOperations
    {
        private SqlConnection connection;
        public async Task StartSqlConnection()
        {
            try
            {
                var connectionString = GetConnectionStrings();
                var tasks = new List<Task<Dictionary<string, List<TableDetails>>>>();
                foreach (var item in connectionString)
                {
                    connection = new SqlConnection(item);
                    await connection.OpenAsync();
                    await Console.Out.WriteLineAsync($"ServerVersion: {connection.ServerVersion}");
                    await Console.Out.WriteLineAsync($"State: {connection.State}");
                    tasks.Add(ExecuteCommand());
                }
                Task.WaitAll(tasks.ToArray());
                foreach (var item in tasks)
                {
                    CreateOrUpdateFiles(await item);
                }

            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        private async Task<Dictionary<string, List<TableDetails>>> ExecuteCommand()
        {
            var tablesDictionary = new Dictionary<string, List<TableDetails>>();
            try
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = @"SELECT 
                                            [Database]=t.TABLE_CATALOG,
                                            SchemaName = c.table_schema,
                                            TableName = c.table_name,
                                            ColumnName = c.column_name,
                                            DataType = data_type,
                                            [Type]= t.TABLE_TYPE
                                        FROM INFORMATION_SCHEMA.TABLES t
                                        INNER JOIN INFORMATION_SCHEMA.COLUMNS c ON T.TABLE_NAME = C.TABLE_NAME 
                                        ORDER BY T.TABLE_NAME";
                command.CommandTimeout = 15;
                command.CommandType = CommandType.Text;
                var result = await command.ExecuteReaderAsync();
                while (await result.ReadAsync())
                {
                    var key = result["Database"].ToString();
                    if (!tablesDictionary.ContainsKey(key))
                    {
                        tablesDictionary.Add(key, new List<TableDetails>());
                    }

                    tablesDictionary[key]
                        .Add(new TableDetails()
                        {
                            Schema = result["SchemaName"].ToString(),
                            TableName = result["TableName"].ToString(),
                            ColumnName = result["ColumnName"].ToString(),
                            DataType = result["DataType"].ToString()
                        });
                }

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
            return tablesDictionary;
        }

        private void CreateOrUpdateFiles(Dictionary<string, List<TableDetails>> DatabaseDetails)
        {
            var projectFolders = (Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory)).Select(x => new DirectoryInfo(x));
            foreach (var item in projectFolders)
            {
                switch (item.Extension)
                {
                    case ".DataAccess":

                        break;
                    case ".Entities":
                        DirectoryInfo directoriesInDatabases = null;
                        var directoriesInEntities = (Directory.GetDirectories(item.FullName)).Select(x => new DirectoryInfo(x));
                        if (!directoriesInEntities.Any(x => x.Name == "Databases"))
                        {
                            directoriesInDatabases = Directory.CreateDirectory($"{item.FullName}\\Databases");
                        }

                        var classTemplate = new StringBuilder();

                        foreach (var database in DatabaseDetails)
                        {
                            var databaseDirectories = Directory.CreateDirectory($"{directoriesInDatabases.FullName}{Path.DirectorySeparatorChar}{database.Key}");
                            database.Value.GroupBy(x => x.TableName).ToList().ForEach(x =>
                              {
                                  classTemplate.Clear();
                                  classTemplate.Append("" +
                                        $"namespace {item.Name}.Databases.{x.Key}{Environment.NewLine}" +
                                        $"{{{Environment.NewLine}    " +
                                        $"public class {database.Key}{Environment.NewLine}    " +
                                        $"{{{Environment.NewLine}{Environment.NewLine}   " +
                                        $"");
                                  FileInfo fi = new FileInfo($"{databaseDirectories.FullName}{Path.DirectorySeparatorChar}{x.Key}.cs");
                                  x.Where(y => y.TableName == x.Key).ToList().ForEach(y =>
                                  {
                                      classTemplate.Append($"      public {y.DataType} {y.ColumnName} {{ get; set; }}{Environment.NewLine}{Environment.NewLine}   ");
                                  });

                                  classTemplate.Append($"{Environment.NewLine}    }}");
                                  classTemplate.Append($"{Environment.NewLine}}}");

                                  using (FileStream fs = fi.Create())
                                  {
                                      Byte[] txt = new UTF8Encoding(true).GetBytes(classTemplate.ToString());
                                      fs.Write(txt, 0, txt.Length);
                                  }
                              });

                        }

                        break;
                    default:
                        break;
                }
            }

        }

        static private IEnumerable<string> GetConnectionStrings()
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(!string.IsNullOrWhiteSpace(envName) ? $"appsettings.{envName}.json" : "appsettings.json", true, true);
            var connectionString = builder.Build().GetSection("ConnectionStrings").GetChildren().Select(x => x.Value);
            return connectionString;
        }

        static private string HandleType(string type)
        {
            switch (type)
            {
                case "BASE TABLE":
                    return "Table";
                case "VIEW":
                    return "View";
                default:
                    return string.Empty;
            }
        }
    }
}

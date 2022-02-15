using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UniversalProjectGenerator.Helpers;
using UnivesalProjectGenerator.Helpers;
using UnivesalProjectGenerator.ProjectTypes.Dotnet;
using UnivesalProjectGenerator.ProjectTypes.React;

namespace UniversalProjectGenerator
{
    static class Program
    {
        static StringArrayComparer stringArrayComparer;

        static void Main(string[] args)
        {
            try
            {
                stringArrayComparer = new StringArrayComparer();
                MainAsync(args).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task MainAsync(string[] args)
        {
            await Console.Out.WriteLineAsync(" - - - - - - - - - - - - - - - - - - ");
            await Console.Out.WriteLineAsync("Universal Project Generator");
            await Console.Out.WriteLineAsync(" - - - - - - - - - - - - - - - - - - ");
            await Console.Out.WriteLineAsync();
            await WaitCommandAsync();
        }

        private static async Task WaitCommandAsync()
        {
            await Console.Out.WriteLineAsync("").ConfigureAwait(false);
            await Console.Out.WriteLineAsync("Waiting your command :").ConfigureAwait(false);
            await ExecuteCommandAsync(Console.ReadLine().Split(' ').Select(p => p.ToLowerInvariant()).ToArray());
        }

        private static async Task ExecuteCommandAsync(string[] command)
        {
            var foundCommand = GeneralCommandList().FirstOrDefault(x => stringArrayComparer.Equals(x.Key, command));
            if (foundCommand.Value != null)
            {
                await foundCommand.Value.Invoke("");
            }
            else
            {
                await Console.Out.WriteLineAsync($"There is no command found as {string.Join(" ", command)}");
            }
            await WaitCommandAsync();
        }

        private static Dictionary<string[], Func<string, Task>> GeneralCommandList()
        {
            var dbConnector = new DbOperations();
            return new Dictionary<string[], Func<string, Task>>()
            {
                 { new string[]{"help"}, async (c) =>{ await Console.Out.WriteLineAsync("Help contents");}},
                 { new string[]{"dotnetapi", "generate" }, async (c) =>{await DotNetGenerator.Generate(); }},
                 { new string[]{"react", "generate" }, async (c) =>{await ReactGeneration.Generate(); }},
                 { new string[]{ "entities", "create" }, async (c) =>{ await dbConnector.StartSqlConnection();}}
            };
        }
    }
}

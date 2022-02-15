using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniversalProjectGenerator.Helpers;
using UnivesalProjectGenerator.Helpers;

namespace UnivesalProjectGenerator.ProjectTypes.React
{
    public static class ReactGeneration
    {
        static string projectPath;

        public static async Task Generate()
        {
            await ProjectGenerationManagement().ConfigureAwait(false);
            await NpmPackageManagement();
            await FolderManagement();
            await FileWatcherHelper.WatchFile(AppDomain.CurrentDomain.BaseDirectory);

        }

        private static async Task ProjectGenerationManagement()
        {
            var projectGenerationScript = ScriptReaderHelper
                .ReadScriptFile($"{AppDomain.CurrentDomain.BaseDirectory}ProjectTypes{Path.DirectorySeparatorChar}React{Path.DirectorySeparatorChar}Scripts{Path.DirectorySeparatorChar}GenerateProject.txt");
            await RunScriptHelper.Execute(
                projectGenerationScript,
                AppDomain.CurrentDomain.BaseDirectory,
                "Project generation process has been started. Please wait !!!").ConfigureAwait(false);

            projectPath = $"{AppDomain.CurrentDomain.BaseDirectory}{projectGenerationScript.Split(" ").Last()}";
        }

        private static async Task NpmPackageManagement()
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}ProjectTypes\\React\\Scripts\\NpmPackageList.txt";
            var content = await File.ReadAllLinesAsync(path).ConfigureAwait(false);
            if (content == null || content.Length == 0)
            {
                await Console.Out.WriteLineAsync("There is no npm packages found for installing");
                return;
            }

            await RunScriptHelper.Execute(
                $"npm install { string.Join(" ", content)}",
                projectPath,
                "Npm package installation process has been started. Please wait !!!").ConfigureAwait(false);
        }

        private static async Task FolderManagement()
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}ProjectTypes\\React\\Scripts\\GenerateFolderAndFiles.txt";
            var content =await File.ReadAllLinesAsync(path).ConfigureAwait(false);
            if (content == null || content.Length == 0)
            {
                await Console.Out.WriteLineAsync("There is no folder or file found for creating");
                return;
            }

            var srcPath = $"{projectPath}\\src";
            foreach (var item in content)
            {
                if (item.Split(".").Last().ToLower() == "js" || item.Split(".").Last().ToLower() == "jsx")
                {
                    var fileInfo = new FileInfo($"{srcPath}\\{item}");
                    if (!Directory.Exists(fileInfo.DirectoryName))
                    {
                        Directory.CreateDirectory(fileInfo.DirectoryName);
                    }

                    File.Create($"{srcPath}\\{item}");
                }
                else
                {
                    Directory.CreateDirectory($"{srcPath}\\{item}");
                }
            }
        }
    }
}

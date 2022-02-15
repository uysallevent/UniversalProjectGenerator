using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalProjectGenerator.Helpers;
using UnivesalProjectGenerator.Helpers;

namespace UnivesalProjectGenerator.ProjectTypes.Dotnet
{
    public static class DotNetGenerator
    {
        static string projectPath;
        static string solutionName;
        static string nameSpace;
        static List<string> unNecessaryFiles;

        public static async Task Generate()
        {
            unNecessaryFiles = new List<string>();
            unNecessaryFiles.Add("Class1.cs");

            await ProjectGenerationManagement().ConfigureAwait(false);
            await FolderManagement();
            await FileWatcherHelper.WatchFile(AppDomain.CurrentDomain.BaseDirectory);
        }

        private static async Task ProjectGenerationManagement()
        {
            var projectGenerationScript = ScriptReaderHelper
                .ReadScriptFile($"{AppDomain.CurrentDomain.BaseDirectory}ProjectTypes{Path.DirectorySeparatorChar}DotNet{Path.DirectorySeparatorChar}Scripts{Path.DirectorySeparatorChar}CreateDotNetApiScript.txt");
            await RunScriptHelper.Execute(
                projectGenerationScript,
                AppDomain.CurrentDomain.BaseDirectory,
                "Project generation process has been started. Please wait !!!").ConfigureAwait(false);
            solutionName = (projectGenerationScript.Split(new[] { '\r', '\n' }).FirstOrDefault());
            solutionName = solutionName.Substring(solutionName.IndexOf("-o ") + 3);
            projectPath = $"{AppDomain.CurrentDomain.BaseDirectory}{solutionName}";
            ClearUnUsedFiles($"{AppDomain.CurrentDomain.BaseDirectory}{solutionName}");
        }

        private static async Task FolderManagement()
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}ProjectTypes\\Dotnet\\Scripts\\FolderAndFiles.txt";
            if (string.IsNullOrEmpty(path))
            {
                await Console.Out.WriteLineAsync("There are no folders or files found for generate");
            }
            var folderAndFiles = await File.ReadAllLinesAsync(path);

            for (int i = 0; i < folderAndFiles.Length; i++)
            {
                nameSpace = string.Empty;
                var assembylName = folderAndFiles[i].Replace(":{Solution Name}:", solutionName);

                if (folderAndFiles[i].Split(".").Last().ToLower() == "cs")
                {
                    var fileInfo = new FileInfo($"{projectPath}\\{assembylName}");
                    if (!Directory.Exists(fileInfo.DirectoryName))
                    {
                        Directory.CreateDirectory(fileInfo.DirectoryName);
                    }

                    int line = i;
                    var classTemplatePath = $"{AppDomain.CurrentDomain.BaseDirectory}ProjectTypes\\Dotnet\\Scripts\\SampleClass.txt";
                    var classTemplate = await File.ReadAllTextAsync(classTemplatePath);
                    nameSpace = folderAndFiles[i].Substring(0, folderAndFiles[i].LastIndexOf("\\\\")).Replace("\\\\", ".");
                    while (true)
                    {
                        line++;
                        if (line - i == 1)
                        {
                            CreateFileContent(":{create}:", ref classTemplate, fileInfo.Name.Split(".")[0]);

                            if ((line > folderAndFiles.Length - 1 || folderAndFiles[line].Contains(":{Solution Name}:")))
                            {
                                break;
                            }
                        }

                        if (line > folderAndFiles.Length - 1 || folderAndFiles[line].Contains(":{Solution Name}:"))
                        {
                            i = line - 1;
                            break;
                        }
                        CreateFileContent(folderAndFiles[line], ref classTemplate);
                    }

                    FileInfo fi = new FileInfo($"{projectPath}\\{assembylName}");
                    using (FileStream fs = fi.Create())
                    {
                        ClearUnUsedSection(ref classTemplate);
                        Byte[] txt = new UTF8Encoding(true).GetBytes(classTemplate);
                        fs.Write(txt, 0, txt.Length);
                    }
                }
                else
                {
                    Directory.CreateDirectory($"{projectPath}\\{assembylName}");
                }
            }

        }


        private static void CreateFileContent(string line, ref string classTemplate, string fileName = null)
        {
            if (line.Contains(":{create}:"))
            {
                nameSpace = nameSpace.Replace(":{Solution Name}:", solutionName);
                classTemplate = classTemplate.Replace(":{namespace}:", nameSpace);
                classTemplate = classTemplate.Replace(":{classname}:", fileName);
            }
            else if (line.Contains(":{accessmodifier}:"))
            {
                var accessmodifier = line.Replace(":{accessmodifier}:", "").Trim();
                classTemplate = classTemplate.Replace(":{accessmodifier}:", accessmodifier);
            }
            else if (line.Contains(":{usings}:"))
            {
                var usings = line.Replace(":{usings}:", "").Split('|');
                var sb = new StringBuilder();
                foreach (var item in usings)
                {
                    sb.Append($"using {item.Trim()};{Environment.NewLine}");
                }
                classTemplate = $"{sb.ToString()}{Environment.NewLine}{classTemplate}";
            }
            else if (line.Contains(":{inherit}:"))
            {
                var inherit = line.Replace(":{inherit}:", ":").Trim();
                classTemplate = classTemplate.Replace(":{inherit}:", inherit);
            }
            else if (line.Contains(":{method}:"))
            {

            }
        }

        private static void ClearUnUsedSection(ref string classTemplate)
        {
            classTemplate = classTemplate.Replace(":{inherit}:", "").Replace(":{method}:", "");
        }

        private static void ClearUnUsedFiles(string path)
        {
            var foldeList = Directory.GetDirectories(path);
            foreach (var item in foldeList)
            {
                var files = Directory.GetFiles(item);
                var filesWillBeRemoved = files.Where(x => unNecessaryFiles.Contains(x.Substring(x.LastIndexOf("\\") + 1))).ToList();
                if (filesWillBeRemoved.Any())
                {
                    filesWillBeRemoved.ForEach(x => File.Delete(x));
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnivesalProjectGenerator.Enums;

namespace UniversalProjectGenerator.Helpers
{
    public static class ScriptReaderHelper
    {

        public static string ReadScriptFile(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"There is no file on {scriptPath} path");
            }
            var content = File.ReadAllText(scriptPath);
            var script = InputChecker(content);
            return script;
        }

        private static string InputChecker(string content)
        {
            var inputList = new List<string>();

            for (int i = 0; i < content.Length; i++)
            {
                var foundChar = content[i];
                if (foundChar == ':' && content.Length > i && content[i + 1] == '{')
                {
                    i += 2;
                    var foundedInput = new StringBuilder();
                    while (true)
                    {

                        if (content[i] == '}')
                        {
                            if (!inputList.Any(x => x == foundedInput.ToString()))
                            {
                                inputList.Add(foundedInput.ToString());

                                Console.WriteLine($"Please enter a {foundedInput} :");
                                var inputValue = Console.ReadLine();
                                if (!string.IsNullOrEmpty(inputValue))
                                {
                                    content = content.Replace($":{{{foundedInput}}}:", inputValue);
                                    break;
                                }
                            }
                            break;
                        }
                        foundedInput.Append(content[i]);
                        i++;
                    }
                }
            }

            return content;
        }
    }
}

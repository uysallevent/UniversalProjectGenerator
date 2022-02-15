using System;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using UniversalProjectGenerator.Helpers;

namespace UnivesalProjectGenerator.Helpers
{
    public static class RunScriptHelper
    {
        public static async Task Execute(string script, string executePath, string waitingMessage)
        {
            using (Runspace runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();
                runspace.SessionStateProxy.Path.SetLocation(executePath);
                using (Pipeline pipeline = runspace.CreatePipeline())
                {
                    pipeline.Commands.AddScript(script);
                    pipeline.InvokeAsync();

                    await Console.Out.WriteLineAsync(waitingMessage);
                    await Console.Out.WriteLineAsync($">>>>>>{script}");

                    while (pipeline.PipelineStateInfo.State == PipelineState.Running || pipeline.PipelineStateInfo.State == PipelineState.Stopping)
                    {
                        ConsoleSpinnerHelper.Turn();
                    }

                    foreach (object item in pipeline.Error.ReadToEnd())
                    {
                        if (item != null)
                            Console.WriteLine(item.ToString());
                    }
                }
                runspace.Close();
            }
        }
    }
}

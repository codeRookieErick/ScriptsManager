using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    public class ShellProccess:IDisposable  
    {
        public event EventHandler<string> OutputReceived;
        Process process;
        public void ResetProcess()
        {
            Kill();
            process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                }
            };
            process.OutputDataReceived += ProcessOutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
        }

        public ShellProccess()
        {
            ResetProcess();
        }

        public void Exec(string data)
        {
            process.StandardInput.WriteLine(data);
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.OutputReceived?.Invoke(this, e.Data);
        }

        public void Kill()
        {
            if (!(process?.HasExited ?? true))
            {
                process?.Kill();
            }
        }

        public void Dispose()
        {
            Kill();
        }
    }
}

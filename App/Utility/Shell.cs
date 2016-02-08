using System;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Collector.Utility
{
    public class Shell
    {
        private Core S;

        public Shell(Core CollectorCore)
        {
            S = CollectorCore;
        }

        public string Execute(string file, string args = "", string workingdir = "", int waitForExit = 5)
        {
            var str = "";
            var p = new Process();
            p.StartInfo = new ProcessStartInfo();
            p.StartInfo.FileName = file;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = workingdir;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8; //fixes command-line output encoding
            p.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); str += e.Data + "\n"; };
           // p.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit(waitForExit * 1000);
            return str;
        }
    }
}

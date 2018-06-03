using System;
using System.Diagnostics;
using System.Text;

namespace Utility
{
    public static class Shell
    {
        public static string Execute(string file, string args = "", string workingdir = "", int waitForExit = 5)
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
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8; //fixes command-line output encoding
            p.OutputDataReceived += (sender, e) => { str += e.Data; };
            p.ErrorDataReceived += (sender, e) => { str += e.Data; };
            //p.WaitForExit();
            p.Start();
            p.BeginOutputReadLine();
            if (waitForExit > 0) { p.WaitForExit(waitForExit * 1000); }
            return str;
        }
    }
}

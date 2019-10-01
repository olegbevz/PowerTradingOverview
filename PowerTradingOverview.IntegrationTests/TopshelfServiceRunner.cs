using System;
using System.Diagnostics;
using System.Text;

namespace PowerTradingOverview.IntegrationTests
{
    public class TopshelfServiceRunner
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(5);

        private readonly string _fileName;
        private readonly string _instance;

        public bool Installed { get; private set; }

        public TopshelfServiceRunner(string fileName, string instance)
        {
            _fileName = fileName;
            _instance = instance;
        }

        public void InstallService()
        {
            var process = CreateShellProcess($"install -instance {_instance}");

            process.Start();
            if (!process.WaitForExit((int)_timeout.TotalMilliseconds))
                throw new TimeoutException();

            ValidateProcessOutput(process);

            if (process.ExitCode != 0)
                throw new Exception($"Process exit code is {process.ExitCode}");

            Installed = true;
        }

        public void UninstallService()
        {
            var process = CreateShellProcess($"uninstall -instance {_instance}");

            process.Start();
            if (!process.WaitForExit((int)_timeout.TotalMilliseconds))
                throw new TimeoutException();

            ValidateProcessOutput(process);

            if (process.ExitCode != 0)
                throw new Exception($"Process exit code is {process.ExitCode}");

            Installed = false;
        }

        private Process CreateShellProcess(string arguments)
        {
            var process = new Process();
            process.StartInfo.FileName = _fileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Verb = "runas";
            return process;
        }

        private static void ValidateProcessOutput(Process process)
        {
            var logReader = new Log4NetReader(process.StandardOutput);
            var errorMessages = new StringBuilder();
            foreach (var errorMessage in logReader.GetErrors())
                errorMessages.AppendLine(errorMessage);

            if (errorMessages.Length > 0)
                throw new Exception(errorMessages.ToString());
        }
    }
}

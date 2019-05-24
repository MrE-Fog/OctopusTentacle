using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Octopus.Diagnostics;
using Octopus.Shared.Diagnostics;

namespace Octopus.Shared.Startup
{
    class MutexHost : ICommandHost, ICommandRuntime
    {
        static readonly OctopusProgram.ExitCode[] FriendlyExitCodes =
        {
            OctopusProgram.ExitCode.Success,
            OctopusProgram.ExitCode.UnknownCommand,
            OctopusProgram.ExitCode.ControlledFailureException
        };
        readonly ILog log = Log.Octopus();

        private readonly string monitorMutexHost;
        private readonly CancellationTokenSource sourceToken = new CancellationTokenSource();
        private readonly ManualResetEventSlim shutdownTrigger = new ManualResetEventSlim(false);
        private Task task;

        public MutexHost(string monitorMutexHost)
        {
            this.monitorMutexHost = monitorMutexHost;
        }

        public void Run(Action<ICommandRuntime> start, Action shutdown)
        {
            if (Mutex.TryOpenExisting(monitorMutexHost, out var m))
            {
                task = Task.Run(() =>
                {
                    while (!sourceToken.IsCancellationRequested)
                    {
                        if (m.WaitOne(500))
                        {
                            shutdown();
                            shutdownTrigger.Set();
                            break;
                        }
                    }
                });
            }
            
            start(this);
        }

        public void Stop(Action shutdown)
        {
            sourceToken.Cancel();
            task?.Wait();

            if (shutdownTrigger.IsSet)
            {
                return;
            }

            shutdown();
            shutdownTrigger.Set();
        }

        public void OnExit(int exitCode)
        {
            if (FriendlyExitCodes.Cast<int>().Contains(exitCode)) return;

            var sb = new StringBuilder()
                .AppendLine(new string('-', 79))
                .AppendLine($"Terminating process with exit code {exitCode}")
                .AppendLine("Full error details are available in the log files at:");
            foreach (var logDirectory in OctopusLogsDirectoryRenderer.LogsDirectoryHistory)
            {
                sb.AppendLine(logDirectory);
            }
            sb.AppendLine("If you need help, please send these log files to https://octopus.com/support");
            sb.AppendLine(new string('-', 79));
            log.Fatal(sb.ToString());
        }

        public void WaitForUserToExit()
        {
            shutdownTrigger.Wait();
        }
    }
}
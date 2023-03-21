using System.Diagnostics;

namespace InteleViewerServerProcess
{
    public interface ILocalProcess
    {
        int CurrentProcessId();
        List<LocalProcess.process> GetProcesses();
    }
    public class LocalProcess : ILocalProcess
    {
        public class process
        {
            public string MainWindowTitle { get; set; }
            public int Id { get; set; }
            public string ProcessName { get; set; }
            public void Kill()
            {
                Process.GetProcessById(Id).Kill();
            }
        }

        public int CurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        public List<process> GetProcesses()
        {
            List<process> processList = new();

            Process[] nativeProcesses = Process.GetProcesses();

            for (int i = 0; i < nativeProcesses.Length; i += 1)
            {
                process? foundProcess = new()
                {
                    MainWindowTitle = nativeProcesses[i].MainWindowTitle,
                    Id = nativeProcesses[i].Id,
                    ProcessName = nativeProcesses[i].ProcessName,

                };
                processList.Add(foundProcess);
            }

            return processList;
        }
    }
}

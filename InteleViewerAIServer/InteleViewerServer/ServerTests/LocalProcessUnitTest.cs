using Microsoft.VisualStudio.TestTools.UnitTesting;
using InteleViewerServerProcess;
using InteleViewerServerLib;
using Moq;
using System.Diagnostics;

namespace ServerTests
{
    [TestClass]
    public class LocalProcessUnitTest
    {
        [TestMethod]
        public void Test_GetProcesses()
        {
            string filename = Path.Combine(Environment.CurrentDirectory, "Resources\\InfiniteLoopTestApp.exe");
            var processStartInfo = new ProcessStartInfo(filename);
            var knownProcess = Process.Start(processStartInfo);

            Assert.IsNotNull(knownProcess);

            var localProcess = new LocalProcess();
            var localProcesses = localProcess.GetProcesses();
            var ids = localProcesses.Select(p => p.Id).ToList();

            Assert.AreEqual(true, ids.Contains(knownProcess.Id));

            knownProcess.Kill();

            localProcess = new LocalProcess();
            localProcesses = localProcess.GetProcesses();
            ids = localProcesses.Select(p => p.Id).ToList();

            Assert.AreEqual(false, ids.Contains(knownProcess.Id));
        }
    }
}

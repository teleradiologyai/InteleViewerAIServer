using Microsoft.VisualStudio.TestTools.UnitTesting;
using InteleViewerServerProcess;
using InteleViewerServerLib;
using Moq;

namespace ServerTests
{


    [TestClass]
    public class InteleViewerDriverUnitTest
    {
        [TestMethod]
        public void Test_LoadOrderInPacs()
        {
            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockLocalProcess = new Mock<ILocalProcess>();
            var accessionNumber = "ABC-123";
            var patientId = "DEF-987";
            var flags = "";

            mockIvCom.Setup(mockObj => mockObj.loadOrderWithFlags(accessionNumber, patientId, flags));
            mockIvCom.SetupProperty(mockObj => mockObj.username);
            mockIvCom.SetupProperty(mockObj => mockObj.baseUrl);
            mockIvCom.SetupProperty(mockObj => mockObj.sessionId);

            var driver = new InteleViewerDriver()
            {
                InteleViewerCom = mockIvCom.Object,
                LocalProcess = mockLocalProcess.Object,
            };

            var openRequest = new LoadOrderRequest()
            {
                AccessionNumber = accessionNumber,
                PatientId = patientId, 
                Flags = flags,
                Username = "tbaker",
                BaseUrl = "https://test.foo",
                SessionId = "tbaker"
            };

            var result = driver.LoadOrderInPacs(openRequest);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Test_CurrentOpenStudyMatchesPatientName_NoImagesOpen()
        {
            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockLocalProcess = new Mock<ILocalProcess>();

            List<LocalProcess.process> processList = new();
            LocalProcess.process ivProcess = new()
            {
                MainWindowTitle = "InteleViewer Search Tool",
                Id = 1,
                ProcessName = "InteleViewer Process Name",

            };
            processList.Add(ivProcess);

            LocalProcess.process? otherProcess = new()
            {
                MainWindowTitle = "MS Paint",
                Id = 1,
                ProcessName = "Other Process Name",

            };
            processList.Add(otherProcess);

            mockLocalProcess.Setup(mockObj => mockObj.GetProcesses()).Returns(processList);

            var driver = new InteleViewerDriver()
            {
                InteleViewerCom = mockIvCom.Object,
                LocalProcess = mockLocalProcess.Object,
            };

            var result = driver.CurrentOpenStudyMatchesPatientName("James", "Dean");
            var match = result.Item1;
            var message = result.Item2;

            Assert.AreEqual(false, match);
            Assert.AreEqual("PACS does not appear to have any images open", message);
        }

        [TestMethod]
        public void Test_CurrentOpenStudyMatchesPatientName_CorrectImagesOpen()
        {
            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockLocalProcess = new Mock<ILocalProcess>();

            List<LocalProcess.process> processList = new();
            LocalProcess.process ivProcess = new()
            {
                MainWindowTitle = "DEAN^JAMES Hexarad Radiology InteleViewer",
                Id = 1,
                ProcessName = "InteleViewer Process Name",

            };
            processList.Add(ivProcess);

            LocalProcess.process? otherProcess = new()
            {
                MainWindowTitle = "MS Paint",
                Id = 1,
                ProcessName = "Other Process Name",

            };
            processList.Add(otherProcess);

            mockLocalProcess.Setup(mockObj => mockObj.GetProcesses()).Returns(processList);

            var driver = new InteleViewerDriver()
            {
                InteleViewerCom = mockIvCom.Object,
                LocalProcess = mockLocalProcess.Object,
            };

            var result = driver.CurrentOpenStudyMatchesPatientName("James", "Dean");
            var match = result.Item1;
            var message = result.Item2;

            Assert.AreEqual(true, match);
            Assert.AreEqual("Patient name matches", message);
        }

        [TestMethod]
        public void Test_CurrentOpenStudyMatchesPatientName_IncorrectImagesOpen()
        {
            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockLocalProcess = new Mock<ILocalProcess>();

            List<LocalProcess.process> processList = new();
            LocalProcess.process ivProcess = new()
            {
                MainWindowTitle = "DOE^JANE - Hexarad Radiology InteleViewer",
                Id = 1,
                ProcessName = "InteleViewer Process Name",

            };
            processList.Add(ivProcess);

            LocalProcess.process? otherProcess = new()
            {
                MainWindowTitle = "MS Paint",
                Id = 1,
                ProcessName = "Other Process Name",

            };
            processList.Add(otherProcess);

            mockLocalProcess.Setup(mockObj => mockObj.GetProcesses()).Returns(processList);

            var driver = new InteleViewerDriver()
            {
                InteleViewerCom = mockIvCom.Object,
                LocalProcess = mockLocalProcess.Object,
            };

            var result = driver.CurrentOpenStudyMatchesPatientName("James", "Dean");
            var match = result.Item1;
            var message = result.Item2;

            Assert.AreEqual(false, match);
            Assert.AreEqual("Patient name failed to match. Name from PACS is doe^jane", message);
        }

        [TestMethod]
        public void Test_CurrentOpenStudyMatchesPatientName_NoInteleradOpen()
        {
            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockLocalProcess = new Mock<ILocalProcess>();

            List<LocalProcess.process> processList = new();
            LocalProcess.process? otherProcess = new()
            {
                MainWindowTitle = "MS Paint",
                Id = 1,
                ProcessName = "Other Process Name",

            };
            processList.Add(otherProcess);

            mockLocalProcess.Setup(mockObj => mockObj.GetProcesses()).Returns(processList);

            var driver = new InteleViewerDriver()
            {
                InteleViewerCom = mockIvCom.Object,
                LocalProcess = mockLocalProcess.Object,
            };

            var result = driver.CurrentOpenStudyMatchesPatientName("James", "Dean");
            var match = result.Item1;
            var message = result.Item2;

            Assert.AreEqual(false, match);
            Assert.AreEqual("PACS does not appear to have any images open", message);
        }

        [TestMethod]
        public void Test_CurrentOpenStudyMatchesPatientName_MultipleInteleradWindowsOpen()
        {
            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockLocalProcess = new Mock<ILocalProcess>();

            List<LocalProcess.process> processList = new();
            LocalProcess.process ivProcess = new()
            {
                MainWindowTitle = "DOE^JANE - Hexarad Radiology InteleViewer",
                Id = 1,
                ProcessName = "InteleViewer Process Name",

            };
            processList.Add(ivProcess);

            LocalProcess.process ivProcess2 = new()
            {
                MainWindowTitle = "JONES^TOM - Hexarad Radiology InteleViewer",
                Id = 1,
                ProcessName = "InteleViewer Process Name",

            };
            processList.Add(ivProcess2);

            mockLocalProcess.Setup(mockObj => mockObj.GetProcesses()).Returns(processList);

            var driver = new InteleViewerDriver()
            {
                InteleViewerCom = mockIvCom.Object,
                LocalProcess = mockLocalProcess.Object,
            };

            var result = driver.CurrentOpenStudyMatchesPatientName("James", "Dean");
            var match = result.Item1;
            var message = result.Item2;

            Assert.AreEqual(false, match);
            Assert.AreEqual("Multiple Intelerad Windows Open", message);
        }
    }
}
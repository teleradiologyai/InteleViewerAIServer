using Microsoft.VisualStudio.TestTools.UnitTesting;
using InteleViewerServerProcess;
using InteleViewerServerLib;
using Moq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using System.Text;

namespace ServerTests
{


    [TestClass]
    public class InteleViewerControllerUnitTest
    {

        [TestMethod]
        public async Task Test_LoadOrder()
        {
            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockIvDriver = new Mock<IInteleViewerDriver>();
            var mockLocalProcess = new Mock<ILocalProcess>();

            List<LocalProcess.process> processList = new();
            LocalProcess.process ivProcess = new()
            {
                MainWindowTitle = "DEAN^JAMES InteleViewer",
                Id = 1,
                ProcessName = "InteleViewer Process Name",

            };
            processList.Add(ivProcess);

            string[] args = new string[] { };
            var app = Program.BuildServer(args, mockLocalProcess.Object);
            var client = app.StartAsync();



            InteleViewerController InteleViewerController = new InteleViewerController()
            {
                InteleViewerDriver = mockIvDriver.Object,
                InteleViewerCom = mockIvCom.Object,
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var feature = new Mock<IHttpResponseFeature>();

            feature.Setup(option => option.HasStarted).Returns(true);
            httpContext.Features.Set<IHttpResponseFeature>(feature.Object);
            RequestDelegate next = async (HttpContext hc) =>
            {
                await Task.CompletedTask;
            };

            InteleViewerController.LoadOrder(httpContext);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            Console.WriteLine(responseBody);

            Assert.AreEqual(responseBody, "{\"Success\":false,\"Message\":\"Bad Request, must pass in AccessionNumber, PatientId, BaseUrl, SessionId and Username\"}");
        }


        [TestMethod]
        public async Task Test_LoadOrderWithValidBody()
        {
            LoadOrderRequest requestBody = new LoadOrderRequest()
            {
                Username = "jdean",
                BaseUrl = "http://10.100.43.3",
                SessionId = "secretID",
                AccessionNumber = "ABC-123",
                PatientId = "DEF-456",
                Flags = "",

            };

            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockIvDriver = new Mock<IInteleViewerDriver>();
            var mockLocalProcess = new Mock<ILocalProcess>();
            mockIvDriver.Setup(mockObj => mockObj.LoadOrderInPacs(It.IsAny<LoadOrderRequest>())).Returns(true);

            string[] args = new string[] { };
            var app = Program.BuildServer(args, mockLocalProcess.Object);
            var client = app.StartAsync();

            InteleViewerController InteleViewerController = new InteleViewerController()
            {
                InteleViewerDriver = mockIvDriver.Object,
                InteleViewerCom = mockIvCom.Object,
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Response.ContentType = "";
            httpContext.Response.Body = new MemoryStream();
            var body = JsonConvert.SerializeObject(requestBody);
            byte[] byteArray = Encoding.ASCII.GetBytes(body);
            MemoryStream stream = new MemoryStream(byteArray);
            httpContext.Request.Body = stream;

            var feature = new Mock<IHttpResponseFeature>();

            feature.Setup(option => option.HasStarted).Returns(true);
            httpContext.Features.Set<IHttpResponseFeature>(feature.Object);
            RequestDelegate next = async (HttpContext hc) =>
            {
                await Task.CompletedTask;
            };

            InteleViewerController.LoadOrder(httpContext);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.AreEqual(responseBody, "{\"Success\":true,\"Message\":null}");
        }

        [TestMethod]
        public async Task Test_LoadOrderWithInvalidBody()
        {
            LoadOrderRequest requestBody = new LoadOrderRequest()
            {
                Username = "jdean",
                BaseUrl = "http://10.100.43.3",
                SessionId = "secretID",
                PatientId = "DEF-456",
                Flags = "",

            };

            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockIvDriver = new Mock<IInteleViewerDriver>();
            var mockLocalProcess = new Mock<ILocalProcess>();
            mockIvDriver.Setup(mockObj => mockObj.LoadOrderInPacs(It.IsAny<LoadOrderRequest>())).Returns(true);

            string[] args = new string[] { };
            var app = Program.BuildServer(args, mockLocalProcess.Object);
            var client = app.StartAsync();

            InteleViewerController InteleViewerController = new InteleViewerController()
            {
                InteleViewerDriver = mockIvDriver.Object,
                InteleViewerCom = mockIvCom.Object,
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Response.ContentType = "";
            httpContext.Response.Body = new MemoryStream();
            var body = JsonConvert.SerializeObject(requestBody);
            byte[] byteArray = Encoding.ASCII.GetBytes(body);
            MemoryStream stream = new MemoryStream(byteArray);
            httpContext.Request.Body = stream;

            var feature = new Mock<IHttpResponseFeature>();

            feature.Setup(option => option.HasStarted).Returns(true);
            httpContext.Features.Set<IHttpResponseFeature>(feature.Object);
            RequestDelegate next = async (HttpContext hc) =>
            {
                await Task.CompletedTask;
            };

            InteleViewerController.LoadOrder(httpContext);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.AreEqual(responseBody, "{\"Success\":false,\"Message\":\"Bad Request, must pass in AccessionNumber, PatientId, BaseUrl, SessionId and Username\"}");
        }

        [TestMethod]
        public async Task Test_CurrentOpenStudy()
        {
            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockIvDriver = new Mock<IInteleViewerDriver>();
            var mockLocalProcess = new Mock<ILocalProcess>();
            mockIvDriver.Setup(mockObj => mockObj.CurrentOpenStudyMatchesPatientName("James", "Dean")).Returns(Tuple.Create(true, "Patient name matches"));

            string[] args = new string[] { };
            var app = Program.BuildServer(args, mockLocalProcess.Object);
            var client = app.StartAsync();

            InteleViewerController InteleViewerController = new InteleViewerController()
            {
                InteleViewerDriver = mockIvDriver.Object,
                InteleViewerCom = mockIvCom.Object,
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            Dictionary<string, string?> queryAttrs = new Dictionary<string, string?>();
            queryAttrs.Add("patientFirstName", "James");
            queryAttrs.Add("patientLastName", "Dean");
            httpContext.Request.QueryString = QueryString.Create(queryAttrs);

            var feature = new Mock<IHttpResponseFeature>();
            feature.Setup(option => option.HasStarted).Returns(true);
            httpContext.Features.Set<IHttpResponseFeature>(feature.Object);
            RequestDelegate next = async (HttpContext hc) =>
            {
                await Task.CompletedTask;
            };

            InteleViewerController.CurrentOpenStudy(httpContext);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.AreEqual(responseBody, "{\"Matched\":true,\"Message\":\"Patient name matches\"}");
        }

        [TestMethod]
        public async Task Test_CurrentOpenStudyMissingRequiredAttributes()
        {
            var mockIvCom = new Mock<CInteleViewerControl>();
            var mockIvDriver = new Mock<IInteleViewerDriver>();
            var mockLocalProcess = new Mock<ILocalProcess>();
            mockIvDriver.Setup(mockObj => mockObj.CurrentOpenStudyMatchesPatientName("James", "Dean")).Returns(Tuple.Create(true, "Patient name matches"));

            string[] args = new string[] { };
            var app = Program.BuildServer(args, mockLocalProcess.Object);
            var client = app.StartAsync();

            InteleViewerController InteleViewerController = new InteleViewerController()
            {
                InteleViewerDriver = mockIvDriver.Object,
                InteleViewerCom = mockIvCom.Object,
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            Dictionary<string, string?> queryAttrs = new Dictionary<string, string?>();
            queryAttrs.Add("patientFirstName", "James");
            httpContext.Request.QueryString = QueryString.Create(queryAttrs);

            var feature = new Mock<IHttpResponseFeature>();
            feature.Setup(option => option.HasStarted).Returns(true);
            httpContext.Features.Set<IHttpResponseFeature>(feature.Object);
            RequestDelegate next = async (HttpContext hc) =>
            {
                await Task.CompletedTask;
            };

            InteleViewerController.CurrentOpenStudy(httpContext);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            Assert.AreEqual(responseBody, "Missing patientFirstName or patientLastName from query");
        }
    }
}
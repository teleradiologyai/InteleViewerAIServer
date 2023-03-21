using System.Net.Mime;
using InteleViewerServerLib;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Web;

namespace InteleViewerServerProcess
{
    class PingStatus
    {
        public string Version { get; set; }
        public bool Running { get; set; }
    }
    class Program
    {
        public static IHost BuildServer(string[] args, ILocalProcess localProcess)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("https://*:22789", "http://*:22788");
                    webBuilder.Configure(app => app.Run(async context =>
                        {

                            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                            context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                            context.Response.Headers.Add("Access-Control-Allow-Headers", "X-PINGOTHER, Content-Type");
                            context.Response.Headers.Add("Access-Control-Max-Age", "86400");

                            var InteleViewerCom = new CInteleViewerControlClass();
                            var InteleViewerDriver = new InteleViewerDriver()
                            {
                                InteleViewerCom = InteleViewerCom,
                                LocalProcess = localProcess,
                            };
                            InteleViewerController InteleViewerController = new InteleViewerController()
                            {
                                InteleViewerDriver = InteleViewerDriver,
                                InteleViewerCom = InteleViewerCom
                            };

                            if (context.Request.Path == "/iv/open" && context.Request.Method == "POST")
                            {
                                InteleViewerController.LoadOrder(context);
                            }
                            else if (context.Request.Path == "/iv/currentOpenStudy" && context.Request.Method == "GET")
                            {
                                InteleViewerController.CurrentOpenStudy(context);
                            }
                            else if (context.Request.Path == "/iv/ping" && context.Request.Method == "GET")
                            {
                                var status = new PingStatus()
                                {
                                    Version = "1.2.0",
                                    Running = true
                                };
                                var body = JsonConvert.SerializeObject(status);

                                context.Response.ContentType = MediaTypeNames.Application.Json;
                                await context.Response.WriteAsync(body);
                            }
                            else
                            {
                                //Return 404 for any other endpoint
                                context.Response.StatusCode = StatusCodes.Status404NotFound;
                                await context.Response.WriteAsync("Unknown path");
                            }
                        }));
                }
                )
                .Build();
        }
        public static void RunServer(string[] args, ILocalProcess localProcess)
        {
            BuildServer(args, localProcess).Run();
        }

        static void KillExistingProcess(ILocalProcess localProcess)
        {
            List<LocalProcess.process> processlist = localProcess.GetProcesses();
            var currentProcessId = localProcess.CurrentProcessId();

            foreach (LocalProcess.process process in processlist)
            {
                if ((process.Id != currentProcessId) && process.ProcessName == "InteleViewerServerProcess")
                {
                    process.Kill();
                }
            }

        }

        static void Main(string[] args)
        {
            var localProcess = new LocalProcess();

            KillExistingProcess(localProcess);
            RunServer(args, localProcess);
        }
    }
}
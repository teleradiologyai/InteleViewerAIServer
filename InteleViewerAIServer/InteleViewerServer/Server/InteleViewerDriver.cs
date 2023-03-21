using InteleViewerServerLib;

namespace InteleViewerServerProcess
{
    public class LoadOrderRequest
    {
        public string? Username { get; set; }
        public string? BaseUrl { get; set; }
        public string? SessionId { get; set; }
        public string? AccessionNumber { get; set; }
        public string? PatientId { get; set; }
        public string? Flags { get; set; }

        public bool Valid()
        {
            return AccessionNumber != null &&
                PatientId != null &&
                BaseUrl != null &&
                SessionId != null &&
                Username != null;
        }
    }

    public class CurrentOpenStudyPatientNameMatch
    {
        public bool Matched { get; set; }
        public string? Message { get; set; }
    }

    public interface IInteleViewerDriver
    {
        bool LoadOrderInPacs(LoadOrderRequest openRequest);
        Tuple<bool, string> CurrentOpenStudyMatchesPatientName(string PatientFirstName, string PatientLastName);
    }
    public class InteleViewerDriver : IInteleViewerDriver
    {
        public CInteleViewerControl InteleViewerCom
        {
            get; set;
        }
        public ILocalProcess LocalProcess { get; set; }

        public bool LoadOrderInPacs(LoadOrderRequest openRequest)
        {
            InteleViewerCom.username = openRequest.Username;
            InteleViewerCom.baseUrl = openRequest.BaseUrl;
            InteleViewerCom.sessionId = openRequest.SessionId;

            InteleViewerCom.loadOrderWithFlags(openRequest.AccessionNumber, openRequest.PatientId, openRequest.Flags);

            return true;
        }

        public Tuple<bool, string> CurrentOpenStudyMatchesPatientName(string PatientFirstName, string PatientLastName)
        {
            var matched = false;
            var currentIvWindows = CurrentIvWindows();
            string? message;

            if (currentIvWindows.Count == 0)
            {
                message = "PACS does not appear to have any images open";
            }
            else if (currentIvWindows.Count() > 1)
            {
                message = "Multiple Intelerad Windows Open";
            }
            else
            {
                var windowName = currentIvWindows.First().ToLower();
                var patientWindowName = windowName.Split("- hexarad radiology").First().Trim();


                if (windowName.Contains(PatientFirstName.ToLower()) && windowName.Contains(PatientLastName.ToLower()))
                {
                    message = "Patient name matches";
                    matched = true;
                }
                else if (!patientWindowName.Contains('^'))
                {
                    message = "PACS does not appear to have any images open";
                }
                else
                {
                    message = $"Patient name failed to match. Name from PACS is {patientWindowName}";
                }
            }
            return Tuple.Create(matched, message);
        }

        private List<string> CurrentIvWindows()
        {
            List<LocalProcess.process> processlist = LocalProcess.GetProcesses();
            List<string> IrWindows = new();

            foreach (LocalProcess.process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle) && 
                    process.MainWindowTitle.Contains("InteleViewer") && 
                    process.MainWindowTitle.Contains("Hexarad Radiology") &&
                    !process.MainWindowTitle.Contains("Search Tool"))
                {
                    IrWindows.Add(process.MainWindowTitle);
                }
            }

            return IrWindows;
        }
    }
}

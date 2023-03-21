using InteleViewerServerLib;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Web;

namespace InteleViewerServerProcess
{
    public class InteleViewerController
    {
        public IInteleViewerDriver InteleViewerDriver
        {
            get; set;
        }
        public CInteleViewerControl InteleViewerCom
        {
            get; set;
        }

        public class CurrentOpenStudyPatientNameMatchResponse
        {
            public bool Matched { get; set; }
            public string? Message { get; set; }
        }

        public class LoadOrderResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }

        async public void LoadOrder(HttpContext context)
        {
            using var reader = new StreamReader(context.Request.Body);
            var content = await reader.ReadToEndAsync();
            var success = false;

            if (content is not null)
            {
                LoadOrderRequest? loadOrderRequest = JsonConvert.DeserializeObject<LoadOrderRequest>(content);

                if (loadOrderRequest != null &&
                    loadOrderRequest.Valid())
                {
                    success = InteleViewerDriver.LoadOrderInPacs(loadOrderRequest);
                }
            }

            LoadOrderResponse response = new LoadOrderResponse();

            if (success == false)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Success = false;
                response.Message = "Bad Request, must pass in AccessionNumber, PatientId, BaseUrl, SessionId and Username";
            }
            else
            {
                response.Success = true;
            }

            var body = JsonConvert.SerializeObject(response);

            await context.Response.WriteAsync(body);
        }

        async public void CurrentOpenStudy(HttpContext context)
        {
            var parsedQuery = HttpUtility.ParseQueryString(context.Request.QueryString.ToString());
            string? PatientFirstName = parsedQuery.Get("patientFirstName");
            string? PatientLastName = parsedQuery.Get("patientLastName");

            if (PatientFirstName is null || PatientLastName is null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing patientFirstName or patientLastName from query");
                return;
            }

            Tuple<bool, string> result = InteleViewerDriver.CurrentOpenStudyMatchesPatientName(PatientFirstName, PatientLastName);
            var response = new CurrentOpenStudyPatientNameMatchResponse()
            {
                Matched = result.Item1,
                Message = result.Item2
            };

            var body = JsonConvert.SerializeObject(response);

            //context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(body);
        }


    }
}

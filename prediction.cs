// This code requires the Nuget package Microsoft.AspNet.WebApi.Client to be installed.
// Instructions for doing this in Visual Studio:
// Tools -> Nuget Package Manager -> Package Manager Console
// Install-Package Newtonsoft.Json

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace BigDataTraining.Function
{
    class Prediction
    {
       
        
        public static HttpClientHandler  global_handler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
            };

        private static HttpClient httpClient = new HttpClient(global_handler);
        public static async Task<float>  GetPrediction(float total1, float total2, float total3, float total4, ILogger log)
        {
			float ratio = (float)((total1 + total2)/ (total1 + total2 + total3 + total4 ));
            float res = 0.0f;
            // Request data goes here
            var scoreRequest = new
            {
                Inputs = new Dictionary<string, List<Dictionary<string, string>>>()
                {
                   {
                       "WebServiceInput0",
                        new List<Dictionary<string, string>>()
                        {
                            new Dictionary<string, string>()
                            {
                                {
                                   "Total_1", total1.ToString(CultureInfo.InvariantCulture.NumberFormat)
                                },
                                {
                                        "Total_2", total2.ToString(CultureInfo.InvariantCulture.NumberFormat)
                                    },
                                    {
                                        "Total_3", total3.ToString(CultureInfo.InvariantCulture.NumberFormat)
                                    },
                                    {
                                        "Total_4", total4.ToString(CultureInfo.InvariantCulture.NumberFormat)
                                    },
                                    {
                                        "ratio_3-4_1-2", ratio.ToString(CultureInfo.InvariantCulture.NumberFormat)
                                    },
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                

                const string apiKey = "BOBOmeZ0ar8A8rs7Hh2RpkCIG4IAOJtb"; // Replace this with the API key for the web service
				
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey);
               

                // WARNING: The 'await' statement below can result in a deadlock
                // if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false)
                // so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)

                var requestString = JsonConvert.SerializeObject(scoreRequest);
                var content = new StringContent(requestString);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PostAsync("http://20.84.235.202:80/api/v1/service/mywebservice/score", content);
                
                if (response.IsSuccessStatusCode)
                {
                    string result =   response.Content.ReadAsStringAsync().Result;
                    
                    JObject data = JObject.Parse(result);
                    res = float.Parse(data["Results"]["WebServiceOutput0"][0]["Scored Labels"].ToString());
              
                    
                }
                else
                {
                    log.LogInformation(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                    log.LogInformation(response.Headers.ToString());

                    string responseContent =   response.Content.ReadAsStringAsync().Result;
                    log.LogInformation(responseContent);
                }
            

            return res;
        }
    }
}
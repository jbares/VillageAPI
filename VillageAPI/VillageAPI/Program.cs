using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace VillageAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            SlackClient slack = new SlackClient("https://hooks.slack.com/services/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");
            var movieId = "HO00009921";            
            var villageUrl = @"https://villagecinemas.com.au/VCAWebsite/Helpers/BindRHSTicketWidget.ashx?requestObject={%22SelectedTab%22:%22ByCinema%22,%22FilterSelected%22:%22VMAX%22,%22DropdownValue%22:%22" + movieId + "%7C303%22,%22Context%22:%22ByCinemaSessions%22,%22isSpecailEvent%22:%22false%22}";
            var count = 1;
            //var hasSessions = false;
                                   
            while(true)
            {                        
                Console.Write("\r{0}", $"Number of API calls: {count}");

                var response = GET(villageUrl);
                var villageData = JsonConvert.DeserializeObject<List<VillageData>>(response);
                villageData.RemoveAt(0);

                if(villageData.Count > 0)
                {
                    var msg = $"There are currently *{villageData.Count}* sessions avaliable for http://villagecinemas.com.au/movies/rogue-one-a-star-wars-story-2d";
                    slack.PostMessage(msg);
                    break;
                }

                count++;

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));                
            }
        }

        // Returns JSON string
        static string GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using(Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch(WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using(Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                }
                throw;
            }
        }        
    }

    public class SlackClient
    {
        private readonly Uri _uri;
        private readonly Encoding _encoding = new UTF8Encoding();

        public SlackClient(string urlWithAccessToken)
        {
            _uri = new Uri(urlWithAccessToken);
        }

        //Post a message using simple strings
        public void PostMessage(string text, string username = null, string channel = null)
        {
            Payload payload = new Payload()
            {
                Channel = channel,
                Username = username,
                Text = text
            };

            PostMessage(payload);
        }

        //Post a message using a Payload object
        public void PostMessage(Payload payload)
        {
            string payloadJson = JsonConvert.SerializeObject(payload);

            using(WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = payloadJson;

                var response = client.UploadValues(_uri, "POST", data);

                //The response text is usually "ok"
                string responseText = _encoding.GetString(response);
            }
        }
    }

    //This class serializes into the Json payload required by Slack Incoming WebHooks
    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class VillageData
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }

        [JsonProperty("SessionAbbreviative")]
        public string SessionAbbreviative { get; set; }

        [JsonProperty("IsSoldOut")]
        public string IsSoldOut { get; set; }

        [JsonProperty("AttributeCode")]
        public string AttributeCode { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("IsOnlineBookingDisabled")]
        public string IsOnlineBookingDisabled { get; set; }

        [JsonProperty("WarningMessage")]
        public string WarningMessage { get; set; }

        [JsonProperty("CapacityStatus")]
        public string CapacityStatus { get; set; }

        [JsonProperty("SortOrder")]
        public string SortOrder { get; set; }
    }
}

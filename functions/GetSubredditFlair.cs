using CloudWithChris.Integrations.Approvals.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace CloudWithChris.Integrations.Approvals.Functions
{
    public static class GetSubRedditFlair
    {
        [FunctionName("GetSubRedditFlair")]
        public static async Task<IActionResult> Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "flair/{subreddit}")] HttpRequest req,
                string subreddit,
                ILogger log)
        {
            // First get an access token to call the Flair API
            // We need to pass in several details including Username/Password of the user who we'll be posting as,
            // Client ID, and Client Secret. There are other ways to authenticate, and this is a TODO to refactor
            // to make this more of a service based approach in the future.
            // 
            // Only the flair scope is required to get access to the list of flairs for the subreddit.
            Uri oAuthUrl = new Uri("https://www.reddit.com/api/v1/access_token");
            var authParameters = new Dictionary<string, string>();
            authParameters.Add("grant_type", "password");
            authParameters.Add("username", System.Environment.GetEnvironmentVariable("RedditUsername"));
            authParameters.Add("password", System.Environment.GetEnvironmentVariable("RedditPassword"));
            authParameters.Add("scope", "flair");
            var byteArray = Encoding.ASCII.GetBytes($"{System.Environment.GetEnvironmentVariable("RedditClientID")}:{System.Environment.GetEnvironmentVariable("RedditClientSecret")}");

            // Form the request for the token.
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage oauthRequest = new HttpRequestMessage(HttpMethod.Post, oAuthUrl) {
                Content = new FormUrlEncodedContent(authParameters) 
            };
            oauthRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            oauthRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            // Obtain the token response and deserialize it into a POCO so we can
            // use the access_token at a later point.
            HttpResponseMessage oauthResponse = await httpClient.SendAsync(oauthRequest);
            string oauthOutput = await oauthResponse.Content.ReadAsStringAsync();
            TokenResult tokenResult = JsonConvert.DeserializeObject<TokenResult>(oauthOutput);

            // Now we're going to call the link_flair_v2 API as documented over at 
            // https://www.reddit.com/dev/api#GET_api_link_flair_v2
            //
            // A User-Agent is required by the API, so we'll leave this as the below.
            Uri flairURL = new Uri($"https://oauth.reddit.com/r/{subreddit}/api/link_flair_v2");
            HttpRequestMessage flairRequest = new HttpRequestMessage(HttpMethod.Get, flairURL);
            flairRequest.Headers.Add("User-Agent", "CloudWithChris Flair Checker");
            flairRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.access_token);

            // Obtain the response from the link_flair_v2 API and return it as an Object in the 
            // HTTP Response with the Deserialized Output of List<Flair>
            HttpResponseMessage flairResponse = await httpClient.SendAsync(flairRequest);
            string flairResponseJsonString = await flairResponse.Content.ReadAsStringAsync();
            return new OkObjectResult((JsonConvert.DeserializeObject<List<Flair>>(flairResponseJsonString)));
        }
    }
}

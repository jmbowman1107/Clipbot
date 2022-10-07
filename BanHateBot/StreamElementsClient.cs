using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BanHateBot.Models;
using Newtonsoft.Json;

namespace BanHateBot
{
    public class StreamElementsClient
    {
        #region ChannelId
        public string ChannelId { get; set; }
        #endregion
        #region JwtTokenString
        public string JwtTokenString { get; set; }
        #endregion
        #region HttpClient
        protected HttpClient HttpClient
        {
            get
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", JwtTokenString);
                return httpClient;
            }
        } 
        #endregion

        #region GetUser
        public async Task<StreamElementsUser> GetUser(string userName)
        {
            var response = await HttpClient.GetStringAsync(new Uri($"https://api.streamelements.com/kappa/v2/points/{ChannelId}/{userName}"));
            try
            {
                return JsonConvert.DeserializeObject<StreamElementsUser>(response);
            }
            catch
            {
                // TODO: What to do here..
                return null;
            }
        }
        #endregion
        #region AddOrRemovePointsFromUser
        public async Task AddOrRemovePointsFromUser(string userName, int amountOfPoints)
        {
            await HttpClient.PutAsync(new Uri($"https://api.streamelements.com/kappa/v2/points/{ChannelId}/{userName}/{amountOfPoints}"), null);
        } 
        #endregion
    }
}

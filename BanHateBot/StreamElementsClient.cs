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
        public string ChannelId { get; set; } = "61a507004f57efdbaa1b78d5"; 
        #endregion
        #region JwtTokenString
        public string JwtTokenString { get; set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyIjoiNjFhNTA3MDA0ZjU3ZWYzMTY1MWI3OGQ0Iiwicm9sZSI6Im93bmVyIiwiY2hhbm5lbCI6IjYxYTUwNzAwNGY1N2VmZGJhYTFiNzhkNSIsInByb3ZpZGVyIjoidHdpdGNoIiwiYXV0aFRva2VuIjoibGxLV1JzT29DaUo4ZTY2WS1aWkJIOHJpX21IdHVlS0o4RlRncldOVVk3d0VTTnRGIiwiaWF0IjoxNjM4ODI5Mzc4LCJpc3MiOiJTdHJlYW1FbGVtZW50cyJ9.igfh4z4cXrKTgZeaXDU0-kpiS6I9ZkuqklnMeSBLKDU"; 
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
                // TODO:
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

using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Diploma_backend.API.Models;
using Diploma_backend.API.Models.Google;
using Diploma_backend.API.Models.Input;
using Newtonsoft.Json;

namespace Diploma_backend.API.GoogleMaps
{
    public static class DistanceHelper
    {
        public static async Task<DistanceMatrix> GetDistanceMatrix(RequestVM model)
        {
            var encodedPointsString = 
                HttpContext.Current.Server.UrlEncode(string.Join("|", model.TechnicalObjects.Select(o => $"{o.Lat},{o.Lng}")) + 
                "|" +
                string.Join("|", model.RepairShops.Select(o => $"{o.Lat},{o.Lng}")));

            HttpResponseMessage response = null;

            using (var client = new HttpClient())
            {
                response = await client.GetAsync($@"https://maps.googleapis.com/maps/api/distancematrix/json?origins={encodedPointsString}&destinations={encodedPointsString}&key={ConfigurationManager.AppSettings["DistanceMatrixApiKey"]}");
            }

            var distanceMatrixResponse = JsonConvert.DeserializeObject<DistanceMatrixResponse>(await response.Content.ReadAsStringAsync());

            var matrix = new DistanceMatrix(distanceMatrixResponse, model.TechnicalObjects.Count());
            return matrix;
        }
    }
}
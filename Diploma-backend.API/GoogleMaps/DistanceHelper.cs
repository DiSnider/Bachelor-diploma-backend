using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Diploma_backend.API.Models.Input;

namespace Diploma_backend.API.GoogleMaps
{
    public class DistanceHelper
    {
        public async Task GetDistanceMatrix(RequestVM model)
        {
            var encodedPointsString = HttpContext.Current.Server.UrlEncode(string.Join("|", model.TechnicalObjects.Select(o => $"{o.Lat},{o.Lng}")));

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($@"https://maps.googleapis.com/maps/api/distancematrix/json?origins={encodedPointsString}&destinations={encodedPointsString}&key={ConfigurationManager.AppSettings["DistanceMatrixApiKey"]}");
            }
        }
    }
}
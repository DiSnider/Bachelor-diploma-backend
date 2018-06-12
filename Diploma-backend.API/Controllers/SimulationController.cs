using System.Threading.Tasks;
using System.Web.Http;
using Diploma_backend.API.GoogleMaps;
using Diploma_backend.API.Models.Input;

namespace Diploma_backend.API.Controllers
{
    public class SimulationController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> SimulateAndGetResult(RequestVM model)
        {
            await new DistanceHelper().GetDistanceMatrix(model);
            return Ok();
        }
    }
}

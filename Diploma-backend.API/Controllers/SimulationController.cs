using System.Threading.Tasks;
using System.Web.Http;
using Diploma_backend.API.GoogleMaps;
using Diploma_backend.API.Models.Input;
using Diploma_backend.API.SimulationLogic;

namespace Diploma_backend.API.Controllers
{
    public class SimulationController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> SimulateAndGetResult(RequestVM model)
        {
            var distanceMatrix = await DistanceHelper.GetDistanceMatrix(model);

            var simulationProcessController = new SimulationProcessController(model, distanceMatrix);
            var simulationProcessResult = simulationProcessController.StartSimulationProcessSession();

            return Json(simulationProcessResult);
        }
    }
}

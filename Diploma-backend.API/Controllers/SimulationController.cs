using System.Net;
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
            RecalculateMeasumentUnits(model);

            var distanceMatrix = await DistanceHelper.GetDistanceMatrix(model);

            var simulationProcessController = new SimulationProcessController(model, distanceMatrix);

            var delayTask = Task.Delay(10000);
            var simulationTask = Task.Run(() => simulationProcessController.StartSimulationProcessSession());

            var firstTask = await Task.WhenAny(simulationTask, delayTask);

            if (firstTask == simulationTask && simulationTask.Result != null)
            {
                return Json(simulationTask.Result);
            }

            return Content(HttpStatusCode.Accepted, "Для даних об'єктів і ремонтних станцій за допустимий час не вдалось знайти розв'язку");
        }
        
        private void RecalculateMeasumentUnits(RequestVM model)
        {
            foreach (var technicalObject in model.TechnicalObjects)
            {
                technicalObject.Intensity = technicalObject.Intensity / 24;
            }

            model.MachineSpeed = model.MachineSpeed * 1000;
        }
    }
}

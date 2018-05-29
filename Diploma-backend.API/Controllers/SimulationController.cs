using System.Threading.Tasks;
using System.Web.Http;

namespace Diploma_backend.API.Controllers
{
    public class SimulationController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> SimulateAndGetResult()
        {
            return Ok();
        }
    }
}

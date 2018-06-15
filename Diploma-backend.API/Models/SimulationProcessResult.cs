namespace Diploma_backend.API.Models
{
    public class SimulationProcessResult
    {
        public int[] OptimalRepairShopsCountsByStations { get; set; }

        public decimal MeanIdleTime { get; set; }
    }
}
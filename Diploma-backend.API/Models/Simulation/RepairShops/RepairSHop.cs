namespace Diploma_backend.API.Models.Simulation
{
    public class RepairShop
    {
        public int RepairStationNumber { get; set; }

        public RepairShopState CurrentState { get; set; }
    }
}
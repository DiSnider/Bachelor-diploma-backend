namespace Diploma_backend.API.Models.Simulation
{
    public class BackToStationRepairShopState : RepairShopState, IRepairShopStateWithTimeToChange
    {
        public decimal TimeLeft { get; set; }
    }
}
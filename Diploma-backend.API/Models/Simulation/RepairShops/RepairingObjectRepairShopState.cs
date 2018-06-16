namespace Diploma_backend.API.Models.Simulation
{
    public class RepairingObjectRepairShopState : RepairShopState, IRepairShopStateWithTimeToChange
    {
        public decimal TimeLeft { get; set; }

        public int ObjectToRepairIndex { get; set; }
    }
}
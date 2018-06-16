namespace Diploma_backend.API.Models.Simulation
{
    public class RepairingObjectRepairShopState : RepairShopState, IRepairShopStateWithTimeToChange
    {
        public decimal TimeLeft { get; set; }

        public TechnicalObject ObjectToRepair { get; set; }
    }
}
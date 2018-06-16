namespace Diploma_backend.API.Models.Simulation
{
    public class GoToObjectRepairShopState : RepairShopState, IRepairShopStateWithTimeToChange
    {
        public decimal TimeLeft { get; set; }

        public TechnicalObject ObjectToArrive { get; set; }
    }
}
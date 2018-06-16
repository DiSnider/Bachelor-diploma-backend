namespace Diploma_backend.API.Models.Simulation
{
    public class GoToObjectRepairShopState : RepairShopState, IRepairShopStateWithTimeToChange
    {
        public decimal TimeLeft { get; set; }

        public int ObjectToArriveIndex { get; set; }
    }
}
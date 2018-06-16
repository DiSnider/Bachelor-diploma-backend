namespace Diploma_backend.API.Models.Simulation
{
    public interface IRepairShopStateWithTimeToChange
    {
        decimal TimeLeft { get; set; }
    }
}
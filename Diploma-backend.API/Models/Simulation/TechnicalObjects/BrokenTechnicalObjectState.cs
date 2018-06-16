namespace Diploma_backend.API.Models.Simulation
{
    public class BrokenTechnicalObjectState : TechnicalObjectState
    {
        public decimal? IdleTimeStamp { get; set; }

        public decimal? ConfirmationDelayTimeStamp { get; set; }
    }
}
using System.Collections.Generic;

namespace Diploma_backend.API.Models.Input
{
    public class RequestVM
    {
        public IEnumerable<IntensityPointVM> TechnicalObjects { get; set; }
        public IEnumerable<PointVM> RepairShops { get; set; }
        public decimal RepairDuration { get; set; }
        public decimal MachineSpeed { get; set; }
        public decimal PermissibleIdleTime { get; set; }
    }
}
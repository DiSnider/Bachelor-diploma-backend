using Diploma_backend.API.Models;
using Diploma_backend.API.Models.Input;

namespace Diploma_backend.API.SimulationLogic
{
    public class SimulationProcess
    {
        private readonly RequestVM _model;
        private readonly DistanceMatrix _distanceMatrix;
        private readonly int[] _currentRepairStations;

        public SimulationProcess(RequestVM model, DistanceMatrix distanceMatrix, int[] currentRepairStations)
        {
            _model = model;
            _distanceMatrix = distanceMatrix;
            _currentRepairStations = currentRepairStations;
        }

        /// <summary>
        /// Simulation algorithm
        /// </summary>
        public decimal SimulateAndGetMeanObjectIdleTime()
        {
            return 12.34m;
        }
    }
}
using System;
using System.Threading;
using Diploma_backend.API.Models;
using Diploma_backend.API.Models.Input;

namespace Diploma_backend.API.SimulationLogic
{
    public class SimulationProcessController
    {
        private readonly RequestVM _model;
        private readonly DistanceMatrix _distanceMatrix;

        public SimulationProcessController(RequestVM model, DistanceMatrix distanceMatrix)
        {
            _model = model;
            _distanceMatrix = distanceMatrix;
        }

        public SimulationProcessResult StartSimulationProcessSession()
        {
            bool @continue = true;
            (decimal, decimal) result = (0m, 0m);
            int[] currentRepairStations = new int[_distanceMatrix.RepairStationsCount];

            for (int i = 0; i < currentRepairStations.Length && @continue; i++)
            {
                currentRepairStations = new int[_distanceMatrix.RepairStationsCount];

                for (int j = 0; j < 5 && @continue; j++)
                {
                    currentRepairStations[i]++;

                    var simulationProcess = new SimulationProcess(_model, _distanceMatrix, currentRepairStations);
                    result = simulationProcess.SimulateAndGetMeanCharacteristics();

                    @continue = result.Item1 > _model.PermissibleIdleTime || result.Item2 > _model.PermissibleConfirmationDelayTime;
                }              
            }

            return new SimulationProcessResult
            {
                OptimalRepairShopsCountsByStations = currentRepairStations,
                MeanIdleTime = result.Item1,
                MeanConfirmationDelayTime = result.Item2
            };
        }
    }
}
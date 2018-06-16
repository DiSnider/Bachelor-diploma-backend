using System;
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
            bool @continue;
            (decimal, decimal) result;
            var currentRepairStations = new int[_distanceMatrix.RepairStationsCount];

            do
            {
                //ToDO changing currentRepairStations
                currentRepairStations[0]++;

                var simulationProcess = new SimulationProcess(_model, _distanceMatrix, currentRepairStations);
                result = simulationProcess.SimulateAndGetMeanObjectIdleTime();

                @continue = result.Item1 > _model.PermissibleIdleTime || result.Item2 > _model.PermissibleConfirmationDelayTime;
            }
            while (@continue);

            return new SimulationProcessResult
            {
                OptimalRepairShopsCountsByStations = currentRepairStations,
                MeanIdleTime = result.Item1,
                MeanConfirmationDelayTime = result.Item2
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Diploma_backend.API.Models;
using Diploma_backend.API.Models.Input;
using Diploma_backend.API.Models.Simulation;

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

        private const int SimulationDays = 365; //one year

        /// <summary>
        /// Simulation algorithm
        /// </summary>
        public decimal SimulateAndGetMeanObjectIdleTime()
        {
            (List<TechnicalObject> technicalObjects, List<RepairShop> repairShops) = SetupInitialStates();

            var idleTimes = new List<decimal>();
            //var confirmationDelayTimes = new List<decimal>();

            var time = 0m;
            while (time < SimulationDays * 24)
            {
                var minimalTechnicalObjectIndex = GetMinimalTechnicalObjectIndex(technicalObjects);
                var minimalRepairShopIndex = GetMinimalRepairShopIndex(repairShops);

                decimal timeStep;
                var isMinTechnicalObject = false;
                if (!minimalTechnicalObjectIndex.HasValue)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    timeStep = (repairShops[minimalRepairShopIndex.Value].CurrentState as IRepairShopStateWithTimeToChange).TimeLeft;
                }
                else if (!minimalRepairShopIndex.HasValue)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    timeStep = (technicalObjects[minimalTechnicalObjectIndex.Value].CurrentState as WorkingTechnicalObject).TimeToBreak;
                    isMinTechnicalObject = true;
                }
                else
                {
                    var minimalTechnicalObjectTime = (technicalObjects[minimalTechnicalObjectIndex.Value].CurrentState as WorkingTechnicalObject).TimeToBreak;
                    var minimalRepairShopTime = (repairShops[minimalRepairShopIndex.Value].CurrentState as IRepairShopStateWithTimeToChange).TimeLeft;
                    timeStep = Math.Min(minimalTechnicalObjectTime, minimalRepairShopTime);

                    isMinTechnicalObject = minimalTechnicalObjectTime <= minimalRepairShopTime;
                }

                //Event #1 Відмова і-того об'єкта
                if (isMinTechnicalObject) 
                {

                }
                //Event #2 Початок ремонту і-того об'єкта
                else if (repairShops[minimalRepairShopIndex.Value].CurrentState is GoToObjectRepairShopState)
                {
                    
                }
                //Event #3 Закінчення ремонту і-того об'єкта
                else if (repairShops[minimalRepairShopIndex.Value].CurrentState is RepairingObjectRepairShopState)
                {

                }
                //Event #4 ЗПрибуття на свою базу і-тої машини
                else if (repairShops[minimalRepairShopIndex.Value].CurrentState is BackToStationRepairShopState)
                {
                    
                }

                //ToDo decrease all times on timeStep in right moment

                time += timeStep;
            }

            var meanIdleTime = idleTimes.Sum() / idleTimes.Count;
            return meanIdleTime;
        }

        private int? GetMinimalTechnicalObjectIndex(List<TechnicalObject> technicalObjects)
        {
            int? minimalTechnicalObjectIndex = 0;
            var workingTechnicalObjects = technicalObjects.Where(t => t.CurrentState is WorkingTechnicalObject).ToList();
            if (workingTechnicalObjects.Any())
            {
                var minimalTimeToBreak = workingTechnicalObjects.Min(t => (t.CurrentState as WorkingTechnicalObject).TimeToBreak);
                for (int i = 0; i < technicalObjects.Count; i++)
                {
                    if (technicalObjects[i].CurrentState is WorkingTechnicalObject
                        && (technicalObjects[i].CurrentState as WorkingTechnicalObject).TimeToBreak ==
                        minimalTimeToBreak)
                    {
                        minimalTechnicalObjectIndex = i;
                    }
                }
            }

            return minimalTechnicalObjectIndex;
        }

        private int? GetMinimalRepairShopIndex(List<RepairShop> repairShops)
        {
            int? minimalRepairShopIndex = 0;
            var timedRepairShops = repairShops.Where(t => t.CurrentState is IRepairShopStateWithTimeToChange).ToList();
            if (timedRepairShops.Any())
            {
                var minimalTimeLeft = timedRepairShops.Min(t => (t.CurrentState as IRepairShopStateWithTimeToChange).TimeLeft);
                for (int i = 0; i < repairShops.Count; i++)
                {
                    if (repairShops[i].CurrentState is IRepairShopStateWithTimeToChange
                        && (repairShops[i].CurrentState as IRepairShopStateWithTimeToChange).TimeLeft ==
                        minimalTimeLeft)
                    {
                        minimalRepairShopIndex = i;
                    }
                }
            }

            return minimalRepairShopIndex;
        }

        private (List<TechnicalObject>, List<RepairShop>) SetupInitialStates()
        {
            var technicalObjects = _model.TechnicalObjects.Select(to => new TechnicalObject
            {
                Intensity = to.Intensity,
                CurrentState = new WorkingTechnicalObject { TimeToBreak = ExponentialValueGenerator.Get(to.Intensity) }
            }).ToList();

            var repairShops = new List<RepairShop>();
            for (int i = 0; i < _model.RepairShops.Count(); i++)
            {
                if (_currentRepairStations[i] > 0)
                {
                    //добавляємо машини від усіх станцій
                    for (int j = 0; j < _currentRepairStations[i]; j++)
                    {
                        repairShops.Add(new RepairShop
                        {
                            RepairStationNumber = i,
                            CurrentState = new OnStationRepairShopState()
                        });
                    }
                }
            }

            return (technicalObjects, repairShops);
        }
    }
}
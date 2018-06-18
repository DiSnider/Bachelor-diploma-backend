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

        private List<TechnicalObject> technicalObjects;
        private List<RepairShop> repairShops;
        private readonly List<decimal> idleTimes;
        private readonly List<decimal> confirmationDelayTimes;

        private decimal time;

        private const int SimulationDays = 10000;

        public SimulationProcess(RequestVM model, DistanceMatrix distanceMatrix, int[] currentRepairStations)
        {
            _model = model;
            _distanceMatrix = distanceMatrix;
            _currentRepairStations = currentRepairStations;

            (technicalObjects, repairShops) = SetupInitialStates();

            idleTimes = new List<decimal>();
            confirmationDelayTimes = new List<decimal>();

            time = 0m;
        }

        
        /// <summary>
        /// Simulation algorithm
        /// </summary>
        public (decimal, decimal) SimulateAndGetMeanCharacteristics()
        {
            try
            {
                while (time < SimulationDays * 24)
                {
                    var (minimalTechnicalObjectIndex, minimalTechnicalObjectTime) = GetMinimalTechnicalObjectData();
                    var (minimalRepairShopIndex, minimalRepairShopTime) = GetMinimalRepairShopData();

                    decimal timeStep;
                    var isMinTechnicalObject = false;
                    if (!minimalTechnicalObjectIndex.HasValue)
                    {
                        timeStep = minimalRepairShopTime.Value;
                    }
                    else if (!minimalRepairShopIndex.HasValue)
                    {
                        timeStep = minimalTechnicalObjectTime.Value;
                        isMinTechnicalObject = true;
                    }
                    else
                    { 
                        timeStep = Math.Min(minimalTechnicalObjectTime.Value, minimalRepairShopTime.Value);

                        isMinTechnicalObject = minimalTechnicalObjectTime <= minimalRepairShopTime;
                    }

                    (List<int>, List<int>) indexesNotToTouch = (new List<int>(), new List<int>());

                    //Event #1 Відмова і-того об'єкта
                    if (isMinTechnicalObject)
                    {
                        indexesNotToTouch = ProcessObjectsBreaking(minimalTechnicalObjectIndex.Value, timeStep);
                    }
                    //Event #2 Початок ремонту і-того об'єкта
                    else if (repairShops[minimalRepairShopIndex.Value].CurrentState is GoToObjectRepairShopState)
                    {
                        indexesNotToTouch = ProcessRepairStart(minimalRepairShopIndex.Value);
                    }
                    //Event #3 Закінчення ремонту і-того об'єкта
                    else if (repairShops[minimalRepairShopIndex.Value].CurrentState is RepairingObjectRepairShopState)
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        indexesNotToTouch = ProcessRepairEnd(minimalRepairShopIndex.Value, timeStep);
                    }
                    //Event #4 ЗПрибуття на свою базу і-тої машини
                    else if (repairShops[minimalRepairShopIndex.Value].CurrentState is BackToStationRepairShopState)
                    {
                        indexesNotToTouch = ProcessBackToStation(minimalRepairShopIndex.Value);
                    }

                    for (int i = 0; i < technicalObjects.Count; i++)
                    {
                        var technicalObject = technicalObjects[i];
                        if (technicalObject.CurrentState is WorkingTechnicalObjectState && !indexesNotToTouch.Item1.Contains(i))
                        {
                            ((WorkingTechnicalObjectState)technicalObject.CurrentState).TimeToBreak -= timeStep;
                        }
                    }

                    for (int i = 0; i < repairShops.Count; i++)
                    {
                        var repairShop = repairShops[i];
                        if (repairShop.CurrentState is IRepairShopStateWithTimeToChange && !indexesNotToTouch.Item2.Contains(i))
                        {
                            ((IRepairShopStateWithTimeToChange)repairShop.CurrentState).TimeLeft -= timeStep;
                        }
                    }

                    time += timeStep;
                }

                var meanIdleTime = idleTimes.Sum() / idleTimes.Count;
                var confirmationDelayTime = confirmationDelayTimes.Sum() / confirmationDelayTimes.Count;

                return (meanIdleTime, confirmationDelayTime);
            }
         
            // ReSharper disable once RedundantCatchClause
            catch (Exception e)
            { throw; }
        }

        #region Events handling

        private (List<int>, List<int>) ProcessObjectsBreaking(int minimalTechnicalObjectIndex, decimal timeStep)
        {
            //here it is still working 
            var technicalObject = technicalObjects[minimalTechnicalObjectIndex];

            technicalObject.CurrentState = new BrokenTechnicalObjectState();
            var breakingTechnicalObjectState = (BrokenTechnicalObjectState)technicalObject.CurrentState;

            var (nearestFreeRepairShopIndex, nearestShopDistance) = GetNearestFreeRepairShopIndexAndDistance(minimalTechnicalObjectIndex);

            //якщо є вільні ремонтні майстерні
            if (nearestFreeRepairShopIndex >= 0)
            {
                confirmationDelayTimes.Add(0);
                breakingTechnicalObjectState.IdleTimeStamp = time + timeStep;

                var nearestFreeRepairShop = repairShops[nearestFreeRepairShopIndex];
                nearestFreeRepairShop.CurrentState = new GoToObjectRepairShopState
                {
                    ObjectToArriveIndex = minimalTechnicalObjectIndex,
                    TimeLeft = nearestShopDistance / _model.MachineSpeed
                };

                return (new List<int> {minimalTechnicalObjectIndex}, new List<int> { nearestFreeRepairShopIndex });
            }
            else
            {
                breakingTechnicalObjectState.ConfirmationDelayTimeStamp = time + timeStep;

                return (new List<int> { minimalTechnicalObjectIndex }, new List<int>());
            }
        }

        private (List<int>, List<int>) ProcessRepairStart(int minimalRepairShopIndex)
        {
            var repairShop = repairShops[minimalRepairShopIndex];
            var technicalObjectIndex = ((GoToObjectRepairShopState)repairShop.CurrentState).ObjectToArriveIndex;

            repairShop.CurrentState = new RepairingObjectRepairShopState
            {
                ObjectToRepairIndex = technicalObjectIndex,
                TimeLeft = _model.RepairDuration
            };

            return (new List<int>(), new List<int> { minimalRepairShopIndex });
        }

        private (List<int>, List<int>) ProcessRepairEnd(int minimalRepairShopIndex, decimal timeStep)
        {
            var repairShop = repairShops[minimalRepairShopIndex];

            var technicalObjectIndex = ((RepairingObjectRepairShopState) repairShop.CurrentState).ObjectToRepairIndex;
            var technicalObject = technicalObjects[technicalObjectIndex];
            var brokenTechnicalObjectState = (BrokenTechnicalObjectState)technicalObject.CurrentState;
            // ReSharper disable once PossibleInvalidOperationException
            idleTimes.Add(time + timeStep - brokenTechnicalObjectState.IdleTimeStamp.Value);
            brokenTechnicalObjectState.IdleTimeStamp = null;

            technicalObject.CurrentState = new WorkingTechnicalObjectState
            {
                TimeToBreak = ExponentialValueGenerator.Get(technicalObject.Intensity)
            };

            var newBrokenTechnicalObjectIndex = GetBrokenTechnicalObjectIndexFromQueue();
            //якщо є зламані об'єкти, то отримуємо найперший із них
            if (newBrokenTechnicalObjectIndex >= 0)
            {
                var newBrokenTechnicalObject = technicalObjects[newBrokenTechnicalObjectIndex];
                // ReSharper disable once PossibleInvalidOperationException
                confirmationDelayTimes.Add(time + timeStep - ((BrokenTechnicalObjectState)newBrokenTechnicalObject.CurrentState).ConfirmationDelayTimeStamp.Value);
                ((BrokenTechnicalObjectState) newBrokenTechnicalObject.CurrentState).IdleTimeStamp = time + timeStep;

                repairShop.CurrentState = new GoToObjectRepairShopState
                {
                    ObjectToArriveIndex = newBrokenTechnicalObjectIndex,
                    TimeLeft = _distanceMatrix.Matrix[technicalObjectIndex, newBrokenTechnicalObjectIndex] / _model.MachineSpeed
                };
            }
            else //їдемо на базу
            {
                repairShop.CurrentState = new BackToStationRepairShopState
                {
                    LastRepairedObjectIndex = technicalObjectIndex,
                    TimeLeft = _distanceMatrix.Matrix[_distanceMatrix.TechnicalObjectsCount + repairShop.RepairStationNumber, technicalObjectIndex] / _model.MachineSpeed
                };
            }

            return (new List<int> { technicalObjectIndex }, new List<int> { minimalRepairShopIndex });
        }

        private (List<int>, List<int>) ProcessBackToStation(int minimalRepairShopIndex)
        {
            var repairShop = repairShops[minimalRepairShopIndex];
            repairShop.CurrentState = new OnStationRepairShopState();

            return (new List<int>(), new List<int> { minimalRepairShopIndex });
        }

        #endregion

        #region Helper methods

        private (int, decimal) GetNearestFreeRepairShopIndexAndDistance(int minimalTechnicalObjectIndex)
        {
            var candidates = repairShops.Where(r => r.CurrentState is OnStationRepairShopState || r.CurrentState is BackToStationRepairShopState).ToList();
            if (candidates.Count == 0)
            {
                return (-1, 0);
            }

            var distances = new List<decimal>();
            foreach (var repairShop in candidates)
            {
                if (repairShop.CurrentState is OnStationRepairShopState)
                {
                    distances.Add(_distanceMatrix.Matrix[_distanceMatrix.TechnicalObjectsCount + repairShop.RepairStationNumber, minimalTechnicalObjectIndex]);
                }
                else if (repairShop.CurrentState is BackToStationRepairShopState)
                {
                    var backToStationState = (BackToStationRepairShopState)repairShop.CurrentState;
                    distances.Add(_model.MachineSpeed * backToStationState.TimeLeft + _distanceMatrix.Matrix[backToStationState.LastRepairedObjectIndex, minimalTechnicalObjectIndex]);
                }
            }

            //індекс елемента з мінімальною відстанню
            var minDistance = distances.Min();
            var index = distances.IndexOf(minDistance);
            return (repairShops.IndexOf(candidates[index]), minDistance);
        }

        private int GetBrokenTechnicalObjectIndexFromQueue()
        {
            var brokenTechnicalObjects = technicalObjects.Where(t => t.CurrentState is BrokenTechnicalObjectState
                                                                     && !repairShops.Exists(r => r.CurrentState is GoToObjectRepairShopState && ((GoToObjectRepairShopState)r.CurrentState).ObjectToArriveIndex == technicalObjects.IndexOf(t) || r.CurrentState is RepairingObjectRepairShopState && ((RepairingObjectRepairShopState)r.CurrentState).ObjectToRepairIndex == technicalObjects.IndexOf(t)))
                .ToList();

            if (brokenTechnicalObjects.Count == 0)
            {
                return -1;
            }

            var confirmationDelays = new List<decimal>();
            foreach (TechnicalObject t in brokenTechnicalObjects)
            {
                // ReSharper disable once PossibleInvalidOperationException
                confirmationDelays.Add(((BrokenTechnicalObjectState)t.CurrentState).ConfirmationDelayTimeStamp.Value);
            }

            //індекс елемента з мінімальним моментом часу
            var index = confirmationDelays.IndexOf(confirmationDelays.Min());
            return technicalObjects.IndexOf(brokenTechnicalObjects[index]);
        }

        private (int?, decimal?) GetMinimalTechnicalObjectData()
        {
            int? minimalTechnicalObjectIndex = null;
            var workingTechnicalObjects = technicalObjects.Where(t => t.CurrentState is WorkingTechnicalObjectState).ToList();
            if (workingTechnicalObjects.Any())
            {
                var minimalTimeToBreak = workingTechnicalObjects.Min(t => (t.CurrentState as WorkingTechnicalObjectState).TimeToBreak);
                for (int i = 0; i < technicalObjects.Count; i++)
                {
                    if (technicalObjects[i].CurrentState is WorkingTechnicalObjectState
                        && (technicalObjects[i].CurrentState as WorkingTechnicalObjectState).TimeToBreak ==
                        minimalTimeToBreak)
                    {
                        minimalTechnicalObjectIndex = i;
                    }
                }

                return (minimalTechnicalObjectIndex, minimalTimeToBreak);
            }

            return (null, null);
        }

        private (int?, decimal?) GetMinimalRepairShopData()
        {
            int? minimalRepairShopIndex = null;
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

                return (minimalRepairShopIndex, minimalTimeLeft);
            }

            return (null, null);
        }

        private (List<TechnicalObject>, List<RepairShop>) SetupInitialStates()
        {
            technicalObjects = _model.TechnicalObjects.Select(to => new TechnicalObject
            {
                Intensity = to.Intensity,
                CurrentState = new WorkingTechnicalObjectState { TimeToBreak = ExponentialValueGenerator.Get(to.Intensity) }
            }).ToList();

            repairShops = new List<RepairShop>();
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

        #endregion


    }
}
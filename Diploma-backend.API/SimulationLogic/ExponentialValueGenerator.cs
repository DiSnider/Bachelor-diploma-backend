using System;

namespace Diploma_backend.API.SimulationLogic
{
    public static class ExponentialValueGenerator
    {
        private static Random _random;

        static ExponentialValueGenerator()
        {
            _random = new Random();
        }

        public static decimal Get(decimal lambda)
        {
            return -1 / lambda * (decimal)Math.Log(_random.NextDouble());
        }
    }
}
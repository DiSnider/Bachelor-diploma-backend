using Diploma_backend.API.Models.Google;

namespace Diploma_backend.API.Models
{
    public class DistanceMatrix
    {
        public DistanceMatrix(DistanceMatrixResponse googleMatrix, int technicalObjectsCount)
        {
            TechnicalObjectsCount = technicalObjectsCount;

            var n = googleMatrix.destination_addresses.Length;
            Matrix = new int[n, n];

            for (int i = 0; i < googleMatrix.rows.Length; i++)
            {
                var row = googleMatrix.rows[i];
                for (int j = 0; j < row.elements.Length; j++)
                {
                    var element = row.elements[j];
                    Matrix[i, j] = element.distance.value;
                }
            }
        }

        public int[,] Matrix { get; private set; }

        public int TechnicalObjectsCount { get; }

        public int RepairStationsCount => Dimension - TechnicalObjectsCount;

        public int Dimension => Matrix.GetLength(0);
    }
}
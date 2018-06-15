namespace Diploma_backend.API.Models.Google
{
    public class DistanceMatrixRowElement
    {
        public DistanceMatrixDistance distance { get; set; }
        public DistanceMatrixDuration duration { get; set; }
        public string status { get; set; }
    }
}
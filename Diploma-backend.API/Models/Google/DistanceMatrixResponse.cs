namespace Diploma_backend.API.Models.Google
{
    public class DistanceMatrixResponse
    {
        public string[] destination_addresses { get; set; }
        public string[] origin_addresses { get; set; }
        public DistanceMatrixRow[] rows { get; set; }
        public string status { get; set; }
    }
}
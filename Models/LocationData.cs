using SQLite;

namespace LocationTracker.Models
{
    // Represents a single recorded location
    public class LocationData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public DateTime Timestamp { get; set; }
    }
}

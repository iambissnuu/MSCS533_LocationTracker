using SQLite;
using LocationTracker.Models;

namespace LocationTracker.Services
{
    // Handles saving and loading locations from SQLite
    public class LocationDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public LocationDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<LocationData>().Wait();
        }

        public Task<List<LocationData>> GetLocationsAsync()
        {
            return _database.Table<LocationData>().ToListAsync();
        }

        public Task<int> SaveLocationAsync(LocationData location)
        {
            return _database.InsertAsync(location);
        }

        public Task<int> DeleteAllAsync()
        {
            return _database.DeleteAllAsync<LocationData>();
        }
    }
}

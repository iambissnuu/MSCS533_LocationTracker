using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using LocationTracker.Models;
using LocationTracker.Services;

namespace LocationTracker;

public partial class MainPage : ContentPage
{
    private readonly LocationDatabase _database;
    private readonly LocationService _locationService;
    private bool _isTracking;
    private LocationData? _lastLocation;

    public MainPage()
    {
        InitializeComponent();

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
        _database = new LocationDatabase(dbPath);
        _locationService = new LocationService();

        LoadPreviousLocations(); // load any previously saved locations
    }

    private async void OnStartTrackingClicked(object sender, EventArgs e)
    {
        var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (permission != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission Denied", "Location access is required to track movement.", "OK");
            return;
        }

        _isTracking = !_isTracking;
        var button = (Button)sender;

        if (_isTracking)
        {
            button.Text = "Stop Tracking";
            await TrackLocationAsync();
        }
        else
        {
            button.Text = "Start Tracking";
        }
    }

    private async Task TrackLocationAsync()
    {
        while (_isTracking)
        {
            var current = await _locationService.GetCurrentLocationAsync();
            if (current != null)
            {
                await _database.SaveLocationAsync(current);
                DrawHeatPoint(current);
            }
            await Task.Delay(5000); // sample every 5 seconds
        }
    }

    private async void LoadPreviousLocations()
    {
        var locations = await _database.GetLocationsAsync();

        if (locations.Count > 0)
        {
            foreach (var loc in locations)
                DrawHeatPoint(loc);

            var last = locations.Last();
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(last.Latitude, last.Longitude),
                Distance.FromMeters(500)));
        }
    }

    private void DrawHeatPoint(LocationData location)
    {
        var pos = new Location(location.Latitude, location.Longitude);

        // Determine color based on proximity (heat intensity)
        Color pointColor = Colors.Blue;
        if (_lastLocation != null)
        {
            double distance = Location.CalculateDistance(
                _lastLocation.Latitude, _lastLocation.Longitude,
                location.Latitude, location.Longitude,
                DistanceUnits.Kilometers);

            if (distance < 0.02)
                pointColor = Colors.Red;        // Hot zone (slow or dense area)
            else if (distance < 0.05)
                pointColor = Colors.Orange;     // Warm zone
            else if (distance < 0.1)
                pointColor = Colors.Yellow;     // Mild zone
        }

        // Create a small “heat dot”
        var radius = 0.00008; // smaller = more precise
        var circle = new Polygon
        {
            StrokeColor = pointColor,
            FillColor = pointColor.WithAlpha(0.4f),
            StrokeWidth = 0
        };

        // 4 small corner points to form a dot
        circle.Geopath.Add(new Location(pos.Latitude + radius, pos.Longitude));
        circle.Geopath.Add(new Location(pos.Latitude, pos.Longitude + radius));
        circle.Geopath.Add(new Location(pos.Latitude - radius, pos.Longitude));
        circle.Geopath.Add(new Location(pos.Latitude, pos.Longitude - radius));

        MyMap.MapElements.Add(circle);

        // Draw a connecting path (polyline)
        if (_lastLocation != null)
        {
            var line = new Polyline
            {
                StrokeColor = Colors.Blue.WithAlpha(0.5f),
                StrokeWidth = 4
            };
            line.Geopath.Add(new Location(_lastLocation.Latitude, _lastLocation.Longitude));
            line.Geopath.Add(new Location(location.Latitude, location.Longitude));
            MyMap.MapElements.Add(line);
        }

        _lastLocation = location;
    }
}

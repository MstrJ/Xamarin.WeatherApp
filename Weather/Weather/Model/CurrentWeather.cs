namespace Weather.Model
{
    //https://api.openweathermap.org/data/2.5/weather?q={city name}&appid={API key}
    public class CurrentWeather
    {
        public int lon { get; set; }
        public int lat { get; set; }
        public string city { get; set; } = "";
    }
}
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Weather.Model;

namespace Weather.Service
{
    public interface IWeatherService
    {
        ObservableCollection<WeatherInfoFull> GetWeatherInfoFull();
        Task<(T,bool)> GetValueAsync<T>(string url);
        Task<WeatherObject> GetCurrentWeather();
        Task SetCurrentWeather(string url);
        void SetWeatherInfoFull();
        bool GetNetwork();
        string ChangeBackground(WeatherObject obj);
        string getKey();
        string GetDayPL(string dayEng);
        WeatherObjectDB GetFromDB();
        void AddandRemoveToDB(WeatherObjectDB obj);
        Task<(WeatherObject,string)> GetLocation();
    }
}
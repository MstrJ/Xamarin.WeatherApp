using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Android.Locations;
using Newtonsoft.Json;
using SQLite;
using Weather.Model;
using Weather.Service;
using Xamarin.Essentials;
using Xamarin.Forms.PlatformConfiguration;
using Android.Content;
using static Weather.Model.Weather5DaysObject;
using Location = Xamarin.Essentials.Location;
using Android.App;
using System.Threading;

namespace Weather.Service
{
    [Service]
    public class WeatherService : IWeatherService
    {
        private WeatherObject _currentWeather { get; set; }
        private bool _network { get; set ; }
        private string _key { get; set; } = "secret";
        private SQLiteConnection _db { get; set; }
        private ObservableCollection<WeatherInfoFull> _weatherInfoFull = new ObservableCollection<WeatherInfoFull>();

        public WeatherService()
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
            var db = new SQLiteConnection(databasePath);
            db.CreateTable<WeatherObjectDB>();
            _db = db;
        }

        public WeatherObjectDB GetFromDB()
        {
            var obj = _db.Table<WeatherObjectDB>().FirstOrDefault();
            return obj;
        }
        public void AddandRemoveToDB(WeatherObjectDB obj)
        {
            if (obj == null) return;
            var last = _db.Table<WeatherObjectDB>().OrderByDescending(x => x.id).FirstOrDefault();
            if(last != null)
            {
                _db.Delete(last);
            }
            _db.Insert(obj);
        }

        public string getKey()
        {
            return _key;
        }
        public string ChangeBackground(WeatherObject obj)
        {
            string v = string.Empty;
            var clouds = obj.clouds.all;

            switch (obj.weather.First().main)
            {
                case "Clear":
                    v = "clearSunnyday";
                    break;
                case "Snow":
                    v = "snowy";
                    break;
                case "Rain":
                    v = "raining";
                    break;
                case "Drizzle":
                    v = "raining";
                    break;
                case "Thunderstorm":
                    v = "thunder";
                    break;
                default:
                    if (clouds > 11 && clouds < 25)
                    {
                        v = "littleCloudySunny";
                    }
                    else if (clouds > 25 && clouds < 50)
                    {
                        v = "littleCloudySunny";
                    }
                    else if (clouds > 51)
                    {
                        v = "sunnyCloudy_Day";
                    }
                    break;
            };
            return v;

        }

        public bool GetNetwork()
        {
            return _network;
        }
        public void SetNetwork(bool n)
        {
            _network = n;
        }
        public async Task<WeatherObject> GetCurrentWeather()
        {
            var w = await Task.FromResult(_currentWeather);
            return w;
        }
        public async Task SetCurrentWeather(string url)
        {
            var currentW = await Task.FromResult(GetValueAsync<WeatherObject>(url));
            currentW.Wait();
            var cw = await currentW;
            if (currentW.Result.Item1 == null) return;
            _network = cw.Item2;
            _currentWeather = cw.Item1;
        }

        public ObservableCollection<WeatherInfoFull> GetWeatherInfoFull()
        {
            return _weatherInfoFull;
        }


        public async void SetWeatherInfoFull()
        {

            string url = $"https://api.openweathermap.org/data/2.5/forecast?q={_currentWeather.name}&appid={_key}&lang=pl";

            var daysWeather = await Task.FromResult(GetValueAsync<Weather5DaysObject.Rootobject>(url)).Result;
            if (daysWeather.Item1 == null) return;
            _weatherInfoFull.Clear();


            string[] days = new string[3] { "Dzisiaj", "Jutro", GetDayPL(DateTime.Now.AddDays(2).DayOfWeek.ToString()) };


            int iday = 0;
            bool isToday = true;
            double tempMaxToday = 0;
            double tempMinToday = 0;
            string[] iconToday = new string[8];
            string[] descToday = new string[8];
            while (isToday)
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds(daysWeather.Item1.list[iday].dt);
                var h = dt.Hour;
                if (h == 0) isToday = false;

                float temp = (daysWeather.Item1.list[iday].main.temp) - 273;
                if (temp > tempMaxToday) tempMaxToday = temp;
                if (temp < tempMinToday) tempMinToday = temp;
                iconToday[iday] = daysWeather.Item1.list[iday].weather.First().icon;
                descToday[iday] = daysWeather.Item1.list[iday].weather.First().description;
                iday++;
            }
            string finalIconToday = iconToday.Where(x=> x!=null).GroupBy(x => x)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;
            string finalDescToday = descToday.Where(x=> x!=null).GroupBy(x => x)
                                .OrderByDescending(g => g.Count())
                                .First()
                                .Key;
            //if (iday == 0) iday = 1;
            WeatherInfoFullAdd(days[0], finalIconToday, finalDescToday, tempMaxToday, tempMinToday);

            for (int j = 0; j < 2; j++)
            {
                float tempMax = float.MinValue;
                float tempMin = float.MaxValue;
                string[] icon = new string[8];
                string[] desc = new string[8];

                for (int i = iday + j * 8; i < iday+8 + j * 8; i++)
                {
                    var dt = DateTimeOffset.FromUnixTimeSeconds(daysWeather.Item1.list[i].dt);
                    var h = dt.Hour;

                    float temp = (daysWeather.Item1.list[i].main.temp) - 273;
                    if (temp > tempMax) tempMax = temp;
                    if (temp < tempMin) tempMin = temp;
                    icon[i - j * 8-iday] = daysWeather.Item1.list[i].weather.First().icon;
                    desc[i - j * 8-iday] = daysWeather.Item1.list[i].weather.First().description;
                }
                var finalIcon = icon.Where(x => x != null).GroupBy(x => x)
                                    .OrderByDescending(g => g.Count())
                                    .First()
                                    .Key;
                var finalDesc = desc.Where(x => x != null).GroupBy(x => x)
                                    .OrderByDescending(g => g.Count())
                                    .First()
                                    .Key;


                WeatherInfoFullAdd(days[j+1], finalIcon, finalDesc, tempMax, tempMin);

            }
        }
        public void WeatherInfoFullAdd(string day,string img,string weathername,double maxTemp, double minTemp)
        {
            if(Math.Round(maxTemp)==Math.Round(minTemp))  _weatherInfoFull.Add(new WeatherInfoFull { Day = day, Img = $"https://openweathermap.org/img/wn/{img}@2x.png", WeatherName = weathername, Temp = $"{Math.Round(maxTemp)}°" });

            else _weatherInfoFull.Add(new WeatherInfoFull { Day = day, Img = $"https://openweathermap.org/img/wn/{img}@2x.png", WeatherName = weathername, Temp = $"{Math.Round(maxTemp * 1) / 1}° / {Math.Round(minTemp * 1) / 1}°" });
        }

        public string GetDayPL(string dayEng)
        {
            switch (dayEng)
            {
                case "Monday":
                    return "Pon";             
                case "Tuesday":
                    return "Wt";            
                case "Wednesday":
                    return "Śr";       
                case "Thursday":
                    return "Czw";
                case "Friday":
                    return "Pt";
                case "Saturday":
                    return "Sob";
                default:
                    return "Ndz";
            }
        }

        public async Task<(WeatherObject,string)> GetLocation()
        {
            Location location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
            {
                location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Low,
                    Timeout = TimeSpan.FromSeconds(10)
                });
            }
            var Lat = ChangeCharToDot(location.Latitude);
            var Long = ChangeCharToDot(location.Longitude);

            string uri = $"https://api.openweathermap.org/data/2.5/weather?lat={Lat}&lon={Long}&appid={_key}&lang=pl";
            var w = await GetValueAsync<WeatherObject>(uri);
            return (w.Item1,uri);
        }
        private string ChangeCharToDot(double value)
        {
            return value.ToString().Replace(",", ".");
        }

        public async Task<(T,bool)> GetValueAsync<T>(string url)
        {
            bool network = true;
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                network = false;
                return (default(T),network);
            }
            using (var client = new HttpClient
            {
                BaseAddress = new Uri(url),
                Timeout = TimeSpan.FromSeconds(30)
            })
            {
                try
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(30));
                    var response = await client.GetAsync(client.BaseAddress,cts.Token);
                 
                    if (response.IsSuccessStatusCode)
                    {
                        var r =await  response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<T>(r);

                        return (result,network);

                    }
                    else
                    {
                        return (default(T),network);
                    }


                }
                catch (AggregateException ex) when (ex.InnerException is HttpRequestException)
                {
                    return (default(T),network);
                }
            };
        }

    }
}

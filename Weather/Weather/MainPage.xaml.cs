using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Locations;
using Weather.Model;
using Weather.Service;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace Weather
{
    public partial class MainPage : ContentPage
    {
        
        private IWeatherService _weatherService { get; set; } = Factory.CreateWeatherService();
        private bool _isLocationAvailable { get; set; } = false;
        public MainPage()
        {
            InitializeComponent();


            Task.Run(async () =>
            {
                await IsLocationEnabled();
            });
            DoDBorLocation();
            BindingContext = new MyViewModel("clearSunnyday.jpg",!_isLocationAvailable);
            setColors();


        }
        public async Task IsLocationEnabled()
        {
            while (true)
            {
                bool last = _isLocationAvailable;
                Context context = Android.App.Application.Context;
                var locationManager = (LocationManager)context.GetSystemService(Context.LocationService);

                if (locationManager != null && (locationManager.IsProviderEnabled(LocationManager.GpsProvider) || locationManager.IsProviderEnabled(LocationManager.NetworkProvider)))
                {
                    _isLocationAvailable = true;
                }
                else
                {
                    _isLocationAvailable = false;
                }
                if (last != _isLocationAvailable)
                {

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var currentWeatherTask = _weatherService.GetCurrentWeather();
                        var currentWeather = await currentWeatherTask;
                        if (currentWeather != null)
                        {
                            DoDBorLocation();
                        }
                    });
                }
                await Task.Delay(1000);

            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
        private void SetInfo(WeatherObject obj,string url)
        {

            bool isLocation = false;
            if (url.ToLower().Contains("lat")) isLocation = true;
            if (obj == null) return;
            var weather =obj;
            if (url != null)
            {
                _weatherService.AddandRemoveToDB(new WeatherObjectDB { cityURL = url,isLocation=isLocation });
            }
            _weatherService.SetWeatherInfoFull();
            //
            LCity.Text = $"{weather.name}"; 
            LTemp.Text = $"{Math.Round(weather.main.temp - 273.15)}";
            LTempType.Text = $"°";
            Ldescription.Text = $"{weather.weather.First().description}";
            var dataT = DateTime.Now;
            LShortInfo.Text = $"{dataT.Day} {_weatherService.GetDayPL(dataT.DayOfWeek.ToString())} {Math.Round(weather.main.temp_max - 273.15)}° / {Math.Round(weather.main.temp_min - 273.15)}°";
            LHumidity.Text = $"{weather.main.humidity} %";
            LWindSpeed.Text = $"{weather.wind.speed} m/s ";
            LPressure.Text = $"{weather.main.pressure} hPa";
            listView.ItemsSource = _weatherService.GetWeatherInfoFull();
            ChangeBg(weather,isLocation);
        }
        private void ChangeBg(WeatherObject obj,bool isLocation)
        {
            BindingContext = new MyViewModel($"{_weatherService.ChangeBackground(obj)}.jpg",isLocation); 
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            string btnTxt = searchBar.Text;
            if (string.IsNullOrEmpty(btnTxt)) return;
            var temp = await _weatherService.GetCurrentWeather();
            var currentName = temp != null ? temp : null;
            if (currentName != null)
            {
                if (currentName.name.ToLower().Equals(btnTxt.ToLower()))
                {
                    return;
                }
            }
            SetByCity();
        }

        public void SetByCity()
        {
            string urlCity = $"https://api.openweathermap.org/data/2.5/weather?q={searchBar.Text}&appid={_weatherService.getKey()}&lang=pl";
            var network = _weatherService.GetNetwork();
            if (!network)
            {
                NetworkAlert();
            }
            CurrentLink(urlCity);
        }

        public async void CurrentLink(string url)
        {
            activityIndicator.IsRunning = true;

            if (url == null) return;
            await Task.Run(() => _weatherService.SetCurrentWeather(url));
            var w = await Task.FromResult(_weatherService.GetCurrentWeather());
            if (w.IsCompleted)
            {
                activityIndicator.IsRunning = false;
            }
            var network = _weatherService.GetNetwork();
            if (!network)
            {
                NetworkAlert();
            }
            var tweather = w.Result;
            if (tweather == null) return;
            SetInfo(tweather, url);

        }
        public async void NetworkAlert()
        {
            await DisplayAlert("Connection Issue", "No network connection", "OK");
            return;
        }

        public void DataFromDB()
        {
            var dataFromDB = _weatherService.GetFromDB();
            if (dataFromDB != null) CurrentLink(dataFromDB.cityURL);
        }

        private void listView_Refreshing(object sender, EventArgs e)
        {
            DoDBorLocation();
            listView.IsRefreshing = false;
        }

        private async void DoDBorLocation()
        {
            if (!_isLocationAvailable)
            {
                DataFromDB();
            }
            else
            {
                var currentWeatherTask = _weatherService.GetLocation();
                var currentWeather = await currentWeatherTask;
                CurrentLink(currentWeather.Item2);
            }
        }

        private async void imgButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new InfoPage());
        }

        private void setColors()
        {
            searchFrame.BackgroundColor = Color.FromRgba(10, 10, 10, 0.3);
            listView.BackgroundColor = Color.FromRgba(0, 0, 0, 0.3);
        }
    }
}

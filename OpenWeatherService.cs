using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using System.Collections.Concurrent;


using Assignment_A1_01.Models;

namespace Assignment_A1_01.Services
{
    public class OpenWeatherService
    {
        HttpClient httpClient = new HttpClient();

        //Cache declaration
        ConcurrentDictionary<(double, double, string), Forecast> cachedGeoForecasts = new ConcurrentDictionary<(double, double, string), Forecast>();
        ConcurrentDictionary<(string, string), Forecast> cachedCityForecasts = new ConcurrentDictionary<(string, string), Forecast>();

        // Your API Key
        readonly string apiKey = "51672c3748bc566b99e3945223f4f78a";

        //Event declaration
        public event EventHandler<string> WeatherForecastAvailable;
        protected virtual void OnWeatherForecastAvailable(string message)
        {
            WeatherForecastAvailable?.Invoke(this, message);
        }
        public async Task<Forecast> GetForecastAsync(string City)
        {

            //part of cache code here to check if forecast in Cache
            //generate an event that shows forecast was from cache

            https://openweathermap.org/current
            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={City}&units=metric&lang={language}&appid={apiKey}";

            var Cities = City;
            var date = DateTime.Now.ToString("yyyy, MM, dd: HH:mm");
            DateTime timestamp = DateTime.Now;
            var key = (Cities, date);

            if (!cachedCityForecasts.TryGetValue(key, out var forecast) && (timestamp.AddMinutes(1) > DateTime.Now))
            {
                forecast = await ReadWebApiAsync(uri);
                cachedCityForecasts[key] = forecast;
                WeatherForecastAvailable?.Invoke(forecast, $"New weather forecast for {City} available");
            }
            else
            WeatherForecastAvailable?.Invoke(forecast, $"Cached weather forecast for {City} available");
            return forecast;
        }
        public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
        {

            var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={apiKey}";
            var longit = longitude;
            var latit = latitude;
            var date = DateTime.Now.ToString("yyyy, MM, dd: HH:mm");
            DateTime timestamp = DateTime.Now;
            var key = (longit, latit, date);


            if (!cachedGeoForecasts.TryGetValue(key, out var forecast) && (timestamp.AddMinutes(1) > DateTime.Now))
                //if (!cachedGeoForecasts.TryGetValue(key, out var forecast))
                {
                    forecast = await ReadWebApiAsync(uri);
                    cachedGeoForecasts[key] = forecast;
                    WeatherForecastAvailable?.Invoke(forecast, $"New weather forecast for {latitude}, {longitude} available");
                }
            else
            WeatherForecastAvailable?.Invoke(forecast, $"Cached weather forecast for {latitude} {longitude} available");
                return forecast;


        }
        private async Task<Forecast> ReadWebApiAsync(string uri)
        {
            //Read the response from the WebApi
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            WeatherApiData wd = await response.Content.ReadFromJsonAsync<WeatherApiData>();

            var forecast = new Forecast
            {
                City = wd.city.name,
                Items = wd.list.Select(f => new ForecastItem
                {
                    Temperature = f.main.temp,
                    Description = f.weather[0].description,
                    WindSpeed = f.wind.speed,
                    DateTime = UnixTimeStampToDateTime(f.dt)


                }).ToList()
            };

            return forecast;
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }


    }
}

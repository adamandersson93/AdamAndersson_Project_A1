using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Concurrent;


using Assignment_A1_01.Models;
using Assignment_A1_01.Services;

namespace Assignment_A1_01
{
    class Program
    {
        static async Task Main(string[] args)
        {

            
            OpenWeatherService service = new OpenWeatherService();
            service.WeatherForecastAvailable += WeatherReportDataAvailable;

            //Register the event
            //Your Code

            Task<Forecast>[] tasks = { null, null, null, null };
            Exception exception = null;
            try
            {

                double latitude = 60.079444;
                double longitude = 17.739722;

                //Create the two tasks and wait for comletion
                tasks[0] = service.GetForecastAsync(latitude, longitude);
                tasks[1] = service.GetForecastAsync("Phoenix");

                Task.WaitAll(tasks[0], tasks[1]);

                tasks[2] = service.GetForecastAsync(latitude, longitude);
                tasks[3] = service.GetForecastAsync("Phoenix");

                //Wait and confirm we get an event showing cahced data avaialable
                Task.WaitAll(tasks[2], tasks[3]);

                Forecast forecast = await new OpenWeatherService().GetForecastAsync(latitude, longitude);
                Forecast forecastCity = await new OpenWeatherService().GetForecastAsync("Phoenix");
                Console.WriteLine();

                Console.WriteLine($"Weather forecast for {forecast.City}");
                var groupedWeatherList = forecast.Items.GroupBy(x => x.DateTime.Date.DayOfWeek, x => x).Distinct().ToList();

                foreach (var forcast in groupedWeatherList)
                {
                    Console.WriteLine(forcast.Key);
                    forcast.ToList().ForEach(x => Console.WriteLine($"- • {x.DateTime.TimeOfDay} {x.Description} Temp: {x.Temperature} C°, Wind: {x.WindSpeed} m/s"));
                }
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------------------------------");

                Console.WriteLine($"Weather forecast for {forecastCity.City}");
                var groupedWeatherListCity = forecastCity.Items.GroupBy(x => x.DateTime.Date.DayOfWeek, x => x).Distinct().ToList();

                foreach (var forcastCity in groupedWeatherListCity)
                {
                    Console.WriteLine(forcastCity.Key);
                    forcastCity.ToList().ForEach(x => Console.WriteLine($"- • {x.DateTime.TimeOfDay} {x.Description} Temp: {x.Temperature} C°, Wind: {x.WindSpeed} m/s"));
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                Console.WriteLine(ex.Message);
            }

            foreach (var task in tasks)
            {
                if (task != null && task.IsFaulted)
                {
                    Console.WriteLine("Error, the task didn't finish!");
                }
                if(task != null && task.IsCompleted)
                {
                    Console.WriteLine("Task completed!");
                }
            }
            void WeatherReportDataAvailable(object sender, string message)
            {
                Console.WriteLine($"Event message from weather service: {message}");
            }
        }
    }
}

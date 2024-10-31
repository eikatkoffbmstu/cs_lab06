
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

public struct Weather
{
    public string Country { get; set; }
    public string Name { get; set; }
    public float Temp { get; set; }
    public string Description { get; set; }
}

class Program
{
    private static readonly HttpClient httpClient = new HttpClient();
    private const string ApiKey = "c53ba5721e341db421d4f4931c3cf695";
    static async Task Main(string[] args)
    {
        List<Weather> weatherData = new List<Weather>();
        Random random = new Random();

        while (weatherData.Count < 25)
        {
            float lat = (float)(random.NextDouble() * 180 - 90);
            float lon = (float)(random.NextDouble() * 360 - 180);

            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={ApiKey}&units=metric";
            var weather = await GetWeatherData(url);
            
            if (weather.HasValue 
                && !string.IsNullOrEmpty(weather.Value.Country) 
                && !string.IsNullOrEmpty(weather.Value.Name))
            {
                weatherData.Add(weather.Value);
                Console.WriteLine($"Добавлен элемент {weatherData.Count}");
            }
        }

        var maxTempCountry = weatherData.OrderByDescending(w => w.Temp).FirstOrDefault();
        var minTempCountry = weatherData.OrderBy(w => w.Temp).FirstOrDefault();
        
        Console.WriteLine($"Страна с максимальной температурой: {maxTempCountry.Country}, Температура: {maxTempCountry.Temp}°C");
        Console.WriteLine($"Страна с минимальной температурой: {minTempCountry.Country}, Температура: {minTempCountry.Temp}°C");

        var averageTemp = weatherData.Average(w => w.Temp);
        Console.WriteLine($"Средняя температура в мире: {averageTemp}°C");

        var uniqueCountriesCount = weatherData.Select(w => w.Country).Distinct().Count();
        Console.WriteLine($"Количество уникальных стран: {uniqueCountriesCount}");

        var descriptions = new[] { "clear sky", "rain", "few clouds" };
        var firstMatch = weatherData.FirstOrDefault(w => descriptions.Contains(w.Description));
        
        if (!string.IsNullOrEmpty(firstMatch.Name))
        {
            Console.WriteLine($"Первая найдетая страна с описанием: {firstMatch.Country}, Местность: {firstMatch.Name}, Описание: {firstMatch.Description}");
        }
        else
        {
            Console.WriteLine("Нет найденных описаний.");
        }
    }

    private static async Task<Weather?> GetWeatherData(string url)
    {
        try
        {
            Console.WriteLine($"Запрос к URL: {url}");
            var response = await httpClient.GetFromJsonAsync<WeatherResponse>(url);

            if (response != null 
                && response.Main != null 
                && response.Sys != null 
                && response.Weather != null 
                && response.Name != ""
                && response.Weather.Count > 0)
            {
                return new Weather
                {
                    Country = response.Sys.Country ?? "Неизвестно",
                    Name = response.Name ?? "Неизвестно",
                    Temp = response.Main.Temp,
                    Description = response.Weather[0].Description
                };
            }
            else
            {
                Console.WriteLine("Ответ API не содержит необходимых данных.");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка запроса: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Общая ошибка: {ex.Message}");
        }
        return null;
    }
}

public class WeatherResponse
{
    public Main? Main { get; set; }
    public Sys? Sys { get; set; }
    public List<WeatherDescription>? Weather { get; set; }
    public string? Name { get; set; }
}

public class Main
{
    public float Temp { get; set; }
}

public class Sys
{
    public string Country { get; set; }
}

public class WeatherDescription
{
    public string Description { get; set; }
}

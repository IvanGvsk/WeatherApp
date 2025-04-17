using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly string _apiKey = "your_key"; // заменить на свой ключ
        private readonly List<string> _days = new List<string>
        {
            "Сегодня", "Завтра",
            DateTime.Today.AddDays(2).ToShortDateString(),
            DateTime.Today.AddDays(3).ToShortDateString()
        };
        private readonly List<string> _periods = new List<string>
        {
            "Утро", "День", "Вечер"
        };

        public MainWindow()
        {
            InitializeComponent();

            day_cb.ItemsSource = _days;
            time_cb.ItemsSource = _periods;

            _ = LoadAndDisplayForecastAsync();
        }

        private async void OnDateOrTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadAndDisplayForecastAsync();
        }

        private async void OnRefreshClicked(object sender, RoutedEventArgs e)
        {
            await LoadAndDisplayForecastAsync();
        }

        private async Task LoadAndDisplayForecastAsync()
        {
            if (string.IsNullOrWhiteSpace(Search_tb.Text))
            {
                Search_tb.Text = "Минск";
            }

            string city = Search_tb.Text.Trim();
            int dayIndex = day_cb.SelectedIndex;  
            int periodIndex = time_cb.SelectedIndex; 

            DateTime targetDate = DateTime.Today.AddDays(dayIndex);
            int targetHour = periodIndex switch
            {
                0 => 6,   // утро
                1 => 12,  // день
                2 => 18,  // вечер
                _ => 12
            };

            string url =
              $"https://api.openweathermap.org/data/2.5/forecast" +
              $"?q={city}&appid={_apiKey}&units=metric&lang=ru";

            ForecastResponse resp;
            try
            {
                var json = await _http.GetStringAsync(url);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                resp = JsonSerializer.Deserialize<ForecastResponse>(json, options);
                if (resp?.list == null)
                    throw new Exception("Пустой ответ от сервера");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось получить прогноз: {ex.Message}");
                return;
            }

            int timezoneOffsetSec = resp.city.timezone;

            var slot = resp.list
                .Select(item => new {
                    Data = item,
                    Dt = DateTime.Parse(item.dt_txt).AddSeconds(timezoneOffsetSec)
                })
                .Where(x => x.Dt.Date == targetDate.Date)
                .OrderBy(x => Math.Abs(x.Dt.Hour - targetHour))
                .FirstOrDefault()?.Data;

            if (slot == null)
            {
                MessageBox.Show("Нет данных на выбранную дату/время.");
                return;
            }

            TemperatureToday_tb.Text = $"{slot.main.temp:F1}°C, {slot.weather[0].description}";
            HumidityToday_tb.Text = $"Влажность: {slot.main.humidity}%";
            WindToday_tb.Text = $"{slot.wind.speed:F1} м/с";
            FeelsLike_tb.Text = $"Ощущается как {slot.main.feels_like:F1}°C";
            Pressure_tb.Text = $"Давление: {slot.main.pressure} гПа";

            string icon = slot.weather[0].icon;
            var bmp = new BitmapImage(new Uri($"https://openweathermap.org/img/wn/{icon}@2x.png"));
            WeatherToday_img.Source = bmp;

            this.Title = $"Погода в {resp.city.name} на {slot.dt_txt}";
        }
    }
}

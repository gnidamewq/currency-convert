using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace fullgnidis
{

    public partial class MainWindow : Window
    {
        private double currentRate = 0.0;
        private Dictionary<string, double> rates = new Dictionary<string, double>();
        private Dictionary<string, string> currencyMap = new()
        {
            { "Доллары", "USD" },
            { "Евро", "EUR" },
            { "Йены", "JPY" },
            { "Фунты", "GBP" },
            { "Юани", "CNY" }
        };
        private Dictionary<string, string> currencySymbols = new()
        {
            { "Доллары", "$" },
            { "Евро", "€" },
            { "Фунты", "£" },
            { "Юани", "¥" },
            { "Йены", "¥" }
        };
        public MainWindow()
        {
            InitializeComponent();
            
        }
        public async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            await LoadRatesAsync();
        }

        private async Task LoadRatesAsync()
        {
            using HttpClient client = new HttpClient();
            string url = "https://v6.exchangerate-api.com/v6/2e448322beb2a324a536533c/latest/RUB";

            try
            {
                var response = await client.GetStringAsync(url);
                var data = JObject.Parse(response);

                if (data["result"]?.ToString() == "success")
                {
                    var rateData = data["conversion_rates"];
                    foreach (var (ruName, code) in currencyMap)
                    {
                        double? rate = rateData?[code]?.Value<double>();
                        if (rate != null)
                        {
                            rates[ruName] = rate.Value;
                        }
                        else
                        {
                            MessageBox.Show($"Курс для {ruName} ({code}) не найден.");
                            rates[ruName] = 0.0;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось получить курсы валют.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении курсов:\n{ex.Message}");
            }
        }

        private async void SelectCur_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rates.Count == 0)
            {
                MessageBox.Show("Курсы валют еще не загружены.");
                return;
            }

            if (SelectCur.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedCurrency = selectedItem.Content.ToString();
                if (rates.TryGetValue(selectedCurrency, out double rate))
                {
                    currentRate = rate;
                }
                else
                {
                    MessageBox.Show("Выбранная валюта не поддерживается.");
                    currentRate = 0.0;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(CurAmount.Text, out double amount))
            {
                double Answer = amount * currentRate;

                if (SelectCur.SelectedItem is ComboBoxItem selectedItem)
                {
                    string selectedCurrency = selectedItem.Content.ToString();

                    if (currencySymbols.TryGetValue(selectedCurrency, out string symbol))
                    {
                        RubAmount.Text = $"{Answer:F2} {symbol}";
                    }
                    else
                    {
                        RubAmount.Text = $"{Answer:F2}";
                    }
                }
            }
            else
            {
                RubAmount.Text = "Ошибка ввода";
            }
        }
        private void ReverseButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(CurAmount.Text, out double amount) && currentRate > 0)
            {
                double result = amount / currentRate;
                RubAmount.Text = $"{result:F2} ₽";
            }
            else
            {
                RubAmount.Text = "Некорректное значение или не выбрана валюта.";
            }
        }
    }
}
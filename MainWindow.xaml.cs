using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NorthwestMethod
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int suppliers = 0;
        private int consumers = 0;

        // Списки для хранения текстбоксов (затраты, запасы, потребности)
        private TextBox[,] costTextBoxes;
        private TextBox[] supplyTextBoxes;
        private TextBox[] demandTextBoxes;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateTable_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SuppliersTextBox.Text, out suppliers) || !int.TryParse(ConsumersTextBox.Text, out consumers))
            {
                MessageBox.Show("Введите корректные целые числа для поставщиков и потребителей.");
                return;
            }

            InputGrid.Children.Clear();
            InputGrid.RowDefinitions.Clear();
            InputGrid.ColumnDefinitions.Clear();

            costTextBoxes = new TextBox[suppliers, consumers];
            supplyTextBoxes = new TextBox[suppliers];
            demandTextBoxes = new TextBox[consumers];

            // Создание строк и колонок
            for (int i = 0; i < suppliers + 2; i++) InputGrid.RowDefinitions.Add(new RowDefinition());
            for (int j = 0; j < consumers + 2; j++) InputGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // Заголовки
            for (int j = 0; j < consumers; j++)
            {
                TextBlock tb = new TextBlock
                {
                    Text = $"C{j + 1}",
                    Margin = new Thickness(5),
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(tb, 0);
                Grid.SetColumn(tb, j + 1);
                InputGrid.Children.Add(tb);
            }

            for (int i = 0; i < suppliers; i++)
            {
                TextBlock tb = new TextBlock
                {
                    Text = $"S{i + 1}",
                    Margin = new Thickness(5),
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(tb, i + 1);
                Grid.SetColumn(tb, 0);
                InputGrid.Children.Add(tb);
            }

            // Ввод затрат
            for (int i = 0; i < suppliers; i++)
            {
                for (int j = 0; j < consumers; j++)
                {
                    TextBox tb = new TextBox { Width = 40, Margin = new Thickness(2) };
                    costTextBoxes[i, j] = tb;
                    Grid.SetRow(tb, i + 1);
                    Grid.SetColumn(tb, j + 1);
                    InputGrid.Children.Add(tb);
                }
            }

            // Ввод запасов
            for (int i = 0; i < suppliers; i++)
            {
                TextBox tb = new TextBox { Width = 50, Margin = new Thickness(2) };
                supplyTextBoxes[i] = tb;
                Grid.SetRow(tb, i + 1);
                Grid.SetColumn(tb, consumers + 1);
                InputGrid.Children.Add(tb);
            }

            // Ввод потребностей
            for (int j = 0; j < consumers; j++)
            {
                TextBox tb = new TextBox { Width = 50, Margin = new Thickness(2) };
                demandTextBoxes[j] = tb;
                Grid.SetRow(tb, suppliers + 1);
                Grid.SetColumn(tb, j + 1);
                InputGrid.Children.Add(tb);
            }

            // Метки для запасов и потребностей
            TextBlock supplyLabel = new TextBlock
            {
                Text = "Запасы",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            };
            Grid.SetRow(supplyLabel, 0);
            Grid.SetColumn(supplyLabel, consumers + 1);
            InputGrid.Children.Add(supplyLabel);

            TextBlock demandLabel = new TextBlock
            {
                Text = "Потребности",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            };
            Grid.SetRow(demandLabel, suppliers + 1);
            Grid.SetColumn(demandLabel, 0);
            InputGrid.Children.Add(demandLabel);
        }
        private void Solve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResultTextBlock.Text = "";
                int[,] cost = new int[suppliers, consumers];
                int[] supply = new int[suppliers];
                int[] demand = new int[consumers];

                // Чтение стоимостей
                for (int i = 0; i < suppliers; i++)
                {
                    for (int j = 0; j < consumers; j++)
                    {
                        if (!int.TryParse(costTextBoxes[i, j].Text, out cost[i, j]))
                        {
                            ResultTextBlock.Text = $"Неверная стоимость на S{i + 1}-C{j + 1}";
                            return;
                        }
                    }
                }

                // Чтение запасов
                for (int i = 0; i < suppliers; i++)
                {
                    if (!int.TryParse(supplyTextBoxes[i].Text, out supply[i]))
                    {
                        ResultTextBlock.Text = $"Неверный запас для поставщика S{i + 1}";
                        return;
                    }
                }

                // Чтение потребностей
                for (int j = 0; j < consumers; j++)
                {
                    if (!int.TryParse(demandTextBoxes[j].Text, out demand[j]))
                    {
                        ResultTextBlock.Text = $"Неверная потребность для потребителя C{j + 1}";
                        return;
                    }
                }

                int totalSupply = 0, totalDemand = 0;
                foreach (var s in supply) totalSupply += s;
                foreach (var d in demand) totalDemand += d;

                // Балансировка
                bool balanced = totalSupply == totalDemand;
                if (!balanced)
                {
                    ResultTextBlock.Text += "Задача несбалансирована. Добавляется фиктивный " +
                        (totalSupply < totalDemand ? "поставщик.\n" : "потребитель.\n");

                    if (totalSupply < totalDemand)
                    {
                        Array.Resize(ref supply, supply.Length + 1);
                        supply[supply.Length - 1] = totalDemand - totalSupply;

                        var newCost = new int[supply.Length, consumers];
                        for (int i = 0; i < suppliers; i++)
                            for (int j = 0; j < consumers; j++)
                                newCost[i, j] = cost[i, j];

                        for (int j = 0; j < consumers; j++)
                            newCost[supply.Length - 1, j] = 0;

                        cost = newCost;
                        suppliers++;
                    }
                    else
                    {
                        Array.Resize(ref demand, demand.Length + 1);
                        demand[demand.Length - 1] = totalSupply - totalDemand;

                        var newCost = new int[suppliers, demand.Length];
                        for (int i = 0; i < suppliers; i++)
                            for (int j = 0; j < consumers; j++)
                                newCost[i, j] = cost[i, j];

                        for (int i = 0; i < suppliers; i++)
                            newCost[i, demand.Length - 1] = 0;

                        cost = newCost;
                        consumers++;
                    }
                }

                // Метод северо-западного угла
                int[,] allocation = new int[suppliers, consumers];
                int si = 0, cj = 0;
                int[] tempSupply = (int[])supply.Clone();
                int[] tempDemand = (int[])demand.Clone();

                while (si < suppliers && cj < consumers)
                {
                    int x = Math.Min(tempSupply[si], tempDemand[cj]);
                    allocation[si, cj] = x;
                    tempSupply[si] -= x;
                    tempDemand[cj] -= x;

                    if (tempSupply[si] == 0) si++;
                    if (tempDemand[cj] == 0) cj++;
                }

                // Подсчёт общей стоимости
                int totalCost = 0;
                for (int i = 0; i < suppliers; i++)
                {
                    for (int j = 0; j < consumers; j++)
                    {
                        totalCost += allocation[i, j] * cost[i, j];
                    }
                }

                ShowResult(allocation, cost, totalCost);
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = "Ошибка: " + ex.Message;
            }
        }

        private void ShowResult(int[,] allocation, int[,] cost, int totalCost)
        {
            string result = "Результат (метод северо-западного угла):\n\n";
            for (int i = 0; i < allocation.GetLength(0); i++)
            {
                for (int j = 0; j < allocation.GetLength(1); j++)
                {
                    result += $"[{allocation[i, j]}] ";
                }
                result += "\n";
            }
            result += $"\nОбщая стоимость перевозки: {totalCost}";
            ResultTextBlock.Text += "\n" + result;
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            // Очистка результатов
            ResultTextBlock.Text = "";

            try
            {
                // Очистка стоимостей (с проверкой на null и границы)
                if (costTextBoxes != null)
                {
                    for (int i = 0; i < Math.Min(suppliers, costTextBoxes.GetLength(0)); i++)
                    {
                        for (int j = 0; j < Math.Min(consumers, costTextBoxes.GetLength(1)); j++)
                        {
                            if (costTextBoxes[i, j] != null)
                                costTextBoxes[i, j].Text = "";
                        }
                    }
                }

                // Очистка запасов (с проверкой на null и границы)
                if (supplyTextBoxes != null)
                {
                    for (int i = 0; i < Math.Min(suppliers, supplyTextBoxes.Length); i++)
                    {
                        if (supplyTextBoxes[i] != null)
                            supplyTextBoxes[i].Text = "";
                    }
                }

                // Очистка потребностей (с проверкой на null и границы)
                if (demandTextBoxes != null)
                {
                    for (int j = 0; j < Math.Min(consumers, demandTextBoxes.Length); j++)
                    {
                        if (demandTextBoxes[j] != null)
                            demandTextBoxes[j].Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при очистке: {ex.Message}");
            }
        }


    }
}

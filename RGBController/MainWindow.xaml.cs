using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace RGBController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> portSelectorItems = new ObservableCollection<string>();

        private readonly int[] bitrates = {
            300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200, 250000
        };

        public ObservableCollection<ComboBoxItem> framerateSelectorItems = new ObservableCollection<ComboBoxItem>();

        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindow_Closed;
            portSelector.ItemsSource = portSelectorItems;
            framerateSelector.ItemsSource = framerateSelectorItems;

            Task.Run(() =>
            {
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        while (App.serialPort.IsOpen == beginButton.IsEnabled) ;

                        beginButton.IsEnabled = !App.serialPort.IsOpen;
                        framerateSelector.IsEnabled = !App.serialPort.IsOpen;
                        portSelector.IsEnabled = !App.serialPort.IsOpen;
                        pixelCountBox.IsEnabled = !App.serialPort.IsOpen;
                    });
                }
            });
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            App.httpListener.Close();
        }

        private void portSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) App.serialPort.PortName = (string)e.AddedItems[0];
        }

        private void pixelCountBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly Regex _regex = new Regex("[^0-9]+");
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void pixelCountBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = pixelCountBox.Text;
            int pixels = text.Equals("") ? 0 : int.Parse(text);
            int bitsToSend = (3 * pixels + 1) * 8;
            framerateSelectorItems.Clear();
            foreach (int rate in bitrates)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = ((float)rate / bitsToSend).ToString("N2");
                item.Tag = rate;
                framerateSelectorItems.Add(item);
            }
        }

        private void framerateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) App.serialPort.BaudRate = (int)((ComboBoxItem)e.AddedItems[0]).Tag;
        }

        private void beginButton_Click(object sender, RoutedEventArgs e)
        {
            App.serialPort.Open();
        }
    }
}

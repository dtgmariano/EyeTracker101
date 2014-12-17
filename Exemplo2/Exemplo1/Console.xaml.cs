using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;


namespace Exemplo1
{
    /// <summary>
    /// Interaction logic for Console.xaml
    /// </summary>
    public partial class Console : Window
    {
        public string position;
        public Console()
        {
            InitializeComponent();
        }

        private void btTopLeft_Click(object sender, RoutedEventArgs e)
        {
            StartButton(0, 0);
        }

        private void btTopCenter_Click(object sender, RoutedEventArgs e)
        {
            StartButton((System.Windows.SystemParameters.PrimaryScreenWidth - Width) / 2, 0);
        }

        private void btTopRight_Click(object sender, RoutedEventArgs e)
        {
            StartButton(System.Windows.SystemParameters.PrimaryScreenWidth - Width, 0);
        }

        private void btCenterLeft_Click(object sender, RoutedEventArgs e)
        {
            StartButton(0, (System.Windows.SystemParameters.PrimaryScreenHeight - Height) / 2);
        }

        private void btCenterRight_Click(object sender, RoutedEventArgs e)
        {
            StartButton(System.Windows.SystemParameters.PrimaryScreenWidth - Width, (System.Windows.SystemParameters.PrimaryScreenHeight - Height) / 2);
        }

        private void btBottomLeft_Click(object sender, RoutedEventArgs e)
        {
            StartButton(0, System.Windows.SystemParameters.PrimaryScreenHeight - Height);
        }

        private void btBottomCenter_Click(object sender, RoutedEventArgs e)
        {
            StartButton((System.Windows.SystemParameters.PrimaryScreenWidth - Width) / 2, System.Windows.SystemParameters.PrimaryScreenHeight - Height);
        }

        private void btBottomRight_Click(object sender, RoutedEventArgs e)
        {
            StartButton(System.Windows.SystemParameters.PrimaryScreenWidth - Width, System.Windows.SystemParameters.PrimaryScreenHeight - Height);
        }
        public void StartButton(double _Left, double _Top)
        {
            MainWindow win2 = new MainWindow(_Left, _Top);
            win2.Show();
            this.Close();
        }
    }
}

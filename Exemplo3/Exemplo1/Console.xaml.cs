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
using System.Windows.Forms;


namespace Exemplo1
{
    public partial class Console : Window
    {
        public int btSize = 200;
        public Console()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void btTopLeft_Click(object sender, RoutedEventArgs e)
        {
            StartButton(0, 0);
        }

        private void btTopCenter_Click(object sender, RoutedEventArgs e)
        {
            StartButton((Screen.PrimaryScreen.Bounds.Width - btSize)/ 2, 0);
        }

        private void btTopRight_Click(object sender, RoutedEventArgs e)
        {
            StartButton(Screen.PrimaryScreen.Bounds.Width - btSize, 0);
        }

        private void btCenterLeft_Click(object sender, RoutedEventArgs e)
        {
            StartButton(0, (Screen.PrimaryScreen.Bounds.Height - btSize)/ 2);
        }

        private void btCenterRight_Click(object sender, RoutedEventArgs e)
        {
            StartButton(Screen.PrimaryScreen.Bounds.Width - btSize, (Screen.PrimaryScreen.Bounds.Height - btSize)/ 2);
        }

        private void btBottomLeft_Click(object sender, RoutedEventArgs e)
        {
            StartButton(0, Screen.PrimaryScreen.Bounds.Height - btSize);
        }

        private void btBottomCenter_Click(object sender, RoutedEventArgs e)
        {
            StartButton((Screen.PrimaryScreen.Bounds.Width - btSize)/ 2, Screen.PrimaryScreen.Bounds.Height - btSize);
        }

        private void btBottomRight_Click(object sender, RoutedEventArgs e)
        {
            StartButton(Screen.PrimaryScreen.Bounds.Width - btSize, Screen.PrimaryScreen.Bounds.Height - btSize);
        }
        public void StartButton(double _Left, double _Top)
        {
            MainWindow win2 = new MainWindow(_Left, _Top);
            win2.Show();
            this.Close();
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

using System;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using TETControls.Calibration;
using TETControls.Cursor;
using TETControls.TrackBox;
using TETCSharpClient.Data;
using System.Windows.Interop;
using TETCSharpClient;
using MessageBox = System.Windows.MessageBox;

namespace Calibration_Mark1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
//    public partial class MainWindow : Window
//    {
//        public MainWindow()
//        {
//            InitializeComponent();
//        }
//    }
//}
    public partial class MainWindow : IConnectionStateListener
    {
        private Screen activeScreen = Screen.PrimaryScreen;
        private CursorControl cursorControl;

        private bool isCalibrated;

        public MainWindow()
        {
            InitializeComponent();
            this.ContentRendered += (sender, args) => InitClient();
            this.KeyDown += MainWindow_KeyDown;
        }

        private void InitClient()
        {
            // Activate/connect client
            GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push);

            // Listen for changes in connection to server
            GazeManager.Instance.AddConnectionStateListener(this);

            // Fetch current status
            OnConnectionStateChanged(GazeManager.Instance.IsActivated);

            // Add a fresh instance of the trackbox in case we reinitialize the client connection.
            TrackingStatusGrid.Children.Clear();
            TrackingStatusGrid.Children.Add(new TrackBoxStatus());

            UpdateState();
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e == null)
                return;

            switch (e.Key)
            {
                // Start calibration on hitting "C"
                case Key.C:
                    ButtonCalibrateClicked(this, null);
                    break;

                // Toggle mouse redirect with "M"
                case Key.M:
                    ButtonMouseClicked(this, null);
                    break;

                // Turn cursor control off on hitting Escape
                case Key.Escape:
                    if (cursorControl != null)
                        cursorControl.Enabled = false;

                    UpdateState();
                    break;
            }
        }

        public void OnConnectionStateChanged(bool IsActivated)
        {
            // The connection state listener detects when the connection to the EyeTribe server changes
            if (btnCalibrate.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new MethodInvoker(() => OnConnectionStateChanged(IsActivated)));
                return;
            }

            if (!IsActivated)
                GazeManager.Instance.Deactivate();

            UpdateState();
        }

        private void ButtonCalibrateClicked(object sender, RoutedEventArgs e)
        {
            // Check connectivitiy status
            if (GazeManager.Instance.IsActivated == false)
                InitClient();

            // API needs to be active to start calibrating
            if (GazeManager.Instance.IsActivated)
                Calibrate();
            else
                UpdateState(); // show reconnect
        }

        private void ButtonMouseClicked(object sender, RoutedEventArgs e)
        {
            if (GazeManager.Instance.IsCalibrated == false)
                return;

            if (cursorControl == null)
                cursorControl = new CursorControl(activeScreen, true, true); // Lazy initialization
            else
                cursorControl.Enabled = !cursorControl.Enabled; // Toggle on/off

            UpdateState();
        }

        private void Calibrate()
        {
            // Update screen to calibrate where the window currently is
            activeScreen = Screen.FromHandle(new WindowInteropHelper(this).Handle);

// TESTE FUNÇÃO
            int amostragem = Convert.ToInt32(tempoAmostragem.Text);
            int transicao = Convert.ToInt32(tempoTransicao.Text);

            int totalPontos;
            if(pontos9.IsChecked == true)
                totalPontos = 9;
            else if (pontos12.IsChecked == true)
                totalPontos = 12;
            else
                totalPontos = 16;

            int sequencia;
            if (seqCrescente.IsChecked == true)
                sequencia = 1;
            else if (seqDescrescente.IsChecked == true)
                sequencia = 2;
            else
                sequencia = 3;
// FIM TESTE FUNÇÃO
            // Initialize and start the calibration
            CalibrationRunner calRunner = new CalibrationRunner(activeScreen, activeScreen.Bounds.Size, totalPontos, amostragem, transicao, sequencia);
            calRunner.OnResult += calRunner_OnResult;
            calRunner.Start();
        }

        private void calRunner_OnResult(object sender, CalibrationRunnerEventArgs e)
        {
            // Invoke on UI thread since we are accessing UI elements
            if (RatingText.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new MethodInvoker(() => calRunner_OnResult(sender, e)));
                return;
            }

            // Show calibration results rating
            if (e.Result == CalibrationRunnerResult.Success)
            {
                isCalibrated = true;
                UpdateState();
            }
            else
                MessageBox.Show(this, "Calibration failed, please try again");
        }

        private void UpdateState()
        {
            // No connection
            if (GazeManager.Instance.IsActivated == false)
            {
                btnCalibrate.Content = "Connect";
                btnMouse.Content = "";
                RatingText.Text = "";
                return;
            }

            if (GazeManager.Instance.IsCalibrated == false)
            {
                btnCalibrate.Content = "Calibrate";
            }
            else
            {
                btnCalibrate.Content = "Recalibrate";

                // Set mouse-button label
                btnMouse.Content = "Mouse control On";

                if (cursorControl != null && cursorControl.Enabled)
                    btnMouse.Content = "Mouse control Off";

                if (GazeManager.Instance.LastCalibrationResult != null)
                    RatingText.Text = RatingFunction(GazeManager.Instance.LastCalibrationResult);
            }
        }

        private string RatingFunction(CalibrationResult result)
        {
            if (result == null)
                return "";

            double accuracy = result.AverageErrorDegree;

            if (accuracy < 0.5)
                return "Calibration Quality: PERFECT";

            if (accuracy < 0.7)
                return "Calibration Quality: GOOD";

            if (accuracy < 1)
                return "Calibration Quality: MODERATE";

            if (accuracy < 1.5)
                return "Calibration Quality: POOR";

            return "Calibration Quality: REDO";
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            GazeManager.Instance.Deactivate();
        }
    }
}


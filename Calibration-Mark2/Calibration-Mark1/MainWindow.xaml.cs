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
using System.Windows.Controls;
using System.Windows.Media;
using TETCSharpClient;
using System.ComponentModel;
using MessageBox = System.Windows.MessageBox;

using System.Diagnostics;
using System.IO;

namespace Calibration_Mark1
{
    public partial class MainWindow : IConnectionStateListener
    {
        private Screen activeScreen = Screen.PrimaryScreen;
        private CursorControl cursorControl;

        //private bool isCalibrated;
        public int totalPontos;

        public int sampleTimeMs;
        public int transitionTimeMs;

        public MainWindow()
        {
            InitializeComponent();

            totalPontos = Convert.ToInt32(pontos9.Content);
            

            if (!IsServerProcessRunning())
                StartServerProcess();

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

            if (!IsServerProcessRunning())
                StartServerProcess();

            UpdateState();
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e == null)
                return;

            switch (e.Key)
            {
                // Iniciar calibração ao pressionar tecla "C"
                case Key.C:
                    ButtonCalibrateClicked(this, null);
                    break;

                // Alternar controle do mouse ao pressionar tecla "M"
                case Key.M:
                    ButtonMouseClicked(this, null);
                    break;

                // Desligar controle do mouse ao pressionar ESC
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
            sampleTimeMs = Convert.ToInt32(TextBox_Amostragem.Text);
            transitionTimeMs = Convert.ToInt32(TextBox_Transicao.Text);

            string sequenciaCustomizada = " ";

            int sequencia;

            if (directionToggle.IsChecked == true)
                MessageBox.Show("Evento Positivo.");

            if (randomMode.IsChecked == true)
                sequencia = 3;
            else
            {
                sequencia = 4;
                sequenciaCustomizada = customModeInput.Text;
            }
// FIM TESTE FUNÇÃO
            // Initialize and start the calibration

            CalibrationRunner calRunner = new CalibrationRunner(activeScreen, activeScreen.Bounds.Size, totalPontos, sampleTimeMs, transitionTimeMs, sequencia, sequenciaCustomizada);
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

            // Mostrar resultados da calibração
            if (e.Result == CalibrationRunnerResult.Success)
            {
                //isCalibrated = true;
                UpdateState();
            }
            else
                MessageBox.Show(this, "Calibração falhou, por favor tente novamente");
        }

        private void UpdateState()
        {
            // No connection
            if (GazeManager.Instance.IsActivated == false)
            {
                btnCalibrate.Content = "Conectar";
                btnMouse.Content = "";
                RatingText.Text = "";
                return;
            }

            if (GazeManager.Instance.IsCalibrated == false)
            {
                btnCalibrate.Content = "Calibrar";
            }
            else
            {
                btnCalibrate.Content = "Recalibrar";

                // Set mouse-button label
                btnMouse.Content = "Controle do mouse ON";

                if (cursorControl != null && cursorControl.Enabled)
                    btnMouse.Content = "Controle do mouse OFF";

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
                return "Qualidade da Calibração: EXCELENTE";

            if (accuracy < 0.7)
                return "Qualidade da Calibração: BOA";

            if (accuracy < 1)
                return "Qualidade da Calibração: MODERADA";

            if (accuracy < 1.5)
                return "Qualidade da Calibração: RUIM";

            return "Qualidade da Calibração: REFAZER";
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            GazeManager.Instance.Deactivate();
        }

        private void CustomModeSelected(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;
            System.Windows.Controls.RadioButton radioButton = e.OriginalSource as System.Windows.Controls.RadioButton;

            if ((radioButton != null) && (radioButton.Name == "customMode"))
                customModeInput.Visibility = Visibility.Visible;
            else
                customModeInput.Visibility = Visibility.Hidden;
        }

        private void CountTotalPoints(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;
            System.Windows.Controls.RadioButton radioButton = e.OriginalSource as System.Windows.Controls.RadioButton;

            if (radioButton != null)
            {
                totalPontos = Convert.ToInt32(radioButton.Content);
            }
        }

        private static bool IsServerProcessRunning()
        {
            try
            {
                foreach (Process p in Process.GetProcesses())
                {
                    if (p.ProcessName.ToLower() == "eyetribe")
                        return true;
                }
            }
            catch (Exception)
            { }

            return false;
        }

        private static void StartServerProcess()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WindowStyle = ProcessWindowStyle.Minimized;
            psi.FileName = GetServerExecutablePath();

            if (psi.FileName == string.Empty || File.Exists(psi.FileName) == false)
                return;

            Process processServer = new Process();
            processServer.StartInfo = psi;
            processServer.Start();

            Thread.Sleep(3000); // wait for it to spin up
        }

        private static string GetServerExecutablePath()
        {
            // check default paths           
            const string x86 = "C:\\Program Files (x86)\\EyeTribe\\Server\\EyeTribe.exe";
            if (File.Exists(x86))
                return x86;

            const string x64 = "C:\\Program Files\\EyeTribe\\Server\\EyeTribe.exe";
            if (File.Exists(x64))
                return x64;

            // Still not found, let user select file
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".exe";
            dlg.Title = "Please select the Eye Tribe server executable";
            dlg.Filter = "Executable Files (*.exe)|*.exe";

            //if (dlg.ShowDialog() == true)
            //    return dlg.FileName;

            return string.Empty;
        }
    }
}


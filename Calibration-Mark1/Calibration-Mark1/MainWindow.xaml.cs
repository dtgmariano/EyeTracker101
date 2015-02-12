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
using MessageBox = System.Windows.MessageBox;

namespace Calibration_Mark1
{
    public partial class MainWindow : IConnectionStateListener
    {
        private Screen activeScreen = Screen.PrimaryScreen;
        private CursorControl cursorControl;

        //private bool isCalibrated;
        public int totalPontos;

        public int tempoAmostragem;
        public int tempoTransicao;

        public MainWindow()
        {
            InitializeComponent();

            totalPontos = Convert.ToInt32(pontos9.Content);

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
            tempoAmostragem = Convert.ToInt32(TextBox_Amostragem.Text);
            tempoTransicao = Convert.ToInt32(TextBox_Transicao.Text);

            int[] josefa = tointarray(textoCustomizada.Text, '-');
            string sequenciaCustomizada = " ";

            //int totalPontos;
            //if(pontos9.IsChecked == true)
            //    totalPontos = 9;
            //else if (pontos12.IsChecked == true)
            //    totalPontos = 12;
            //else
            //    totalPontos = 16;

            int sequencia;
            if (seqCrescente.IsChecked == true)
                sequencia = 1;
            else if (seqDescrescente.IsChecked == true)
                sequencia = 2;
            else if (seqAleatoria.IsChecked == true)
                sequencia = 3;
            else
            {
                sequencia = 4;
                sequenciaCustomizada = textoCustomizada.Text;
            }
// FIM TESTE FUNÇÃO
            // Initialize and start the calibration
            CalibrationRunner calRunner = new CalibrationRunner(activeScreen, activeScreen.Bounds.Size, totalPontos, tempoAmostragem, tempoTransicao, sequencia);
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

        private void TextBox_seqCustomizada(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;
            System.Windows.Controls.RadioButton radioButton = e.OriginalSource as System.Windows.Controls.RadioButton;

            if ((radioButton != null) && (radioButton.Name == "seqCustomizada"))
                textoCustomizada.Visibility = Visibility.Visible;
            else
                textoCustomizada.Visibility = Visibility.Hidden;
        }

        private void RadioButton_totalPontos(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;
            System.Windows.Controls.RadioButton radioButton = e.OriginalSource as System.Windows.Controls.RadioButton;

            if (radioButton != null)
            {
                totalPontos = Convert.ToInt32(radioButton.Content);
            }
        }

        int[] tointarray(string value, char sep)
        {
            string[] sa = value.Split(sep);
            int[] ia = new int[sa.Length];
            for (int i = 0; i < ia.Length; ++i)
            {
                int j;
                string s = sa[i];
                if (int.TryParse(s, out j))
                {
                    ia[i] = j;
                }
            }
            return ia;
        }
    }
}


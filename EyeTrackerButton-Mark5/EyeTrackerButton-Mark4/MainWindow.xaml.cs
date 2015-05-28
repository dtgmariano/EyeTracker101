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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

using TETCSharpClient;
using TETCSharpClient.Data;

using EyeTrackerButton_Mark4.Handler;


namespace EyeTrackerButton_Mark4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IGazeListener
    {
        #region Variables

        private const int LOG_LENGTH = 100;
        public double buttonHeight;
        public double buttonWidth;
        //private List<GazeData> sampleLog;
        DispatcherTimer timer = new DispatcherTimer();
        public IConnectionStateListener listener;
        private bool CONTROLE = false;

        #endregion

        #region Constructor

        public MainWindow(double _Left, double _Top)
        {
            InitializeComponent();
            GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push);
            GazeManager.Instance.AddGazeListener(this);
            //Teste
            GazeManager.Instance.AddConnectionStateListener(listener);
            // Fim de Teste
            timer.Interval = TimeSpan.FromMilliseconds(800);
            timer.Tick += timer_Tick;
            timer.Start();

            StartupPosition(_Left, _Top);
            buttonWidth = _Left;
            buttonHeight = _Top;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

            this.Closing += MainWindow_Closing;
        }

        #endregion

        #region Get/Set

        public bool Enabled { get; set; }
        public bool Smooth { get; set; }
        public Screen ActiveScreen { get; set; }

        #endregion

        #region Public methods

        public void OnGazeUpdate(GazeData gazeData)
        {
            var x = (int)Math.Round(gazeData.SmoothedCoordinates.X, 0);
            var y = (int)Math.Round(gazeData.SmoothedCoordinates.Y, 0);
            if (x == 0 & y == 0) return;

            // Invoke thread

            //Dispatcher.BeginInvoke(new Action(() => UpdateUI(x, y)));
            //NativeMethods.SetCursorPos(x, y);
            CheckClick(x, y, buttonHeight, buttonWidth);

            // start or stop tracking lost animation
            if ((gazeData.State & GazeData.STATE_TRACKING_GAZE) == 0 &&
               (gazeData.State & GazeData.STATE_TRACKING_PRESENCE) == 0) 
                //return;
                Dispatcher.BeginInvoke(new Action(() => buttonSelect.Fill = new SolidColorBrush(System.Windows.Media.Colors.Red)));

            //OnConnectionStateChanged(GazeManager.Instance.IsActivated);
            //if (listener.OnConnectionStateChanged == true)
            //if ((gazeData.State & GazeData.STATE_TRACKING_LOST) != 0)
                //Dispatcher.BeginInvoke(new Action(() => buttonSelect.Fill = new SolidColorBrush(System.Windows.Media.Colors.Red)));

        }

        public void OnConnectionStateChanged(bool IsConnected)
        {
            if (IsConnected == false)
            {
                Dispatcher.BeginInvoke(new Action(() => buttonSelect.Fill = new SolidColorBrush(System.Windows.Media.Colors.Red)));
            }
            //else 
            //{
            //    Dispatcher.BeginInvoke(new Action(() => buttonSelect.Fill = new SolidColorBrush(System.Windows.Media.Colors.DarkGray)));
            //}
        }

        public void StartupPosition(double _Left, double _Top)
        {
            Left = _Left;
            Top = _Top;
        }

        #endregion

        #region Private methods

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GazeManager.Instance.RemoveGazeListener(this);
            GazeManager.Instance.Deactivate();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        //private void UpdateUI(int x, int y)
        //{
        //    // Unhide the GazePointer if you want to see your gaze point
        //    if (GazePointer.Visibility == Visibility.Visible)
        //    {
        //        var relativePt = new System.Windows.Point(x, y);
        //        relativePt = transfrm.Transform(relativePt);
        //        Canvas.SetLeft(GazePointer, relativePt.X - GazePointer.Width / 2);
        //        Canvas.SetTop(GazePointer, relativePt.Y - GazePointer.Height / 2);
        //    }
        //}

        private void CheckClick(int x, int y, double buttonHeight, double buttonWidth)
        {
            var newVarW = 200;
            //var newVarH = 200;

            // Verificando opções de rolagem de tela.
            //if ((y > buttonHeight) && (y < buttonHeight + newVarH) && (x < buttonWidth + newVarW) && (x > buttonWidth))           
            if ((x < buttonWidth + newVarW) && (x > buttonWidth))
            {
                if (!CONTROLE)
                { 
                    Dispatcher.BeginInvoke(new Action(() => buttonSelect.Fill = new SolidColorBrush(System.Windows.Media.Colors.LightGreen)));
                    Dispatcher.BeginInvoke(new Action(() => MouseHandler.doClick(x,y)));
                    CONTROLE = true;
                }
            }
            else 
            {
                if (CONTROLE)
                {
                    Dispatcher.BeginInvoke(new Action(() => buttonSelect.Fill = new SolidColorBrush(System.Windows.Media.Colors.DarkGray)));
                    timer.Start();
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            CONTROLE = false;
            OnConnectionStateChanged(GazeManager.Instance.IsActivated);
            //timer.Stop();
        }

        #endregion

        public class NativeMethods
        {
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
            [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]

            public static extern bool SetCursorPos(int x, int y);
        }
    }
}

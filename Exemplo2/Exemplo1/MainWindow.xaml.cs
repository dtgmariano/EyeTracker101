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

using TETCSharpClient;
using TETCSharpClient.Data;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Runtime.InteropServices; 

namespace Exemplo1
{
    public partial class MainWindow : Window, IGazeListener
    {
        #region Variables

        private const int LOG_LENGTH = 100;
        public double buttonHeight;
        public double buttonWidth;
        //private List<GazeData> sampleLog;
        DispatcherTimer timer = new DispatcherTimer();
        private bool CONTROLE = false;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        #endregion

        #region Constructor

        public MainWindow(double _Left, double _Top)
        {
            InitializeComponent();
            GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push);
            GazeManager.Instance.AddGazeListener(this);

            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += timer_Tick;

            StartupPosition(_Left, _Top);
            buttonWidth = _Left;
            buttonHeight = _Top;

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
            NativeMethods.SetCursorPos(x, y);
            CheckClick(x, y, buttonHeight, buttonWidth);

            // start or stop tracking lost animation
            if ((gazeData.State & GazeData.STATE_TRACKING_GAZE) == 0 &&
               (gazeData.State & GazeData.STATE_TRACKING_PRESENCE) == 0) return;

        }

        public void StartupPosition(double _Left, double _Top)
        {
            this.Left = _Left;
            this.Top = _Top;
        }

        //public void StartupPosition(string position)
        //{
        //    switch (position)
        //    {
        //        case "TL":
        //            Left = 0;
        //            Top = 0;
        //            break;
        //        case "TC":
        //            Left = (System.Windows.SystemParameters.PrimaryScreenWidth - Width) / 2;
        //            Top = 0;
        //            break;
        //        case "TR":
        //            Left = System.Windows.SystemParameters.PrimaryScreenWidth - Width;
        //            Top = 0;
        //            break;
        //        case "CL":
        //            Left = 0;
        //            Top = (System.Windows.SystemParameters.PrimaryScreenHeight - Height) / 2;
        //            break;
        //        case "CR":
        //            Left = System.Windows.SystemParameters.PrimaryScreenWidth - Width;
        //            Top = (System.Windows.SystemParameters.PrimaryScreenHeight - Height) / 2;
        //            break;
        //        case "BL":
        //            Left = 0;
        //            Top = System.Windows.SystemParameters.PrimaryScreenHeight - Height;
        //            break;
        //        case "BC":
        //            Left = (System.Windows.SystemParameters.PrimaryScreenWidth - Width) / 2;
        //            Top = System.Windows.SystemParameters.PrimaryScreenHeight - Height;
        //            break;
        //        case "BR":
        //            Left = System.Windows.SystemParameters.PrimaryScreenWidth - Width;
        //            Top = System.Windows.SystemParameters.PrimaryScreenHeight - Height;
        //            break;
        //    }
        //}

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
            var h = Screen.PrimaryScreen.Bounds.Height;
            var w = Screen.PrimaryScreen.Bounds.Width;
            var newVarW = 200;
            var newVarH = 200;

            // Verificando opções de rolagem de tela.
            //if ((y > 0) && (y < newVarH) && (x > w - newVarW) && (x < w))
            if ((y > buttonHeight) && (y < buttonHeight + newVarH) && (x < buttonWidth + newVarW) && (x > buttonWidth))
            {
                if (!CONTROLE)
                { 
                    Dispatcher.BeginInvoke(new Action(() => buttonSelect.Fill = new SolidColorBrush(System.Windows.Media.Colors.LightBlue)));
                    Dispatcher.BeginInvoke(new Action(() => mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y, 0, 0)));
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
            timer.Stop();
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
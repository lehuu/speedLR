using System.Windows;
using System.Windows.Interop;
using Timer = System.Windows.Forms.Timer;
using Brushes = System.Windows.Media.Brushes;
using Application = System.Windows.Application;
using Point = System.Drawing.Point;

namespace SpeedLR
{
    public partial class MainWindow : Window
    {
        private Timer connectionCheckTimer;
        private static int DEFAULT_PORT = 49000;
        private NotifyIcon notifyIcon;

        private ControllerWindow controller;
        private GlobalHotkey activatorHotkey;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ConnectToServer();

            // Check connection periodically
            connectionCheckTimer = new Timer();
            connectionCheckTimer.Interval = 5000; // 5 seconds
            connectionCheckTimer.Tick += CheckConnection;
            connectionCheckTimer.Start();

            // init context menu
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("wheel.ico"); // Replace with your icon file
            notifyIcon.Text = Title;
            notifyIcon.Visible = true;

            // Create a context menu for the NotifyIcon
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += ExitMenuItem_Click;
            ToolStripMenuItem openMenuItem = new ToolStripMenuItem("Open");
            openMenuItem.Click += OpenMenuItem_Click;

            contextMenu.Items.Add(exitMenuItem);
            contextMenu.Items.Add(openMenuItem);

            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.MouseDoubleClick += OpenMenuItem_Click;

            controller = new ControllerWindow();
            controller.Show();
            controller.Hide();

            var helper = new WindowInteropHelper(this);

            activatorHotkey = new GlobalHotkey(helper.Handle, 1, GlobalHotkey.MOD_CONTROL);
            activatorHotkey.Register(HwndHook);
            activatorHotkey.HotKeyDoublePressed += Ctrl_DoublePressed;
        }

        private async void CheckConnection(object sender, EventArgs e)
        {
            var isConnected = await Connector.Instance.IsConnected();
            if (isConnected)
            {
                this.connectButton.Background = Brushes.Green;
                this.connectButton.Content = "Connected";
            }
            else
            {
                this.connectButton.Background = Brushes.Red;
                this.connectButton.Content = "Reconnect";
            }
        }

        private async void ConnectToServer()
        {
            this.portButton.Content = "Port: " + DEFAULT_PORT;

            try
            {
                this.connectButton.Content = "Connecting...";
                this.connectButton.Background = Brushes.Blue;

                await Connector.Instance.Connect(DEFAULT_PORT);

                this.connectButton.Background = Brushes.Green;
                this.connectButton.Content = "Connected";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.connectButton.Background = Brushes.Red;
                this.connectButton.Content = "Reconnect";
            }
        }

        private void Ctrl_DoublePressed(object sender, EventArgs e)
        {
            if (controller.IsVisible)
            {
                controller.Hide();
            }
            else
            {
                var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                Point mousePosition = Control.MousePosition;

                var mouse = transform.Transform(new System.Windows.Point(mousePosition.X, mousePosition.Y));

                Screen screen = Screen.FromPoint(mousePosition);

                var formWidth = controller.Width;
                var formHeight = controller.Height;

                double x = mouse.X - formWidth / 2;
                double y = mouse.Y - formHeight / 2;

                var topLeft = transform.Transform(new System.Windows.Point(screen.WorkingArea.Left, screen.WorkingArea.Top));
                var bottomRight = transform.Transform(new System.Windows.Point(screen.WorkingArea.Right, screen.WorkingArea.Bottom));
                x = Math.Max(topLeft.X, Math.Min(x, bottomRight.X - formWidth));
                y = Math.Max(topLeft.Y, Math.Min(y, bottomRight.Y - formHeight));

                controller.Left = x;
                controller.Top = y;
                controller.Show();
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var isConnected = await Connector.Instance.IsConnected();
            if (!isConnected)
            {
                ConnectToServer();
            }
        }

        private void PortButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            // Close the application
            notifyIcon.Dispose(); // Release the notify icon resource
            Application.Current.Shutdown();
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (activatorHotkey != null)
            {
                activatorHotkey.ProcessWindowMessage(hwnd, msg, wParam, lParam, ref handled); // Pass the message to the GlobalHotkey class
            }
            return IntPtr.Zero;
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            Connector.Instance.CloseConnection();
            activatorHotkey.Unregister(HwndHook);
        }
    }
}
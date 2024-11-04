using System.Windows;
using System.Windows.Interop;
using Brushes = System.Windows.Media.Brushes;
using Application = System.Windows.Application;
using Point = System.Drawing.Point;
using System.Windows.Controls;

namespace SpeedLR
{
    public partial class MainWindow : Window
    {
        private static int DEFAULT_PORT = 49000;
        private NotifyIcon _notifyIcon;

        private ControllerWindow _controller;
        private GlobalHotkey _activatorHotkey;

        public MainWindow()
        {
            InitializeComponent();
            CreateMenu();

            IsVisibleChanged += MainWindow_IsVisibleChanged;
        }

        private void CreateMenu()
        {
            int numberOfMenus = 3;
            int numberOfButtons = 8;

            for (int i = 0; i < numberOfMenus; i++)
            {
                for (int j = 0; j < numberOfButtons; j++)
                {
                    EmptyButton button = new EmptyButton();
                    button.Margin = CircleCreator.CreateButtonsInCircle(buttonGrid, i, (float) j / (float) numberOfButtons);
                    buttonGrid.Children.Add(button);
                }
            }
        }

        private void CreateButtonsInCircle(Grid centerGrid, double distance, int buttonCount, float offset)
        {
            // Get the center position of buttonGrid
            double centerX = centerGrid.Margin.Left + centerGrid.ActualWidth / 2;
            double centerY = centerGrid.Margin.Top + centerGrid.ActualHeight / 2;
            double angleStep = 2 * Math.PI / buttonCount;

            for (int i = 0; i < buttonCount; i++)
            {
                // Calculate the current angle
                double angle = i * angleStep + offset * angleStep;

                // Calculate x and y offsets based on the angle and distance
                double offsetX = distance * Math.Cos(angle);
                double offsetY = distance * Math.Sin(angle);

                // Create a new button
                EmptyButton button = new EmptyButton();

                // Set the position of the button
                button.Margin = new Thickness(centerX + offsetX ,
                                              centerY + offsetY, 0, 0);

                // Add button to the main grid (assuming the main Grid is named "MainGrid")
                centerGrid.Children.Add(button);
            }
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CheckConnection(sender, null);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ConnectToServer();

            // init context menu
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = new Icon("wheel.ico"); // Replace with your icon file
            _notifyIcon.Text = Title;
            _notifyIcon.Visible = true;

            // Create a context menu for the NotifyIcon
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += ExitMenuItem_Click;
            ToolStripMenuItem openMenuItem = new ToolStripMenuItem("Open");
            openMenuItem.Click += OpenMenuItem_Click;

            contextMenu.Items.Add(exitMenuItem);
            contextMenu.Items.Add(openMenuItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.MouseDoubleClick += OpenMenuItem_Click;

            _controller = new ControllerWindow();

            var helper = new WindowInteropHelper(this);

            _activatorHotkey = new GlobalHotkey(helper.Handle, 1, GlobalHotkey.MOD_CONTROL);
            _activatorHotkey.Register(HwndHook);
            _activatorHotkey.HotKeyDoublePressed += Ctrl_DoublePressed;
        }

        private async void CheckConnection(object sender, EventArgs? e)
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
            if (_controller.IsVisible)
            {
                _controller.Hide();
            }
            else
            {
                if (_controller.IsPinned)
                {
                    _controller.Show();
                    return;
                }
                var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                Point mousePosition = System.Windows.Forms.Control.MousePosition;

                var mouse = transform.Transform(new System.Windows.Point(mousePosition.X, mousePosition.Y));

                Screen screen = Screen.FromPoint(mousePosition);

                var formWidth = _controller.Width;
                var formHeight = _controller.Height;

                double x = mouse.X - formWidth / 2;
                double y = mouse.Y - formHeight / 2;

                var topLeft = transform.Transform(new System.Windows.Point(screen.WorkingArea.Left, screen.WorkingArea.Top));
                var bottomRight = transform.Transform(new System.Windows.Point(screen.WorkingArea.Right, screen.WorkingArea.Bottom));
                x = Math.Max(topLeft.X, Math.Min(x, bottomRight.X - formWidth));
                y = Math.Max(topLeft.Y, Math.Min(y, bottomRight.Y - formHeight));

                _controller.Left = x;
                _controller.Top = y;
                _controller.Show();
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
            _notifyIcon.Dispose(); // Release the notify icon resource
            Application.Current.Shutdown();
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_activatorHotkey != null)
            {
                _activatorHotkey.ProcessWindowMessage(hwnd, msg, wParam, lParam, ref handled); // Pass the message to the GlobalHotkey class
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
            _activatorHotkey.Unregister(HwndHook);
        }
    }
}
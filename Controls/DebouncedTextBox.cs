using System.Windows.Controls;
using System.Windows.Threading;
using TextBox = System.Windows.Controls.TextBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace SpeedLR.Controls
{
    public class DebouncedTextBox : TextBox
    {
        private DispatcherTimer _debounceTimer;
        private bool _isTextChangedFromUI = false;

        public event EventHandler DebouncedTextChanged;

        public DebouncedTextBox()
        {
            // Initialize the debounce timer
            _debounceTimer = new DispatcherTimer();
            _debounceTimer.Interval = TimeSpan.FromMilliseconds(300); // Adjust delay as needed
            _debounceTimer.Tick += DebounceTimer_Tick;

            // Attach event handlers
            TextChanged += DebouncedTextBox_TextChanged;
            PreviewKeyDown += DebouncedTextBox_PreviewKeyDown;
        }

        private void DebouncedTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _isTextChangedFromUI = true;
        }

        private void DebouncedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isTextChangedFromUI)
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            }

            _isTextChangedFromUI = false;
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();

            // Raise the DebouncedTextChanged event
            DebouncedTextChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

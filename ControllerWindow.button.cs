using System.Windows;
using System.Linq;

namespace SpeedLR
{

    public partial class ControllerWindow : Window
    {
        private void Escape_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            ClearActiveButtons();
        }

        private void Next_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            ToggleButton(_currentMenuIndex, (_currentButtonIndex + 1) % _menus[_currentMenuIndex].Length);
        }

        private void Next_Submenu(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            var nextSubmenu = (_currentMenuIndex + 1) % _menus.Length;
            ToggleButton(nextSubmenu, Math.Clamp(_currentButtonIndex, 0, _menus[nextSubmenu].Length - 1));
        }

        private void Prev_Submenu(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            var nextSubmenu = (_currentMenuIndex - 1 + _menus.Length) % _menus.Length;
            ToggleButton(nextSubmenu, Math.Clamp(_currentButtonIndex, 0, _menus[nextSubmenu].Length - 1));
        }

        private void Prev_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            ToggleButton(_currentMenuIndex, (_currentButtonIndex - 1 + _menus[_currentMenuIndex].Length) % _menus[_currentMenuIndex].Length);
        }

        private void Increase_Step(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            var currentStep = _stepButtons.Select((item, i) => new { Item = item, Index = i })
                .FirstOrDefault(x => x.Item.IsActive)?.Index ?? -1;

            var nextStep = (currentStep + 1 + _stepButtons.Length) % _stepButtons.Length;
            for (int i = 0; i < _stepButtons.Length; i++)
            {
                _stepButtons[i].IsActive = i == nextStep;
            }
        }

        private void Decrease_Step(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            var currentStep = _stepButtons.Select((item, i) => new { Item = item, Index = i })
                .FirstOrDefault(x => x.Item.IsActive)?.Index ?? 0;

            var nextStep = (currentStep - 1 + _stepButtons.Length) % _stepButtons.Length;
            for (int i = 0; i < _stepButtons.Length; i++)
            {
                _stepButtons[i].IsActive = i == nextStep;
            }
        }

        private void Reset_Pressed(object sender, EventArgs e)
        {
            SendCommand(CommandType.RESET);
        }

        private void Reset_Pressed()
        {
            SendCommand(CommandType.RESET);
        }

        private void Inc_Pressed(object sender, EventArgs e)
        {
            SendCommand(CommandType.UP);
        }

        private void Dec_Pressed(object sender, EventArgs e)
        {
            SendCommand(CommandType.DOWN);
        }

        private void HandleGlobalScrollUp()
        {
            // This will be invoked on any upward scroll globally
            Dispatcher.Invoke(() =>
            {
                SendCommand(CommandType.UP);
            });
        }

        private void HandleGlobalScrollDown()
        {
            // This will be invoked on any downward scroll globally
            Dispatcher.Invoke(() =>
            {
                SendCommand(CommandType.DOWN);
            });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ControlButton clickedButton)
            {
                if (String.IsNullOrEmpty(clickedButton.LRCommand))
                {
                    return;
                }

                ToggleButton(clickedButton);
            }
        }
        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ControlButton clickedButton)
            {
                foreach (var item in _stepButtons)
                {
                    item.IsActive = clickedButton.Name == item.Name;
                }
            }
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}

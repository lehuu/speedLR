using System.Windows;
using System.Windows.Input;
using SpeedLR.Controls;

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

            if (InEditMode)
            {
                InEditMode = false;
                return;
            }

            Hide();
        }

        private void Back_Pressed(object sender, EventArgs e)
        {
            switch (CurrentButton.Type)
            {

                case ButtonType.LR:
                    if (InEditMode)
                    {
                        Reset_Pressed();
                        return;
                    }
                    return;
                case ButtonType.MENU:
                case ButtonType.NONE:
                default:
                    return;
            }
        }

        private void Reset_Pressed()
        {
            SendCommand(CommandType.RESET);
        }

        private bool CanNavigate()
        {
            return IsVisible && _menus.Length > 0;
        }

        private int FindNextClosestButton(int row)
        {
            if (CurrentButtonIndex < 0 || CurrentMenuIndex < 0
                || CurrentMenuIndex >= _menus.Length || CurrentButtonIndex >= _menus[CurrentMenuIndex].Length
                || _menus == null || _menus.Length == 0)
            {
                return 0;
            }

            var currentXPosition = _menus[CurrentMenuIndex][CurrentButtonIndex].Margin.Left;

            return _menus[row]
                   .Select((value, index) => new { Value = value.Margin.Left, Index = index })
                   .OrderBy(item => Math.Abs(item.Value - currentXPosition))
                   .First().Index;
        }

        private void Enter_Pressed(object sender, EventArgs e)
        {
            switch (CurrentButton.Type)
            {

                case ButtonType.MENU:
                    SwitchToMenu(CurrentButton.Data);
                    return;
                case ButtonType.LR:
                    InEditMode = !InEditMode;
                    return;
                case ButtonType.NONE:
                default:
                    return;
            }
        }

        private void Up_Pressed(object sender, EventArgs e)
        {
            if (!CanNavigate())
            {
                return;
            }

            if (InEditMode)
            {
                SendCommand(CommandType.UP);
                return;
            }

            var nextSubmenu = (CurrentMenuIndex - 1 + _menus.Length) % _menus.Length;
            ToggleButton(nextSubmenu, FindNextClosestButton(nextSubmenu));
        }

        private void Down_Pressed(object sender, EventArgs e)
        {
            if (!CanNavigate())
            {
                return;
            }

            if (InEditMode)
            {
                SendCommand(CommandType.DOWN);
                return;
            }

            var nextSubmenu = (CurrentMenuIndex + 1) % _menus.Length;
            ToggleButton(nextSubmenu, FindNextClosestButton(nextSubmenu));
        }

        private void Right_Pressed(object sender, EventArgs e)
        {
            if (!CanNavigate())
            {
                return;
            }

            if (InEditMode)
            {
                var currentStep = _stepButtons.Select((item, i) => new { Item = item, Index = i })
                    .FirstOrDefault(x => x.Item.IsActive)?.Index ?? -1;

                var nextStep = (currentStep + 1 + _stepButtons.Length) % _stepButtons.Length;
                for (int i = 0; i < _stepButtons.Length; i++)
                {
                    _stepButtons[i].IsActive = i == nextStep;
                }
                return;
            }

            var menuIndex = CurrentMenuIndex % _menus.Length;

            ToggleButton(menuIndex, (CurrentButtonIndex + 1) % _menus[menuIndex].Length);
        }

        private void Left_Pressed(object sender, EventArgs e)
        {
            if (!CanNavigate())
            {
                return;
            }

            if (InEditMode)
            {
                var currentStep = _stepButtons.Select((item, i) => new { Item = item, Index = i })
                    .FirstOrDefault(x => x.Item.IsActive)?.Index ?? 0;

                var nextStep = (currentStep - 1 + _stepButtons.Length) % _stepButtons.Length;
                for (int i = 0; i < _stepButtons.Length; i++)
                {
                    _stepButtons[i].IsActive = i == nextStep;
                }
                return;
            }

            var menuIndex = CurrentMenuIndex % _menus.Length;

            ToggleButton(menuIndex, (CurrentButtonIndex - 1 + _menus[menuIndex].Length) % _menus[menuIndex].Length);
        }

        private void HandleGlobalScrollUp()
        {
            if (!_watcher.IsLightroomActive())
            {
                return;
            }

            // This will be invoked on any upward scroll globally
            Dispatcher.Invoke(() =>
            {
                SendCommand(CommandType.UP);
            });
        }

        private void HandleGlobalScrollDown()
        {
            if (!_watcher.IsLightroomActive())
            {
                return;
            }

            // This will be invoked on any downward scroll globally
            Dispatcher.Invoke(() =>
            {
                SendCommand(CommandType.DOWN);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is LRControlButton clickedLRButton)
            {
                if (String.IsNullOrEmpty(clickedLRButton.LRCommand))
                {
                    return;
                }

                for (int i = 0; i < _menus.Length; i++)
                {
                    for (int j = 0; j < _menus[i].Length; j++)
                    {
                        if (_menus[i][j] == clickedLRButton)
                        {
                            if (CurrentButton.Type == ButtonType.LR && _menus[CurrentMenuIndex][CurrentButtonIndex] == clickedLRButton)
                            {
                                InEditMode = !InEditMode;
                            }
                            else
                            {
                                InEditMode = true;
                                ToggleButton(i, j);
                            }

                        }
                    }
                }
                return;
            }
            if (sender is MenuControlButton clickedMenuButton)
            {
                SwitchToMenu(clickedMenuButton.MenuCommand);
            }
        }
        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is LRControlButton clickedButton)
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

        private void DragButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            IsPinned = !IsPinned;
        }
    }
}

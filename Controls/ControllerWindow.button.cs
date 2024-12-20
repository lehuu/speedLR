using System.Windows;
using System.Windows.Input;
using SpeedLR.Controls;

namespace SpeedLR
{
    public partial class ControllerWindow : Window
    {
        private void Escape_Pressed(ref bool isHandled)
        {
            if (!CanNavigate())
            {
                return;
            }

            isHandled = true;
            Hide();
        }

        private void BackCtrl_Pressed(ref bool isHandled)
        {
            switch (CurrentButton.Type)
            {
                case ButtonType.LR:
                    SendCommand(CommandType.RESET);
                    isHandled = true;
                    return;
                case ButtonType.MENU:
                case ButtonType.NONE:
                default:
                    return;
            }
        }

        private void Reset_Pressed(ref bool isHandled)
        {
            SendCommand(CommandType.RESET);
            isHandled = true;
        }

        private bool CanNavigate()
        {
            return _watcher.IsLightroomActive && IsVisible && _menus.Length > 0;
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

        private void Ctrl_Pressed(ref bool isHandled)
        {
            switch (CurrentButton.Type)
            {
                case ButtonType.MENU:
                    SwitchToMenu(CurrentButton.Data);
                    isHandled = true;
                    return;
                case ButtonType.NONE:
                default:
                    return;
            }
        }

        private void Up_Pressed(ref bool isHandled)
        {
            if (!CanNavigate())
            {
                return;
            }

            var nextSubmenu = (CurrentMenuIndex - 1 + _menus.Length) % _menus.Length;
            ToggleButton(nextSubmenu, FindNextClosestButton(nextSubmenu));
            isHandled = true;
        }

        private void UpCtrl_Pressed(ref bool isHandled)
        {
            if (!CanNavigate())
            {
                return;
            }

            SendCommand(CommandType.UP);
            isHandled = true;
            return;
        }

        private void Down_Pressed(ref bool isHandled)
        {
            if (!CanNavigate())
            {
                return;
            }

            var nextSubmenu = (CurrentMenuIndex + 1) % _menus.Length;
            ToggleButton(nextSubmenu, FindNextClosestButton(nextSubmenu));
            isHandled = true;
        }

        private void DownCtrl_Pressed(ref bool isHandled)
        {
            if (!CanNavigate())
            {
                return;
            }

            SendCommand(CommandType.DOWN);
            isHandled = true;
            return;
        }

        private void Right_Pressed(ref bool isHandled)
        {
            if (!CanNavigate())
            {
                return;
            }

            var menuIndex = CurrentMenuIndex % _menus.Length;

            ToggleButton(menuIndex, (CurrentButtonIndex + 1) % _menus[menuIndex].Length);
            isHandled = true;
        }

        private void RightCtrl_Pressed(ref bool isHandled)
        {
            if (!CanNavigate())
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
            isHandled = true;
            return;
        }

        private void Left_Pressed(ref bool isHandled)
        {
            if (!CanNavigate())
            {
                return;
            }

            var menuIndex = CurrentMenuIndex % _menus.Length;

            ToggleButton(menuIndex, (CurrentButtonIndex - 1 + _menus[menuIndex].Length) % _menus[menuIndex].Length);
            isHandled = true;
        }

        private void LeftCtrl_Pressed(ref bool isHandled)
        {
            if (!CanNavigate())
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
            isHandled = true;
            return;
        }

        private void HandleGlobalScrollUp(ref bool isHandled)
        {
            if (!_watcher.IsLightroomActive)
            {
                return;
            }

            // This will be invoked on any upward scroll globally
            Dispatcher.Invoke(() =>
            {
                SendCommand(CommandType.UP);
            });
            isHandled = true;
        }

        private void HandleGlobalScrollDown(ref bool isHandled)
        {
            if (!_watcher.IsLightroomActive)
            {
                return;
            }

            // This will be invoked on any downward scroll globally
            Dispatcher.Invoke(() =>
            {
                SendCommand(CommandType.DOWN);
            });
            isHandled = true;
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
                                return;
                            }
                            ToggleButton(i, j);
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

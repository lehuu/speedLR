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

            if(CurrentButton.Type == ButtonType.NONE)
            {
                Backspace_Pressed(sender, e);
                return;
            }

            popup.Visibility = Visibility.Collapsed;
            ClearActiveButtons();
        }

        private void Next_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            ToggleButton(CurrentMenuIndex, (CurrentButtonIndex + 1) % _menus[CurrentMenuIndex].Length);
        }

        private void Next_Submenu(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            var nextSubmenu = (CurrentMenuIndex + 1) % _menus.Length;
            ToggleButton(nextSubmenu, Math.Clamp(CurrentButtonIndex, 0, _menus[nextSubmenu].Length - 1));
        }

        private void Prev_Submenu(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            var nextSubmenu = (CurrentMenuIndex - 1 + _menus.Length) % _menus.Length;
            ToggleButton(nextSubmenu, Math.Clamp(CurrentButtonIndex, 0, _menus[nextSubmenu].Length - 1));
        }

        private void Prev_Pressed(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }
            ToggleButton(CurrentMenuIndex, (CurrentButtonIndex - 1 + _menus[CurrentMenuIndex].Length) % _menus[CurrentMenuIndex].Length);
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
            if (CurrentButton.Type == ButtonType.MENU)
            {
                SwitchToMenu(CurrentButton.Data);
                _menuHistory.Add(_currentMenuId);
                backButton.IsEnabled = _menuHistory.Count > 1;
                return;
            }
            SendCommand(CommandType.DOWN);
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
                            ToggleButton(i, j);
                        }
                    }
                }
                return;
            }
            if (sender is MenuControlButton clickedMenuButton)
            {
                SwitchToMenu(clickedMenuButton.MenuCommand);
                _menuHistory.Add(_currentMenuId);
                backButton.IsEnabled = _menuHistory.Count > 1;
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

        private void Backspace_Pressed(object sender, EventArgs e)
        {
            if (_menuHistory.Count <= 1)
            {
                return;
            }
            _menuHistory.RemoveAt(_menuHistory.Count - 1);
            SwitchToMenu(_menuHistory[_menuHistory.Count - 1]);
            backButton.IsEnabled = _menuHistory.Count > 1;
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

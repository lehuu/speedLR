using System.Windows;
using System.Windows.Input;
using SpeedLR.Controls;

namespace SpeedLR
{
    public partial class ControllerWindow : Window
    {
        private void HandleClose(ref bool isHandled)
        {
            if (!CanNavigate())
            {
                return;
            }

            isHandled = true;
            Hide();
        }

        private void HandleReset(ref bool isHandled)
        {
            if (!(CanNavigate() && CurrentButton.Type == ButtonType.LR))
            {
                return;
            }
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

        private void HandleMenuChange(ref bool isHandled)
        {
            if (!(CanNavigate() && CurrentButton.Type == ButtonType.MENU))
            {
                return;
            }

            SwitchToMenu(CurrentButton.Data);
            isHandled = true;
        }

        private void HandleArrowKeys(ref bool isHandled, DirectionType direction)
        {
            if (!CanNavigate())
            {
                return;
            }

            var menuDelta = (direction == DirectionType.UP || direction == DirectionType.LEFT) ? -1 : 1;

            switch (direction)
            {
                case DirectionType.RIGHT:
                case DirectionType.LEFT:
                    var menuIndex = CurrentMenuIndex % _menus.Length;
                    ToggleButton(menuIndex, (CurrentButtonIndex + menuDelta + _menus[menuIndex].Length) % _menus[menuIndex].Length);
                    break;
                case DirectionType.UP:
                case DirectionType.DOWN:
                    var nextSubmenu = (CurrentMenuIndex + menuDelta + _menus.Length) % _menus.Length;
                    ToggleButton(nextSubmenu, FindNextClosestButton(nextSubmenu));
                    break;
            }

            isHandled = true;
        }



        private void HandleCtrlArrouKeys(ref bool isHandled, DirectionType direction)
        {
            if (!CanNavigate())
            {
                return;
            }

            switch (direction)
            {
                case DirectionType.RIGHT:
                case DirectionType.LEFT:
                    var currentStep = _stepButtons.Select((item, i) => new { Item = item, Index = i })
                          .FirstOrDefault(x => x.Item.IsActive)?.Index ?? (direction == DirectionType.RIGHT ? -1 : 0);

                    var stepDelta = (direction == DirectionType.RIGHT ? 1 : -1);

                    var nextStep = (currentStep + stepDelta + _stepButtons.Length) % _stepButtons.Length;
                    for (int i = 0; i < _stepButtons.Length; i++)
                    {
                        _stepButtons[i].IsActive = i == nextStep;
                    }

                    break;
                case DirectionType.UP:
                    SendCommand(CommandType.UP);
                    break;
                case DirectionType.DOWN:
                    SendCommand(CommandType.DOWN);
                    break;
            }


            isHandled = true;
        }

        private void HandleGlobalScroll(ref bool isHandled, CommandType direction)
        {
            if (!(CanNavigate() && CurrentButton.Type == ButtonType.LR))
            {
                return;
            }

            // This will be invoked on any upward scroll globally
            Dispatcher.Invoke(() =>
            {
                SendCommand(direction);
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

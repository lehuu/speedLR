using System.Windows;
using Button = System.Windows.Controls.Button;

namespace SpeedLR
{

    public partial class ControllerWindow : Window
    {
        private ControlButton[][] menus;
        private GlobalHotkey escapeHotkey;
        private GlobalHotkey nextControlHotkey;
        private GlobalHotkey prevControlHotkey;
        private GlobalHotkey increaseHotkey;
        private GlobalHotkey decreaseHotkey;

        private string currentCommand = "";

        public ControllerWindow()
        {
            InitializeComponent();

            var firstMenu = new ControlButton[3];
            firstMenu[0] = this.exposure;
            firstMenu[1] = this.shadows;
            firstMenu[2] = this.highlights;

            var secondMenu = new ControlButton[3];
            secondMenu[0] = this.blacks;
            secondMenu[1] = this.whites;
            secondMenu[2] = this.texture;

            menus = new ControlButton[2][];
            menus[0] = firstMenu;
            menus[1] = secondMenu;
        }

        private void NavigateControls()
        {

        }

        private void ControllerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void ToggleButton(ControlButton button)
        {
            foreach (var submenus in menus)
            {
                foreach (var item in submenus)
                {
                    if(button != null && item.Name == button.Name)
                    {
                        item.IsActive = !item.IsActive;
                        currentCommand = item.IsActive ? item.LRCommand : "";
                        continue;
                    }

                    item.IsActive = false;
                }
            }
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

                //Connector.Instance.SendCommandAsync(clickedButton.LRCommand + "=1%");
            }
        }
    }
}

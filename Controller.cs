namespace SpeedLR
{
    public partial class Controller : Form
    {
        public Controller()
        {
            InitializeComponent();
        }

        private void Controller_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Minimize to tray instead of closing
                e.Cancel = true;
                Hide();
            }
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReSpawn.Controls
{
    public partial class GameTileControl : UserControl
    {
        public static readonly DependencyProperty LaunchCommandProperty =
            DependencyProperty.Register("LaunchCommand", typeof(ICommand),
            typeof(GameTileControl));

        public ICommand LaunchCommand
        {
            get => (ICommand)GetValue(LaunchCommandProperty);
            set => SetValue(LaunchCommandProperty, value);
        }

        public GameTileControl()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (LaunchCommand?.CanExecute(DataContext) == true)
                    LaunchCommand.Execute(DataContext);
            }
        }
    }
}
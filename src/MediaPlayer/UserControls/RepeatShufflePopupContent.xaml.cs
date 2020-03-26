using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MediaPlayer.UserControls
{
    public sealed partial class RepeatShufflePopupContent : UserControl
    {
        public RepeatShufflePopupContent(Brush backgroundColor, object dataContext)
        {
            this.InitializeComponent();

            this.Container.Background = backgroundColor;
            this.DataContext = dataContext;

            this.Loaded += RepeatShufflePopupContent_Loaded;
        }

        void RepeatShufflePopupContent_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //if (Windows.UI.ViewManagement.ApplicationView.Value == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            //    Windows.UI.ViewManagement.ApplicationView.TryUnsnap();

            if (Windows.UI.ViewManagement.ApplicationView.Value == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                this.LoadSubtitlesButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.SubtitleLine.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                this.LoadSubtitlesButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.SubtitleLine.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
    }
}

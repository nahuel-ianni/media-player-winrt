using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MediaPlayer.UserControls
{
    public sealed partial class RemoveGroupPopupContent : UserControl
    {
        public RemoveGroupPopupContent(Brush backgroundColor, object dataContext, bool removeAllGroupsVisible = false)
        {
            this.InitializeComponent();

            this.Container.Background = backgroundColor;
            this.DataContext = dataContext;

            if (removeAllGroupsVisible)
            {
                this.RemoveGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.RemoveAllGroupsGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.RemoveGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.RemoveAllGroupsGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
    }
}

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MediaPlayer.UserControls
{
    public sealed partial class SortGroupsPopupContent : UserControl
    {
        public SortGroupsPopupContent(Brush backgroundColor, object dataContext)
        {
            this.InitializeComponent();

            this.Container.Background = backgroundColor;
            this.DataContext = dataContext;
        }
    }
}

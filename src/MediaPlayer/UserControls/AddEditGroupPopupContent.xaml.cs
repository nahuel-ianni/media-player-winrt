using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MediaPlayer.UserControls
{
    public sealed partial class AddEditGroupPopupContent : UserControl
    {
        #region Properties
        public string MediaGroupName
        {
            get { return this.GroupNameTextBox.Text; }
            set { this.GroupNameTextBox.Text = value; }
        }

        public event EventHandler NameAccepted;
        #endregion

        #region Initializers
        public AddEditGroupPopupContent(Brush backgroundColor) : this(backgroundColor, string.Empty) { }

        public AddEditGroupPopupContent(Brush backgroundColor, string mediaGroupName)
        {
            this.InitializeComponent();
            this.Loaded += AddEditGroupPopupContent_Loaded;

            MediaGroupName = mediaGroupName;
            this.Container.Background = backgroundColor;
        }
        #endregion

        #region Events
        private void AddEditGroupPopupContent_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
        }

        private void GroupNameTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (NameAccepted != null &&
                !string.IsNullOrEmpty(MediaGroupName) &&
                e.Key == Windows.System.VirtualKey.Enter)
            {
                NameAccepted(MediaGroupName, null);
            }
        }

        private void GroupNameTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox groupNameTextbox = ((TextBox)sender);

            groupNameTextbox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            groupNameTextbox.SelectAll();
        }
        #endregion
    }
}

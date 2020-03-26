using MediaPlayer.Converters;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaPlayer.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region Declarations
        protected Media.Manager mediaManager { get; set; }
        #endregion

        #region Initializers
        public BaseViewModel()
        {
            mediaManager = (Media.Manager)App.Current.Resources["mediaManager"];

            if (mediaManager == null)
                throw new Exception("No Media.Manager could be found on the application resources.");

            mediaManager.CurrentGroup = null;
            mediaManager.CurrentFile = null;
        }
        #endregion

        #region INotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace csharp.ui.controls.ProgressRing
{
    /// <summary>
    /// Interaction logic for Animation.xaml
    /// </summary>
    public partial class Animation : UserControl
    {
        #region Public Constructors

        public Animation()
        {
            InitializeComponent();

            TimeSpan ts = new TimeSpan(0, 0, 0, 1, 900);
            TimeSpan ts1 = new TimeSpan(0, 0, 0, 0, (int)ts.TotalMilliseconds / 17);
        }

        #endregion Public Constructors
    }

    public class TimeSpanConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}
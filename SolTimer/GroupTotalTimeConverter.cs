using System.Globalization;
using System.Windows.Data;

namespace SolTimer
{
    public class GroupTotalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CollectionViewGroup group)
            {
                var entries = group.Items.OfType<TimerEntry>();
                var totalTime = entries.Aggregate(TimeSpan.Zero, (total, entry) => total.Add(entry.Duration));
                return totalTime.ToString(@"hh\:mm\:ss");
            }
            return "00:00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
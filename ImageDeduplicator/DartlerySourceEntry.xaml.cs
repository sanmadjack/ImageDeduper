using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ImageDeduplicator.ImageSources;

namespace ImageDeduplicator {
    /// <summary>
    /// Interaction logic for DatabaseSourceEntry.xaml
    /// </summary>
    public partial class DartlerySourceEntry : Window {
        public DartlerySourceEntry() {
            InitializeComponent();
            this.addressText.Text = Properties.Settings.Default.LastDartleryAddress;
            this.imagePathText.Text = Properties.Settings.Default.LastDartleryImagePath;
            this.passwordText.Password= Properties.Settings.Default.LastDartleryPassword;
            this.tagsText.Text = Properties.Settings.Default.LastDartleryTags;
            this.userText.Text = Properties.Settings.Default.LastDartleryUser;
            String cutoffDateTimeString = Properties.Settings.Default.LastDartleryCutoffDate;
            DateTime dt;
            if(DateTime.TryParse(cutoffDateTimeString, out dt))
            {
                this.cutoffDate.SelectedDate = dt;
                this.cutoffTime.Text = dt.ToShortTimeString();
            }
        }

        private DateTime? selectedDateTime
        {
            get
            {
                if(cutoffDate.SelectedDate.HasValue)
                {
                    DateTime selected = cutoffDate.SelectedDate.Value;
                    DateTime output = new DateTime(selected.Year, selected.Month, selected.Day);
                    TimeSpan ts;
                    if(TimeSpan.TryParse(cutoffTime.Text, out ts))
                    {
                        output.AddTicks(ts.Ticks);
                    }
                    return output;
                }
                return null;
            }
        }

        public AImageSource getImageSource() {
            return new DartleryImageSource(this.addressText.Text, this.userText.Text,this.passwordText.Password, this.tagsText.Text, selectedDateTime,this.imagePathText.Text);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.LastDartleryAddress = this.addressText.Text;
            Properties.Settings.Default.LastDartleryImagePath = this.imagePathText.Text;
            Properties.Settings.Default.LastDartleryPassword = this.passwordText.Password;
            Properties.Settings.Default.LastDartleryTags = this.tagsText.Text;
            Properties.Settings.Default.LastDartleryUser = this.userText.Text;
            Properties.Settings.Default.LastDartleryCutoffDate = selectedDateTime.ToString();

            Properties.Settings.Default.Save();
            this.DialogResult = true;
            this.Close();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
        }
    }
}

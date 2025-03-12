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
    public partial class DatabaseSourceEntry : Window {
        public DatabaseSourceEntry() {
            InitializeComponent();
            this.connectionStringTxt.Text = Properties.Settings.Default.LastConnectionString;
            this.selectQueryText.Text = Properties.Settings.Default.LastQuery;
            this.deleteQueryText.Text = Properties.Settings.Default.LastDeleteQuery;
            this.nameText.Text = Properties.Settings.Default.LastQueryName;
        }

        public AImageSource getImageSource() {
            return new DatabaseImageSource(this.nameText.Text, DatabaseImageSource.PROVIDER_MYSQL, this.connectionStringTxt.Text, this.selectQueryText.Text, this.deleteQueryText.Text);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.LastQuery = this.selectQueryText.Text;
            Properties.Settings.Default.LastDeleteQuery = this.deleteQueryText.Text;
            Properties.Settings.Default.LastQueryName = this.nameText.Text;
            Properties.Settings.Default.LastConnectionString = this.connectionStringTxt.Text;
            Properties.Settings.Default.Save();
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
        }
    }
}

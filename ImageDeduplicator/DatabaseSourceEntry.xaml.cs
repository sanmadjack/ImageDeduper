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
            this.queryText.Text = Properties.Settings.Default.LastQuery;
            this.nameText.Text = Properties.Settings.Default.LastQueryName;
        }

        public AImageSource getImageSource() {
            return new DatabaseImageSource(this.nameText.Text, "mysql", this.connectionStringDropDown.Text, this.queryText.Text);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            if (!Properties.Settings.Default.connectionStrings.Contains(this.connectionStringDropDown.Text)) {
                MessageBoxResult result =  MessageBox.Show("Save connection string?", "Do you want to save this connection string?", MessageBoxButton.YesNoCancel);
                switch(result) {
                    case MessageBoxResult.Cancel:
                        return;
                    case MessageBoxResult.Yes:
                        Properties.Settings.Default.connectionStrings.Add(this.connectionStringDropDown.Text);
                        break;
                }
            }
            Properties.Settings.Default.LastQuery = this.queryText.Text;
            Properties.Settings.Default.LastQueryName = this.nameText.Text;
            Properties.Settings.Default.Save();
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (Properties.Settings.Default.connectionStrings == null)
                Properties.Settings.Default.connectionStrings = new System.Collections.Specialized.StringCollection();
            this.connectionStringDropDown.ItemsSource = Properties.Settings.Default.connectionStrings;
        }
    }
}

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
    public partial class ShimmieSourceEntry : Window {
        public ShimmieSourceEntry() {
            InitializeComponent();
            this.connectionStringTxt.Text = Properties.Settings.Default.LastConnectionString;
            this.tagsTxt.Text = Properties.Settings.Default.LastShimmieTags;
            this.imagePathTxt.Text = Properties.Settings.Default.LastShimmieImagePath;
            this.txtIdFrom.Text = Properties.Settings.Default.LastShimmieIDFrom.ToString();
            this.txtIdTo.Text = Properties.Settings.Default.LastShimmieIDTo.ToString();
        }

        public AImageSource getImageSource() {
            string database;
            if(rdoMysql.IsChecked.GetValueOrDefault(false))
            {
                database = DatabaseImageSource.PROVIDER_MYSQL;
            } else
            {
                database = DatabaseImageSource.PROVIDER_POSTGRES;
            }


            if (String.IsNullOrEmpty(this.tagsTxt.Text)) {
                return new ShimmieImageSource(database, this.connectionStringTxt.Text, long.Parse(this.txtIdFrom.Text), long.Parse(this.txtIdTo.Text), this.imagePathTxt.Text);
            } else {
                return new ShimmieImageSource(database, this.connectionStringTxt.Text, this.tagsTxt.Text, this.imagePathTxt.Text);
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            try {
                Properties.Settings.Default.LastShimmieTags = this.tagsTxt.Text;
                Properties.Settings.Default.LastConnectionString = this.connectionStringTxt.Text;
                Properties.Settings.Default.LastShimmieImagePath = this.imagePathTxt.Text;
                Properties.Settings.Default.LastShimmieIDFrom = long.Parse(this.txtIdFrom.Text);
                Properties.Settings.Default.LastShimmieIDTo = long.Parse(this.txtIdTo.Text);
                Properties.Settings.Default.Save();
                this.DialogResult = true;
                this.Close();
            } catch(Exception ex) {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
        }
    }
}

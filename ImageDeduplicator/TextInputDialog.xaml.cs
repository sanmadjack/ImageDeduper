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

namespace ImageDeduplicator {
    /// <summary>
    /// Interaction logic for TextInputDialog.xaml
    /// </summary>
    public partial class TextInputDialog : Window {
        public TextInputDialog(string title, bool show_invert_check = false) {
            InitializeComponent();
            this.Title = title;
            if(show_invert_check) {
                this.invertCheck.Visibility = Visibility.Visible;
            } else {
                this.invertCheck.Visibility = Visibility.Collapsed;
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        public string GetInput() {
            return this.inputTxt.Text;
        }

        public bool GetInvert() {
            return this.invertCheck.IsChecked.Value;
        }

        private void invertCheck_Click(object sender, RoutedEventArgs e) {

        }
    }
}

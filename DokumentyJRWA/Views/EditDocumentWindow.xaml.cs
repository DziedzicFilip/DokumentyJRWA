using System.Windows;

namespace DokumentyJRWA.Views
{
    public partial class EditDocumentWindow : Window
    {
        public EditDocumentWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

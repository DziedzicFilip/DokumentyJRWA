using System.Windows;

namespace DokumentyJRWA.Views
{
    public partial class AddDocumentWindow : Window
    {
        public AddDocumentWindow()
        {
            InitializeComponent();
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
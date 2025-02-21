using Microsoft.UI.Xaml.Controls;

namespace StudentManagementApp.Views
{
    public sealed partial class FacultyManagementPage : Page
    {
        public FacultyManagementPage()
        {
            this.InitializeComponent();
            this.DataContext = MainViewModel.Instance;
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}

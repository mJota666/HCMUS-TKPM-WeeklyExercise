using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StudentManagementApp.ViewModels;

namespace StudentManagementApp.Views
{
    public sealed partial class MainPage : Page
    {


        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = MainViewModel.Instance;
        }
        private void GoToFacultyManagementPage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FacultyManagementPage));
        }
        private void GoToProgramManagementPage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProgramManagementPage));
        }
        private void GoToStudentStatusManagementPage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StudentStatusManagementPage));
        }
    }
}

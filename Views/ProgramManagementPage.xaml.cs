using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagementApp.Views
{
    public sealed partial class ProgramManagementPage : Page
    {
        public ProgramManagementPage()
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

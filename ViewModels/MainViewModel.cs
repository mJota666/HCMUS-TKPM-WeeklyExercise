using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Dispatching;
using StudentManagementApp.Models;
using StudentManagementApp.Services;
// Alias our custom RelayCommand to avoid conflicts with CommunityToolkit.Mvvm.Input.RelayCommand
using CustomRelayCommand = StudentManagementApp.Utilities.RelayCommand;

namespace StudentManagementApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly StudentDataService _dataService;
        private readonly string _jsonFilePath = "Data/students.json";
        private readonly DispatcherQueue _dispatcherQueue;

        public ObservableCollection<Student> Students { get; set; } = new ObservableCollection<Student>();

        // Backing field for SelectedStudent.
        private Student _selectedStudent = new Student();
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent != null)
                {
                    // Unsubscribe from previous student's property changed event.
                    _selectedStudent.PropertyChanged -= SelectedStudent_PropertyChanged;
                }

                _selectedStudent = value;
                if (_selectedStudent != null)
                {
                    // Subscribe to the new student's property changed event.
                    _selectedStudent.PropertyChanged += SelectedStudent_PropertyChanged;
                }

                Debug.WriteLine("SelectedStudent changed");
                OnPropertyChanged(nameof(SelectedStudent));
                RaiseCommandCanExecuteChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        // Commands exposed to the View.
        public ICommand NewStudentCommand { get; }
        public ICommand AddStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand UpdateStudentCommand { get; }
        public ICommand SearchStudentCommand { get; }
        public ICommand LoadStudentsCommand { get; }

        public MainViewModel()
        {
            // Get the UI thread's DispatcherQueue.
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Initialize the data service with the file path.
            _dataService = new StudentDataService(_jsonFilePath);

            // Create a new student for data entry.
            SelectedStudent = new Student();

            // Initialize commands with their Execute and CanExecute delegates.
            NewStudentCommand = new CustomRelayCommand(o =>
            {
                SelectedStudent = new Student();
            });
            AddStudentCommand = new CustomRelayCommand(o => AddStudent(), o => CanAddOrUpdateStudent());
            DeleteStudentCommand = new CustomRelayCommand(o => DeleteStudent(), o => SelectedStudent != null);
            UpdateStudentCommand = new CustomRelayCommand(o => UpdateStudent(), o => SelectedStudent != null && CanAddOrUpdateStudent());
            SearchStudentCommand = new CustomRelayCommand(o => SearchStudent());
            LoadStudentsCommand = new CustomRelayCommand(async o => await LoadStudentsAsync());

            // Load initial data on a background thread.
            Task.Run(async () => await LoadStudentsAsync());
        }

        /// <summary>
        /// Event handler that is triggered when any property on SelectedStudent changes.
        /// This forces a re-check of the command conditions.
        /// </summary>
        private void SelectedStudent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Property '{e.PropertyName}' changed on SelectedStudent.");
            RaiseCommandCanExecuteChanged();
        }

        /// <summary>
        /// Loads the student list asynchronously from the data service.
        /// </summary>
        private async Task LoadStudentsAsync()
        {
            var students = await _dataService.LoadStudentsAsync();
            _dispatcherQueue.TryEnqueue(() =>
            {
                Students.Clear();
                foreach (var s in students)
                {
                    Debug.WriteLine(s.HoTen);
                    Students.Add(s);
                }
            });
        }

        /// <summary>
        /// Saves the student list asynchronously using the data service.
        /// </summary>
        private async void SaveStudentsAsync()
        {
            await _dataService.SaveStudentsAsync(Students.ToList());
            Debug.WriteLine("Saving Successfully!");
        }

        /// <summary>
        /// Validates the input values for adding or updating a student.
        /// The Add and Update buttons are enabled only if this method returns true.
        /// </summary>
        private bool CanAddOrUpdateStudent()
        {
            Debug.WriteLine("Validating input for Add/Update");
            // Ensure SelectedStudent is not null.
            if (SelectedStudent == null)
                return false;
            Debug.WriteLine(3);

            // Validate required fields.
            if (string.IsNullOrWhiteSpace(SelectedStudent.MSSV) ||
                string.IsNullOrWhiteSpace(SelectedStudent.HoTen) ||
                SelectedStudent.NgaySinh == default)
                return false;
            Debug.WriteLine(4);

            // Validate email format.
            if (!Regex.IsMatch(SelectedStudent.Email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return false;
            Debug.WriteLine(5);

            // Validate phone number (must be 10 or 11 digits).
            if (!Regex.IsMatch(SelectedStudent.SoDienThoai ?? "", @"^\d{10,11}$"))
                return false;
            Debug.WriteLine(6);

            // Validate department name.
            string[] validKhoa = new string[] { "Khoa Luật", "Khoa Tiếng Anh thương mại", "Khoa Tiếng Nhật", "Khoa Tiếng Pháp" };
            if (!validKhoa.Contains(SelectedStudent.Khoa))
                return false;
            Debug.WriteLine(7);

            // Validate student status.
            string[] validTinhTrang = new string[] { "Đang học", "Đã tốt nghiệp", "Đã thôi học", "Tạm dừng học" };
            if (!validTinhTrang.Contains(SelectedStudent.TinhTrang))
                return false;
            Debug.WriteLine(8);
            return true;
        }

        /// <summary>
        /// Adds a new student if the student with the same MSSV does not already exist.
        /// </summary>
        private void AddStudent()
        {
            // Check for duplicate MSSV.
            if (Students.Any(s => s.MSSV == SelectedStudent.MSSV))
            {
                Debug.WriteLine("Duplicate MSSV found. Student not added.");
                return;
            }

            var newStudent = new Student
            {
                MSSV = SelectedStudent.MSSV,
                HoTen = SelectedStudent.HoTen,
                NgaySinh = SelectedStudent.NgaySinh,
                GioiTinh = SelectedStudent.GioiTinh,
                Khoa = SelectedStudent.Khoa,
                KhoaHoc = SelectedStudent.KhoaHoc,
                ChuongTrinh = SelectedStudent.ChuongTrinh,
                DiaChi = SelectedStudent.DiaChi,
                Email = SelectedStudent.Email,
                SoDienThoai = SelectedStudent.SoDienThoai,
                TinhTrang = SelectedStudent.TinhTrang
            };

            Students.Add(newStudent);
            SaveStudentsAsync();
        }

        /// <summary>
        /// Deletes the currently selected student.
        /// </summary>
        private void DeleteStudent()
        {
            if (SelectedStudent != null)
            {
                Students.Remove(SelectedStudent);
                SaveStudentsAsync();
            }
        }

        /// <summary>
        /// Saves changes to the selected student. With two‑way binding, updates are automatic.
        /// </summary>
        private void UpdateStudent()
        {
            SaveStudentsAsync();
        }

        /// <summary>
        /// Searches the Students collection by MSSV or HoTen based on the SearchText.
        /// </summary>
        private void SearchStudent()
        {
            var filtered = Students.Where(s =>
                (!string.IsNullOrEmpty(s.MSSV) && s.MSSV.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(s.HoTen) && s.HoTen.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            Students.Clear();
            foreach (var s in filtered)
            {
                Students.Add(s);
            }
        }

        /// <summary>
        /// Helper method to notify commands that depend on the validity of SelectedStudent.
        /// </summary>
        private void RaiseCommandCanExecuteChanged()
        {
            (AddStudentCommand as CustomRelayCommand)?.RaiseCanExecuteChanged();
            (UpdateStudentCommand as CustomRelayCommand)?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

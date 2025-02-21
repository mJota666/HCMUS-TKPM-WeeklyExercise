using System;
using System.IO;
using System.Reflection;
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
using StudentManagementApp.Helpers;
// Alias our custom RelayCommand to avoid conflicts with CommunityToolkit.Mvvm.Input.RelayCommand
using CustomRelayCommand = StudentManagementApp.Utilities.RelayCommand;
using System.Text;
using Windows.Storage;
using System.Text.Json;

using StudentManagementApp.Extensions;


namespace StudentManagementApp.ViewModels
{

    public class MainViewModel : INotifyPropertyChanged
    {
        // Export JSON and CSV
        // App-version and Build Date
        public string AppVersion => Helpers.AppInfoHelper.GetAppVersion();
        public string BuildDate
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                // Get the last write time of the assembly file.
                DateTime buildDate = File.GetLastWriteTime(assembly.Location);
                return buildDate.ToString("yyyy-MM-dd");
            }
        }        
        // Singleton
        private static MainViewModel _instance = new MainViewModel();
        public static MainViewModel Instance => _instance;

        private readonly StudentDataService _dataService;
        private readonly string _jsonFilePath = "Data/students.json";
        private readonly DispatcherQueue _dispatcherQueue;

        // Student collection loaded from JSON.
        public ObservableCollection<Student> Students { get; set; } = new ObservableCollection<Student>();

        // Lookup collections for Faculty, Student Status, and Program.
        public ObservableCollection<string> Faculties { get; set; } = new ObservableCollection<string>
        {
            "Công nghệ Thông tin",
            "Toán",
            "Điện tử Viễn thông",
            "Vật lý"
        };

        public ObservableCollection<string> StudentStatus { get; set; } = new ObservableCollection<string>
        {
            "Đang học",
            "Đã tốt nghiệp"
        };

        public ObservableCollection<string> Programs { get; set; } = new ObservableCollection<string>
        {
            "Chất lượng cao",
            "Cử nhân tài năng",
            "Việt Pháp",
            "Chính quy"
        };

        // Backing field for SelectedStudent.
        private Student _selectedStudent = new Student();
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent != null)
                {
                    // Unsubscribe from previous student's PropertyChanged event.
                    _selectedStudent.PropertyChanged -= SelectedStudent_PropertyChanged;
                }

                _selectedStudent = value;
                if (_selectedStudent != null)
                {
                    // Subscribe to new student's PropertyChanged event.
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

        private string _selectedFaculty = string.Empty;
        public string SelectedFaculty
        {
            get => _selectedFaculty;
            set
            {
                if (_selectedFaculty != value)
                {
                    _selectedFaculty = value;
                    OnPropertyChanged(nameof(SelectedFaculty));
                    FacultyToRename = value;

                }
            }
        }

        private string _facultyToRename = string.Empty;
        public string FacultyToRename
        {
            get => _facultyToRename;
            set
            {
                if (_facultyToRename != value)
                {
                    _facultyToRename = value;
                    OnPropertyChanged(nameof(FacultyToRename));
                }
            }
        }

        private string _selectedProgram = string.Empty;
        public string SelectedProgram
        {
            get => _selectedProgram;
            set
            {
                if (_selectedProgram != value)
                {
                    _selectedProgram = value;
                    OnPropertyChanged(nameof(SelectedProgram));
                    // Update the rename textbox value:
                    ProgramToRename = value;
                }
            }
        }

        private string _programToRename = string.Empty;
        public string ProgramToRename
        {
            get => _programToRename;
            set
            {
                if (_programToRename != value)
                {
                    _programToRename = value;
                    OnPropertyChanged(nameof(ProgramToRename));
                }
            }
        }

        private string _selectedStudentStatus = string.Empty;
        public string SelectedStudentStatus
        {
            get => _selectedStudentStatus;
            set
            {
                if (_selectedStudentStatus != value)
                {
                    _selectedStudentStatus = value;
                    OnPropertyChanged(nameof(SelectedStudentStatus));
                    StudentStatusToRename = value;
                }
            }
        }
        private string _studentStatusToRename = string.Empty;
        public string StudentStatusToRename
        {
            get => _studentStatusToRename;
            set
            {
                if (_studentStatusToRename != value)
                {
                    _studentStatusToRename = value;
                    OnPropertyChanged(nameof(StudentStatusToRename));
                }
            }
        }


        // Commands for student management.
        public ICommand NewStudentCommand { get; }
        public ICommand AddStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand UpdateStudentCommand { get; }
        public ICommand SearchStudentCommand { get; }
        public ICommand LoadStudentsCommand { get; }

        // Commands for updating lookup lists.
        public ICommand AddFacultyCommand { get; }
        public ICommand RenameFacultyCommand { get; }

        public ICommand AddProgramCommand { get; }
        public ICommand RenameProgramCommand { get; }
        public ICommand AddStudentStatusCommand { get; }

        public ICommand RenameStudentStatusCommand { get; }

        // Commands for Export.
        public ICommand ExportJsonCommand { get; }
        public ICommand ExportCsvCommand { get; }

        public MainViewModel()
        {

            // Capture the UI thread's DispatcherQueue.
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Initialize the data service with the JSON file path.
            _dataService = new StudentDataService(_jsonFilePath);

            // Create a new student for data entry.
            SelectedStudent = new Student();

            // Initialize student commands.
            NewStudentCommand = new CustomRelayCommand(o =>
            {
                SelectedStudent = new Student();
            });
            AddStudentCommand = new CustomRelayCommand(o => AddStudent(), o => CanAddOrUpdateStudent());
            DeleteStudentCommand = new CustomRelayCommand(o => DeleteStudent(), o => SelectedStudent != null);
            UpdateStudentCommand = new CustomRelayCommand(o => UpdateStudent(), o => SelectedStudent != null && CanAddOrUpdateStudent());
            SearchStudentCommand = new CustomRelayCommand(o => SearchStudent());
            LoadStudentsCommand = new CustomRelayCommand(async o => await LoadStudentsAsync());

            // Initialize lookup commands.
            AddFacultyCommand = new CustomRelayCommand(o =>
            {
                if (o is string newFaculty && !string.IsNullOrWhiteSpace(newFaculty))
                {
                    if (!Faculties.Contains(newFaculty))
                    {
                        Faculties.Add(newFaculty);
                    }
                }
            });
            RenameFacultyCommand = new CustomRelayCommand(o =>
            {

                // Rename the selected faculty to the value in FacultyToRename.
                if (!string.IsNullOrWhiteSpace(FacultyToRename) && !string.IsNullOrEmpty(SelectedFaculty))
                {
                    int index = Faculties.IndexOf(SelectedFaculty);
                    if (index >= 0)
                    {
                        // Update the collection.
                        Faculties[index] = FacultyToRename;
                        // Update SelectedFaculty to reflect the new name.
                        SelectedFaculty = FacultyToRename;
                    }
                }
            });
            AddProgramCommand = new CustomRelayCommand(o =>
            {
                if (o is string newProgram && !string.IsNullOrWhiteSpace(newProgram))
                {
                    if (!Programs.Contains(newProgram))
                    {
                        Programs.Add(newProgram);
                    }
                }
            });
            RenameProgramCommand = new CustomRelayCommand(o =>
            {
                if (!string.IsNullOrEmpty(ProgramToRename) && !string.IsNullOrEmpty(SelectedProgram))
                {
                    int index = Programs.IndexOf(SelectedProgram);
                    if (index >= 0)
                    {
                        // Update the collection
                        Programs[index] = ProgramToRename;
                        // Update SelectedProgram to reflect the new name
                        SelectedProgram = ProgramToRename;
                    }
                }
            });
            AddStudentStatusCommand = new CustomRelayCommand(o =>
            {
                if (o is string newStatus && !string.IsNullOrWhiteSpace(newStatus))
                {
                    if (!StudentStatus.Contains(newStatus))
                    {
                        StudentStatus.Add(newStatus);
                    }
                }
            });
            RenameStudentStatusCommand = new CustomRelayCommand(o => 
            {
                if (!string.IsNullOrEmpty(StudentStatusToRename) && !string.IsNullOrEmpty(SelectedStudentStatus))
                {
                    int index = StudentStatus.IndexOf(SelectedStudentStatus);
                    if (index >= 0)
                    {
                        StudentStatus[index] = StudentStatusToRename;
                        SelectedStudentStatus = StudentStatusToRename;
                    }
                }
            });

            // Export commands.
            ExportJsonCommand = new CustomRelayCommand(async o => await ExportToJsonAsync());
            ExportCsvCommand = new CustomRelayCommand(async o => await ExportToCsvAsync());

            // Load initial student data on a background thread.
            Task.Run(async () => await LoadStudentsAsync());
        }

        /// <summary>
        /// Event handler that is triggered when any property on SelectedStudent changes.
        /// This forces a re-check of the command conditions.
        /// </summary>
        private void SelectedStudent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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

            // Validate required fields.
            if (string.IsNullOrWhiteSpace(SelectedStudent.MSSV) ||
                string.IsNullOrWhiteSpace(SelectedStudent.HoTen) ||
                SelectedStudent.NgaySinh == default)
                return false;

            // Validate email format.
            if (!Regex.IsMatch(SelectedStudent.Email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return false;

            // Validate phone number (must be 10 or 11 digits).
            if (!Regex.IsMatch(SelectedStudent.SoDienThoai ?? "", @"^\d{10,11}$"))
                return false;

            // Validate Program: The SelectedStudent.ChuongTrinh must exist in the Programs list.
            if (!Programs.Contains(SelectedStudent.ChuongTrinh))
                return false;

            // Validate Faculty: The SelectedStudent.Khoa must exist in the Faculties list.
            if (!Faculties.Contains(SelectedStudent.Khoa))
                return false;

            // Validate student status.
            if (!StudentStatus.Contains(SelectedStudent.TinhTrang))
                return false;

            Debug.WriteLine("Validation passed");
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
        /// Searches the Students collection by MSSV, HoTen, or Khoa based on the SearchText.
        /// </summary>
        private void SearchStudent()
        {
            var filtered = Students.Where(s =>
                (!string.IsNullOrEmpty(s.MSSV) && s.MSSV.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(s.HoTen) && s.HoTen.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(s.Khoa) && s.Khoa.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            Students.Clear();
            foreach (var s in filtered)
            {
                Students.Add(s);
            }
        }
        //Export the current student list to a JSON file.
        //private async Task ExportToJsonAsync()
        //{
        //    Debug.WriteLine("Export Json ne");
        //    try
        //    {
        //        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        //        StorageFile file = await localFolder.CreateFileAsync("students_export.json", CreationCollisionOption.ReplaceExisting);
        //        using (Stream stream = await file.OpenStreamForWriteAsync())
        //        {
        //            var options = new JsonSerializerOptions { WriteIndented = true };
        //            await JsonSerializer.SerializeAsync(stream, Students.ToList(), options);
        //        }
        //        Debug.WriteLine("Exported JSON successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error exporting JSON: {ex.Message}");
        //    }
        //}
        private async Task ExportToJsonAsync()
        {
            try
            {
                // Get the local app data folder path using .NET APIs.
                string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = Path.Combine(localFolderPath, "students_export.json");

                var options = new JsonSerializerOptions { WriteIndented = true };

                // Create or overwrite the file.
                using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await JsonSerializer.SerializeAsync(stream, Students.ToList(), options);
                }
                Debug.WriteLine($"Exported JSON successfully to {filePath}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting JSON: {ex.Message}");
            }
        }




        // Export the current student list to a CSV file.
        //private async Task ExportToCsvAsync()
        //{
        //    try
        //    {
        //        StringBuilder csvContent = new StringBuilder();
        //        csvContent.AppendLine("MSSV,HoTen,NgaySinh,GioiTinh,Khoa,KhoaHoc,ChuongTrinh,DiaChi,Email,SoDienThoai,TinhTrang");
        //        foreach (var student in Students)
        //        {
        //            csvContent.AppendLine($"{student.MSSV},{student.HoTen},{student.NgaySinh:yyyy-MM-dd},{student.GioiTinh},{student.Khoa},{student.KhoaHoc},{student.ChuongTrinh},{student.DiaChi},{student.Email},{student.SoDienThoai},{student.TinhTrang}");
        //        }
        //        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        //        StorageFile file = await localFolder.CreateFileAsync("students_export.csv", CreationCollisionOption.ReplaceExisting);
        //        await FileIO.WriteTextAsync(file, csvContent.ToString());
        //        Debug.WriteLine("Exported CSV successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error exporting CSV: {ex.Message}");
        //    }
        //}
        private async Task ExportToCsvAsync()
        {
            try
            {
                StringBuilder csvContent = new StringBuilder();
                csvContent.AppendLine("MSSV,HoTen,NgaySinh,GioiTinh,Khoa,KhoaHoc,ChuongTrinh,DiaChi,Email,SoDienThoai,TinhTrang");
                foreach (var student in Students)
                {
                    csvContent.AppendLine($"{student.MSSV},{student.HoTen},{student.NgaySinh:yyyy-MM-dd},{student.GioiTinh},{student.Khoa},{student.KhoaHoc},{student.ChuongTrinh},{student.DiaChi},{student.Email},{student.SoDienThoai},{student.TinhTrang}");
                }

                string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = Path.Combine(localFolderPath, "students_export.csv");

                // Write CSV content to file.
                await File.WriteAllTextAsync(filePath, csvContent.ToString());
                Debug.WriteLine($"Exported CSV successfully to {filePath}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting CSV: {ex.Message}");
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

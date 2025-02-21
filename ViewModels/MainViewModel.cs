using System;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Dispatching;
using StudentManagementApp.Models;
using StudentManagementApp.Services;
using StudentManagementApp.Helpers;
// Alias our custom RelayCommand to avoid conflicts with CommunityToolkit.Mvvm.Input.RelayCommand
using CustomRelayCommand = StudentManagementApp.Utilities.RelayCommand;
using System.Text.Json;
using Windows.Storage;

namespace StudentManagementApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // App version and build date.
        public string AppVersion => AppInfoHelper.GetAppVersion();
        public string BuildDate
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                DateTime buildDate = File.GetLastWriteTime(assembly.Location);
                return buildDate.ToString("yyyy-MM-dd");
            }
        }

        // Singleton instance.
        private static MainViewModel _instance = new MainViewModel();
        public static MainViewModel Instance => _instance;

        private readonly StudentDataService _dataService;
        private readonly string _jsonFilePath = "Data/students.json";
        private readonly DispatcherQueue _dispatcherQueue;

        // Student collection loaded from JSON.
        public ObservableCollection<Student> Students { get; set; } = new ObservableCollection<Student>();

        // Lookup collections.
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

        // Student management properties.
        private Student _selectedStudent = new Student();
        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent != null)
                {
                    _selectedStudent.PropertyChanged -= SelectedStudent_PropertyChanged;
                }
                _selectedStudent = value;
                if (_selectedStudent != null)
                {
                    _selectedStudent.PropertyChanged += SelectedStudent_PropertyChanged;
                }
                Debug.WriteLine("SelectedStudent changed");
                SimpleLogger.LogInfo("SelectedStudent changed.");
                OnPropertyChanged(nameof(SelectedStudent));
                RaiseCommandCanExecuteChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        // Lookup management for Faculty.
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

        // Lookup management for Program.
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

        // Lookup management for Student Status.
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

        // Commands for lookup management.
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
            // Capture the UI thread's dispatcher.
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Initialize the data service.
            _dataService = new StudentDataService(_jsonFilePath);

            // Create a new student for data entry.
            SelectedStudent = new Student();

            // Initialize student management commands.
            NewStudentCommand = new CustomRelayCommand(o => SelectedStudent = new Student());
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
                        SimpleLogger.LogInfo($"Added new faculty: {newFaculty}");
                    }
                }
            });
            RenameFacultyCommand = new CustomRelayCommand(o =>
            {
                if (!string.IsNullOrWhiteSpace(FacultyToRename) && !string.IsNullOrEmpty(SelectedFaculty))
                {
                    int index = Faculties.IndexOf(SelectedFaculty);
                    if (index >= 0)
                    {
                        Faculties[index] = FacultyToRename;
                        SelectedFaculty = FacultyToRename;
                        SimpleLogger.LogInfo($"Renamed faculty to: {FacultyToRename}");
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
                        SimpleLogger.LogInfo($"Added new program: {newProgram}");
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
                        Programs[index] = ProgramToRename;
                        SelectedProgram = ProgramToRename;
                        SimpleLogger.LogInfo($"Renamed program to: {ProgramToRename}");
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
                        SimpleLogger.LogInfo($"Added new student status: {newStatus}");
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
                        SimpleLogger.LogInfo($"Renamed student status to: {StudentStatusToRename}");
                    }
                }
            });

            // Initialize export commands.
            ExportJsonCommand = new CustomRelayCommand(async o => await ExportToJsonAsync());
            ExportCsvCommand = new CustomRelayCommand(async o => await ExportToCsvAsync());

            // Load initial student data on a background thread.
            Task.Run(async () => await LoadStudentsAsync());
        }

        private void SelectedStudent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Property '{e.PropertyName}' changed on SelectedStudent.");
            SimpleLogger.LogInfo($"Property '{e.PropertyName}' changed on SelectedStudent.");
            RaiseCommandCanExecuteChanged();
        }

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
            SimpleLogger.LogInfo($"Loaded {students.Count} students.");
        }

        private async void SaveStudentsAsync()
        {
            await _dataService.SaveStudentsAsync(Students.ToList());
            Debug.WriteLine("Saving Successfully!");
            SimpleLogger.LogInfo($"Saved {Students.Count} students successfully.");
        }

        private bool CanAddOrUpdateStudent()
        {
            Debug.WriteLine("Validating input for Add/Update");
            if (SelectedStudent == null)
                return false;

            if (string.IsNullOrWhiteSpace(SelectedStudent.MSSV) ||
                string.IsNullOrWhiteSpace(SelectedStudent.HoTen) ||
                SelectedStudent.NgaySinh == default)
                return false;

            if (!Regex.IsMatch(SelectedStudent.Email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return false;

            if (!Regex.IsMatch(SelectedStudent.SoDienThoai ?? "", @"^\d{10,11}$"))
                return false;

            if (!Programs.Contains(SelectedStudent.ChuongTrinh))
                return false;

            if (!Faculties.Contains(SelectedStudent.Khoa))
                return false;

            if (!StudentStatus.Contains(SelectedStudent.TinhTrang))
                return false;

            Debug.WriteLine("Validation passed");
            SimpleLogger.LogInfo("Validation passed for Add/Update.");
            return true;
        }

        private void AddStudent()
        {
            if (Students.Any(s => s.MSSV == SelectedStudent.MSSV))
            {
                Debug.WriteLine("Duplicate MSSV found. Student not added.");
                SimpleLogger.LogWarning($"Duplicate MSSV {SelectedStudent.MSSV} found. Student not added.");
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
            SimpleLogger.LogInfo($"Added new student with MSSV: {newStudent.MSSV}");
        }

        private void DeleteStudent()
        {
            if (SelectedStudent != null)
            {
                Students.Remove(SelectedStudent);
                SaveStudentsAsync();
                SimpleLogger.LogInfo("Deleted a student.");
            }
        }

        private void UpdateStudent()
        {
            SaveStudentsAsync();
            SimpleLogger.LogInfo("Updated student information.");
        }

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
            SimpleLogger.LogInfo($"Search completed with {filtered.Count} results.");
        }

        private async Task ExportToJsonAsync()
        {
            try
            {
                // Use .NET file I/O to export JSON.
                string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = Path.Combine(localFolderPath, "students_export.json");

                var options = new JsonSerializerOptions { WriteIndented = true };

                using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await JsonSerializer.SerializeAsync(stream, Students.ToList(), options);
                }
                Debug.WriteLine($"Exported JSON successfully to {filePath}.");
                SimpleLogger.LogInfo($"Exported JSON successfully to {filePath}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting JSON: {ex.Message}");
                SimpleLogger.LogError($"Error exporting JSON: {ex.Message}");
            }
        }

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

                await File.WriteAllTextAsync(filePath, csvContent.ToString());
                Debug.WriteLine($"Exported CSV successfully to {filePath}.");
                SimpleLogger.LogInfo($"Exported CSV successfully to {filePath}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting CSV: {ex.Message}");
                SimpleLogger.LogError($"Error exporting CSV: {ex.Message}");
            }
        }

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

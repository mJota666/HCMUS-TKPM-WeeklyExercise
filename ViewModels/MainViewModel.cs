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
        public static MainViewModel _instance = new MainViewModel();
        public static MainViewModel Instance => _instance;

        // Configurable business rule properties.
        public string AllowedEmailDomain { get; set; } = "@student.university.edu.vn";
        public string PhoneRegexPattern { get; set; } = @"^(\+84\d{9}|0[35789]\d{8})$";

        // Flag to enable/disable business rules.
        public bool IsBusinessRulesEnabled { get; set; } = true;

        // For status transitions.
        public string _originalStudentStatus = string.Empty;
        public string OriginalStudentStatus
        {
            get => _originalStudentStatus;
            set
            {
                if (_originalStudentStatus != value)
                {
                    _originalStudentStatus = value;
                    OnPropertyChanged(nameof(OriginalStudentStatus));
                }
            }
        }

        public readonly StudentDataService _dataService;
        public readonly string _jsonFilePath = "Data/students.json";
        public readonly DispatcherQueue _dispatcherQueue;

        // Deletion service to enforce deletion time window.
        private readonly DeletionService _deletionService;

        // NEW: Export confirmation service (for HTML/Markdown).
        private readonly ExportConfirmationService _exportConfirmationService = new ExportConfirmationService();

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
            "Tốt nghiệp",
            "Bảo lưu",
            "Đình chỉ"
        };

        public ObservableCollection<string> Programs { get; set; } = new ObservableCollection<string>
        {
            "Chất lượng cao",
            "Cử nhân tài năng",
            "Việt Pháp",
            "Chính quy"
        };

        // Student management properties.
        public Student _selectedStudent = new Student();
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
                    // When selecting a student, store the original status.
                    OriginalStudentStatus = _selectedStudent.TinhTrang;
                }
                Debug.WriteLine("SelectedStudent changed");
                SimpleLogger.LogInfo("SelectedStudent changed.");
                OnPropertyChanged(nameof(SelectedStudent));
                RaiseCommandCanExecuteChanged();
            }
        }

        public string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        // Lookup management for Faculty.
        public string _selectedFaculty = string.Empty;
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

        public string _facultyToRename = string.Empty;
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
        public string _selectedProgram = string.Empty;
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

        public string _programToRename = string.Empty;
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
        public string _selectedStudentStatus = string.Empty;
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

        public string _studentStatusToRename = string.Empty;
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
        // Commands for Delete
        public ICommand DeleteFacultyCommand { get; }
        public ICommand DeleteProgramCommand { get; }
        public ICommand DeleteStudentStatusCommand { get; }

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
        // NEW: Commands for exporting confirmation letters.
        public ICommand ExportConfirmationHtmlCommand { get; }
        public ICommand ExportConfirmationMarkdownCommand { get; }

        public MainViewModel()
        {
            // Capture the UI thread's dispatcher.
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Initialize the data service.
            _dataService = new StudentDataService(_jsonFilePath);

            // Initialize the deletion service with a 30-minute window.
            _deletionService = new DeletionService(TimeSpan.FromMinutes(30));

            // Create a new student for data entry.
            SelectedStudent = new Student();

            // Delete Logic
            DeleteFacultyCommand = new CustomRelayCommand(o => DeleteFaculty(), o => CanDeleteFaculty());
            DeleteProgramCommand = new CustomRelayCommand(o => DeleteProgram(), o => CanDeleteProgram());
            DeleteStudentStatusCommand = new CustomRelayCommand(o => DeleteStudentStatus(), o => CanDeleteStudentStatus());


            // Initialize student management commands.
            NewStudentCommand = new CustomRelayCommand(o => SelectedStudent = new Student());
            AddStudentCommand = new CustomRelayCommand(o => AddStudent(), o => CanAddOrUpdateStudent());
            DeleteStudentCommand = new CustomRelayCommand(o => DeleteStudent(), o => SelectedStudent != null);
            UpdateStudentCommand = new CustomRelayCommand(o => UpdateStudent(), o => SelectedStudent != null && CanAddOrUpdateStudent());
            SearchStudentCommand = new CustomRelayCommand(o => SearchStudent());
            LoadStudentsCommand = new CustomRelayCommand(async o => await LoadStudentsAsync());

            // Initialize lookup commands for Faculty.
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

            // Initialize lookup commands for Program.
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

            // Initialize lookup commands for Student Status.
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

            // NEW: Initialize confirmation export commands.
            ExportConfirmationHtmlCommand = new CustomRelayCommand(async o => await ExportConfirmationToHtmlAsync());
            ExportConfirmationMarkdownCommand = new CustomRelayCommand(async o => await ExportConfirmationToMarkdownAsync());

            // Load initial student data on a background thread.
            Task.Run(async () => await LoadStudentsAsync());
        }
        private bool CanDeleteFaculty()
        {
            //if (string.IsNullOrEmpty(SelectedFaculty))
            //    return false;
             return !Students.Any(s => s.Khoa.Equals(SelectedFaculty, StringComparison.OrdinalIgnoreCase));
        }

        private void DeleteFaculty()
        {
            Debug.WriteLine("Debug Delete Faculty !");
            if (CanDeleteFaculty())
            {
                Faculties.Remove(SelectedFaculty);
                SimpleLogger.LogInfo($"Deleted faculty: {SelectedFaculty}");
                SelectedFaculty = string.Empty; // Optionally clear the selection.
            }
            else
            {
                SimpleLogger.LogWarning($"Cannot delete faculty '{SelectedFaculty}': It is referenced by one or more students.");
            }
        }

        private bool CanDeleteProgram()
        {
            // Only allow deletion if a program is selected and no student is referencing it.
            //if (string.IsNullOrEmpty(SelectedProgram))
            //    return false;

            return !Students.Any(s => s.ChuongTrinh.Equals(SelectedProgram, StringComparison.OrdinalIgnoreCase));
        }

        private void DeleteProgram()
        {
            if (CanDeleteProgram())
            {
                Programs.Remove(SelectedProgram);
                SimpleLogger.LogInfo($"Deleted program: {SelectedProgram}");
                SelectedProgram = string.Empty; // Optionally clear the selection.
            }
            else
            {
                SimpleLogger.LogWarning($"Cannot delete program '{SelectedProgram}': It is referenced by one or more students.");
            }
        }

        private bool CanDeleteStudentStatus()
        {
            // Only allow deletion if a student status is selected and no student is referencing it.
            //if (string.IsNullOrEmpty(SelectedStudentStatus))
            //    return false;

            return !Students.Any(s => s.TinhTrang.Equals(SelectedStudentStatus, StringComparison.OrdinalIgnoreCase));
        }

        private void DeleteStudentStatus()
        {
            if (CanDeleteStudentStatus())
            {
                StudentStatus.Remove(SelectedStudentStatus);
                SimpleLogger.LogInfo($"Deleted student status: {SelectedStudentStatus}");
                SelectedStudentStatus = string.Empty; // Optionally clear the selection.
            }
            else
            {
                SimpleLogger.LogWarning($"Cannot delete student status '{SelectedStudentStatus}': It is referenced by one or more students.");
            }
        }

        public void SelectedStudent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Property '{e.PropertyName}' changed on SelectedStudent.");
            SimpleLogger.LogInfo($"Property '{e.PropertyName}' changed on SelectedStudent.");
            RaiseCommandCanExecuteChanged();
        }

        public async Task LoadStudentsAsync()
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

        public async void SaveStudentsAsync()
        {
            await _dataService.SaveStudentsAsync(Students.ToList());
            Debug.WriteLine("Saving Successfully!");
            SimpleLogger.LogInfo($"Saved {Students.Count} students successfully.");
        }

        public bool CanAddOrUpdateStudent()
        {
            Debug.WriteLine("Validating input for Add/Update");
            if (SelectedStudent == null)
                return false;

            if (string.IsNullOrWhiteSpace(SelectedStudent.MSSV) ||
                string.IsNullOrWhiteSpace(SelectedStudent.HoTen) ||
                SelectedStudent.NgaySinh == default)
                return false;

            // If business rules are disabled, bypass additional validations.
            if (!IsBusinessRulesEnabled)
                return true;

            // Email must end with the allowed domain.
            if (string.IsNullOrWhiteSpace(SelectedStudent.Email) ||
                !SelectedStudent.Email.EndsWith(AllowedEmailDomain, StringComparison.OrdinalIgnoreCase))
                return false;

            // Validate phone number using the configured pattern.
            if (!Regex.IsMatch(SelectedStudent.SoDienThoai ?? "", PhoneRegexPattern))
                return false;

            // Business rule for student status transitions.
            if (!string.IsNullOrEmpty(OriginalStudentStatus))
            {
                if (OriginalStudentStatus.Equals("Tốt nghiệp", StringComparison.OrdinalIgnoreCase))
                {
                    if (!SelectedStudent.TinhTrang.Equals("Tốt nghiệp", StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                else if (OriginalStudentStatus.Equals("Đang học", StringComparison.OrdinalIgnoreCase))
                {
                    string[] allowedFromDangHoc = { "Bảo lưu", "Tốt nghiệp", "Đình chỉ", "Đang học" };
                    if (!allowedFromDangHoc.Contains(SelectedStudent.TinhTrang))
                        return false;
                }
                else if (OriginalStudentStatus.Equals("Bảo lưu", StringComparison.OrdinalIgnoreCase))
                {
                    string[] allowedFromBaoLuu = { "Đang học", "Đình chỉ", "Bảo lưu" };
                    if (!allowedFromBaoLuu.Contains(SelectedStudent.TinhTrang))
                        return false;
                }
            }

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

        public void AddStudent()
        {
            if (Students.Any(s => s.MSSV.Equals(SelectedStudent.MSSV, StringComparison.OrdinalIgnoreCase)))
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
                TinhTrang = SelectedStudent.TinhTrang,
                // Set creation time when adding a new student.
                CreationTime = DateTime.Now
            };

            Students.Add(newStudent);
            SaveStudentsAsync();
            SimpleLogger.LogInfo($"Added new student with MSSV: {newStudent.MSSV}");
        }

        public void DeleteStudent()
        {
            if (SelectedStudent != null)
            {
                // Enforce deletion rule: Only allow deletion if CreationTime is within the allowed window.
                if (!_deletionService.CanDeleteStudent(SelectedStudent))
                {
                    Debug.WriteLine("Deletion not allowed: The student was created more than 30 minutes ago.");
                    SimpleLogger.LogWarning("Deletion not allowed: The student was created more than 30 minutes ago.");
                    return;
                }

                Students.Remove(SelectedStudent);
                SaveStudentsAsync();
                SimpleLogger.LogInfo("Deleted a student.");
            }
        }

        public void UpdateStudent()
        {
            SaveStudentsAsync();
            SimpleLogger.LogInfo("Updated student information.");
        }

        public void SearchStudent()
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

        public async Task ExportToJsonAsync()
        {
            try
            {
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

        public async Task ExportToCsvAsync()
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

        // NEW: Export confirmation letter to HTML.
        public async Task ExportConfirmationToHtmlAsync()
        {
            if (SelectedStudent == null)
                return;
            try
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = Path.Combine(folderPath, "confirmation.html");
                var exportService = new ExportConfirmationService();
                await exportService.ExportConfirmationToHtmlAsync(SelectedStudent, filePath);
                Debug.WriteLine($"Exported confirmation to HTML successfully to {filePath}.");
                SimpleLogger.LogInfo($"Exported confirmation to HTML successfully to {filePath}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting confirmation to HTML: {ex.Message}");
                SimpleLogger.LogError($"Error exporting confirmation to HTML: {ex.Message}");
            }
        }

        // NEW: Export confirmation letter to Markdown.
        public async Task ExportConfirmationToMarkdownAsync()
        {
            if (SelectedStudent == null)
                return;
            try
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = Path.Combine(folderPath, "confirmation.md");
                var exportService = new ExportConfirmationService();
                await exportService.ExportConfirmationToMarkdownAsync(SelectedStudent, filePath);
                Debug.WriteLine($"Exported confirmation to Markdown successfully to {filePath}.");
                SimpleLogger.LogInfo($"Exported confirmation to Markdown successfully to {filePath}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error exporting confirmation to Markdown: {ex.Message}");
                SimpleLogger.LogError($"Error exporting confirmation to Markdown: {ex.Message}");
            }
        }

        public void RaiseCommandCanExecuteChanged()
        {
            (AddStudentCommand as CustomRelayCommand)?.RaiseCanExecuteChanged();
            (UpdateStudentCommand as CustomRelayCommand)?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

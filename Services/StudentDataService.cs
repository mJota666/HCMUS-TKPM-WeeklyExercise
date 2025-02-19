using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using StudentManagementApp.Models;

namespace StudentManagementApp.Services
{
    public class StudentDataService
    {
        private readonly string _filePath;

        public StudentDataService(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<List<Student>> LoadStudentsAsync()
        {
            if (!File.Exists(_filePath))
                return new List<Student>();

            using FileStream stream = File.OpenRead(_filePath);
            var students = await JsonSerializer.DeserializeAsync<List<Student>>(stream);
            Debug.WriteLine("Debug student");
            Debug.WriteLine(students);
            return students ?? new List<Student>();
        }

        public async Task SaveStudentsAsync(List<Student> students)
        {
            try
            {
                using FileStream stream = File.Create(_filePath);
                await JsonSerializer.SerializeAsync(stream, students, new JsonSerializerOptions { WriteIndented = true });
                Debug.WriteLine($"Successfully saved {students.Count} students to {_filePath}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving students to {_filePath}: {ex.Message}");
            }
        }

    }
}

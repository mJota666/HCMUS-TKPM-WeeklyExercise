using System;
using StudentManagementApp.Models;
using System.Diagnostics;

namespace StudentManagementApp.Services
{
    public class DeletionService
    {
        private readonly TimeSpan _deletionWindow;

        public DeletionService(TimeSpan deletionWindow)
        {
            _deletionWindow = deletionWindow;
        }

        public bool CanDeleteStudent(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));
            Debug.WriteLine("Xoa ne");
            Debug.WriteLine(DateTime.Now - student.CreationTime);
            Debug.WriteLine(_deletionWindow);

            return (DateTime.Now - student.CreationTime) <= _deletionWindow;
        }
    }
}

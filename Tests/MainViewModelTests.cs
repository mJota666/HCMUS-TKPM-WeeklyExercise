//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using StudentManagementApp.ViewModels;
//using StudentManagementApp.Models;
//using System.Linq;

//namespace StudentManagementApp.Tests
//{
//    [TestClass]
//    public class MainViewModelTests
//    {
//        // Create a new instance of the MainViewModel for each test.
//        private MainViewModel CreateViewModel()
//        {
//            return new MainViewModel();
//        }

//        [TestMethod]
//        public void CanAddOrUpdateStudent_ReturnsFalse_WhenRequiredFieldsAreMissing()
//        {
//            // Arrange
//            var vm = CreateViewModel();
//            // Create a student with missing required fields.
//            vm.SelectedStudent = new Student
//            {
//                MSSV = "", // missing MSSV
//                HoTen = "", // missing name
//                NgaySinh = DateTime.MinValue, // invalid date
//                Email = "invalid-email",
//                SoDienThoai = "12345", // too short
//                Khoa = "Nonexistent", // not in Faculties list
//                ChuongTrinh = "Nonexistent", // not in Programs list
//                TinhTrang = "Nonexistent" // not in StudentStatus list
//            };

//            // Act
//            bool result = vm.CanAddOrUpdateStudent();

//            // Assert
//            Assert.IsFalse(result, "Validation should fail if required fields are missing or invalid.");
//        }

//        [TestMethod]
//        public void CanAddOrUpdateStudent_ReturnsTrue_WhenAllValid()
//        {
//            // Arrange
//            var vm = CreateViewModel();
//            // Create a valid student.
//            vm.SelectedStudent = new Student
//            {
//                MSSV = "SV001",
//                HoTen = "Nguyen Van A",
//                NgaySinh = new DateTime(2000, 1, 1),
//                Email = "test@example.com",
//                SoDienThoai = "0123456789",
//                Khoa = vm.Faculties.First(),          // e.g. "Công nghệ Thông tin"
//                KhoaHoc = "K2024",
//                ChuongTrinh = vm.Programs.First(),      // e.g. "Chất lượng cao"
//                TinhTrang = vm.StudentStatus.First()     // e.g. "Đang học"
//            };

//            // Act
//            bool result = vm.CanAddOrUpdateStudent();

//            // Assert
//            Assert.IsTrue(result, "Validation should pass for a student with valid fields.");
//        }

//        [TestMethod]
//        public void AddStudent_AddsStudent_WhenNotDuplicate()
//        {
//            // Arrange
//            var vm = CreateViewModel();
//            vm.SelectedStudent = new Student
//            {
//                MSSV = "SV002",
//                HoTen = "Le Thi B",
//                NgaySinh = new DateTime(2001, 2, 2),
//                Email = "b@example.com",
//                SoDienThoai = "0987654321",
//                Khoa = vm.Faculties.First(),          // use an existing value
//                KhoaHoc = "K2024",
//                ChuongTrinh = vm.Programs.First(),
//                TinhTrang = vm.StudentStatus.First()
//            };
//            int initialCount = vm.Students.Count;

//            // Act
//            vm.AddStudent();

//            // Assert
//            Assert.AreEqual(initialCount + 1, vm.Students.Count, "Student should be added when valid and not duplicate.");
//        }

//        [TestMethod]
//        public void AddStudent_DoesNotAddDuplicate()
//        {
//            // Arrange
//            var vm = CreateViewModel();
//            var student = new Student
//            {
//                MSSV = "SV003",
//                HoTen = "Tran Van C",
//                NgaySinh = new DateTime(2002, 3, 3),
//                Email = "c@example.com",
//                SoDienThoai = "01234567890",
//                Khoa = vm.Faculties.First(),
//                KhoaHoc = "K2024",
//                ChuongTrinh = vm.Programs.First(),
//                TinhTrang = vm.StudentStatus.First()
//            };

//            vm.SelectedStudent = student;
//            vm.AddStudent();
//            int countAfterFirst = vm.Students.Count;

//            // Act: try to add the same student again.
//            vm.SelectedStudent = student;
//            vm.AddStudent();

//            // Assert
//            Assert.AreEqual(countAfterFirst, vm.Students.Count, "Duplicate student should not be added.");
//        }
//    }
//}

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using StudentManagementApp.Models;

namespace StudentManagementApp.Services
{
    public class ExportConfirmationService
    {
        // These values could be loaded from a config file instead of being hardcoded.
        private const string UniversityName = "TRƯỜNG ĐẠI HỌC Khoa Học Tự Nhiên, ĐHQG TP.HCM";
        private const string DepartmentName = "PHÒNG ĐÀO TẠO";
        private const string UniversityAddress = "227 Nguyễn Văn Cừ, Quận 5, TP.HCM";
        private const string UniversityPhone = "094 580 068";
        private const string UniversityEmail = "contact@university.edu.vn";

        /// <summary>
        /// Exports the student's confirmation letter as an HTML file.
        /// </summary>
        /// <param name="student">The student whose confirmation letter will be generated.</param>
        /// <param name="outputPath">The file path to save the HTML file.</param>
        public async Task ExportConfirmationToHtmlAsync(Student student, string outputPath)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            string htmlContent = $@"
<html>
  <head>
    <meta charset=""UTF-8"">
    <title>Giấy xác nhận tình trạng sinh viên</title>
    <style>
      body {{ font-family: Arial, sans-serif; margin: 20px; }}
      .header {{ text-align: center; margin-bottom: 20px; }}
      .header img {{ height: 100px; }}
      .header p {{ margin: 2px 0; font-size: 16px; }}
      h2 {{ color: navy; margin-top: 20px; }}
      ul {{ font-size: 16px; }}
      .section {{ margin-top: 20px; }}
    </style>
  </head>
  <body>
    <div class=""header"">
      <img src=""https://hcmus.edu.vn/wp-content/uploads/2021/12/logo-khtn_remake-1.png"" alt=""Logo"" />
      <p><strong>{UniversityName}</strong></p>
      <p><strong>{DepartmentName}</strong></p>
      <p>📍 Địa chỉ: {UniversityAddress}</p>
      <p>📞 Điện thoại: {UniversityPhone} | 📧 Email: {UniversityEmail}</p>
    </div>
    <hr/>
    <h2>GIẤY XÁC NHẬN TÌNH TRẠNG SINH VIÊN</h2>
    <p>{UniversityName} xác nhận:</p>
    <div class=""section"">
      <h3>1. Thông tin sinh viên:</h3>
      <ul>
        <li><strong>Họ và tên:</strong> {student.HoTen}</li>
        <li><strong>Mã số sinh viên:</strong> {student.MSSV}</li>
        <li><strong>Ngày sinh:</strong> {student.NgaySinh:dd/MM/yyyy}</li>
        <li><strong>Giới tính:</strong> {student.GioiTinh}</li>
        <li><strong>Khoa:</strong> {student.Khoa}</li>
        <li><strong>Chương trình đào tạo:</strong> {student.ChuongTrinh}</li>
        <li><strong>Khóa:</strong> {student.KhoaHoc}</li>
      </ul>
    </div>
    <div class=""section"">
      <h3>2. Tình trạng sinh viên hiện tại:</h3>
      <ul>
        <li>Đang theo học</li>
        <li>Đã hoàn thành chương trình, chờ xét tốt nghiệp</li>
        <li>Đã tốt nghiệp</li>
        <li>Bảo lưu</li>
        <li>Đình chỉ học tập</li>
        <li>Tình trạng khác</li>
      </ul>
      <p><strong>Tình trạng của sinh viên:</strong> {student.TinhTrang}</p>
    </div>
    <div class=""section"">
      <h3>3. Mục đích xác nhận:</h3>
      <ul>
        <li>Xác nhận đang học để vay vốn ngân hàng</li>
        <li>Xác nhận làm thủ tục tạm hoãn nghĩa vụ quân sự</li>
        <li>Xác nhận làm hồ sơ xin việc / thực tập</li>
        <li>Xác nhận lý do khác: [Ghi rõ]</li>
      </ul>
    </div>
    <div class=""section"">
      <h3>4. Thời gian cấp giấy:</h3>
      <p>Giấy xác nhận có hiệu lực đến ngày: [DD/MM/YYYY] (tùy vào mục đích xác nhận)</p>
    </div>
    <p><strong>Xác nhận của {UniversityName}</strong></p>
    <p>📅 Ngày cấp: {DateTime.Now:dd/MM/yyyy}</p>
    <p>🖋 <strong>Trưởng Phòng Đào Tạo</strong> (Ký, ghi rõ họ tên, đóng dấu)</p>
  </body>
</html>";

            await File.WriteAllTextAsync(outputPath, htmlContent, Encoding.UTF8);
        }

        /// <summary>
        /// Exports the student's confirmation letter as a Markdown file.
        /// </summary>
        /// <param name="student">The student whose confirmation letter will be generated.</param>
        /// <param name="outputPath">The file path to save the Markdown file.</param>
        public async Task ExportConfirmationToMarkdownAsync(Student student, string outputPath)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            string mdContent = $@"
**{UniversityName}**  
**{DepartmentName}**  
📍 Địa chỉ: {UniversityAddress}  
📞 Điện thoại: {UniversityPhone} | 📧 Email: {UniversityEmail}

---

### **GIẤY XÁC NHẬN TÌNH TRẠNG SINH VIÊN**

{UniversityName} xác nhận:

**1. Thông tin sinh viên:**

- **Họ và tên:** {student.HoTen}
- **Mã số sinh viên:** {student.MSSV}
- **Ngày sinh:** {student.NgaySinh:dd/MM/yyyy}
- **Giới tính:** {student.GioiTinh}
- **Khoa:** {student.Khoa}
- **Chương trình đào tạo:** {student.ChuongTrinh}
- **Khóa:** {student.KhoaHoc}

**2. Tình trạng sinh viên hiện tại:**

- Đang theo học  
- Đã hoàn thành chương trình, chờ xét tốt nghiệp  
- Đã tốt nghiệp  
- Bảo lưu  
- Đình chỉ học tập  
- Tình trạng khác  

**Tình trạng của sinh viên:** {student.TinhTrang}

**3. Mục đích xác nhận:**

- Xác nhận đang học để vay vốn ngân hàng  
- Xác nhận làm thủ tục tạm hoãn nghĩa vụ quân sự  
- Xác nhận làm hồ sơ xin việc / thực tập  
- Xác nhận lý do khác: [Ghi rõ]

**4. Thời gian cấp giấy:**

- Giấy xác nhận có hiệu lực đến ngày: [DD/MM/YYYY] (tùy vào mục đích xác nhận)

📍 **Xác nhận của {UniversityName}**  
📅 Ngày cấp: {DateTime.Now:dd/MM/yyyy}  
🖋 **Trưởng Phòng Đào Tạo**  
(Ký, ghi rõ họ tên, đóng dấu)
";

            await File.WriteAllTextAsync(outputPath, mdContent, Encoding.UTF8);
        }
    }
}

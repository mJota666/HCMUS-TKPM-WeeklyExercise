using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using StudentManagementApp.Models;

namespace StudentManagementApp.Services
{
    public class ExportConfirmationService
    {
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
        <title>Xác nhận tình trạng sinh viên</title>
        <style>
            body {{ font-family: Arial, sans-serif; margin: 20px; }}
            h1 {{ color: navy; }}
            p {{ font-size: 16px; }}
        </style>
    </head>
    <body>
        <h1>Giấy xác nhận tình trạng sinh viên</h1>
        <p><strong>MSSV:</strong> {student.MSSV}</p>
        <p><strong>Họ tên:</strong> {student.HoTen}</p>
        <p><strong>Tình trạng:</strong> {student.TinhTrang}</p>
        <p><strong>Ngày xác nhận:</strong> {DateTime.Now:yyyy-MM-dd}</p>
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
# Giấy xác nhận tình trạng sinh viên

**MSSV:** {student.MSSV}  
**Họ tên:** {student.HoTen}  
**Tình trạng:** {student.TinhTrang}  
**Ngày xác nhận:** {DateTime.Now:yyyy-MM-dd}  
";
            await File.WriteAllTextAsync(outputPath, mdContent, Encoding.UTF8);
        }
    }
}

✅ **Lưu trữ dữ liệu:**
- Bộ dữ liệu gồm 3 sinh viên, được lưu trữ trong Data/students.json:
	- 2 sinh viên khoa Toán
	- 1 sinh viên khoa Điện tử Viễn Thông
- `Students.json`
![Lưu trữ dữ liệu](Screenshots/StoreData.png)
- Dữ liệu được lưu trữ hiển thị lên màn hình
![Hiển thị dữ liệu](Screenshots/App.png)

✅ **Cho phép đổi tên & thêm mới:**
- **Khoa (Faculty)**
	- Trước khi thêm khoa Môi trường
	![Trước khi thêm khoa](Screenshots/BeforeAddFaculty.png)
	- Sau khi thêm khoa Môi trường, list đã được cập nhật
	![Sau khi thêm khoa](Screenshots/AfterAddFaculty.png)
	- Sau khi đổi tên khoa Công nghệ thông tin thành CNTT
	![Đổi tên khoa](Screenshots/RenameFaculty.png)
- **Tình trạng sinh viên (Student Status)**
	- Trước khi thêm `StudentStatus`
	![Trước khi thêm tình trạng](Screenshots/BeforeAddNewStatus.png)
	- Sau khi thêm `StudentStatus` Du học
	![Sau khi thêm tình trạng du học](Screenshots/AfterAddNewStatus.png)
	- Sửa `StudentStatus` đang học thành `Active` 
	![Đổi tên tình trạng](Screenshots/RenameStatus.png)
- **Chương trình đào tạo (Program)**
	-  Trước khi thêm chương trình đào tạo mới
	![Trước khi thêm chương trình](Screenshots/BeforeAddNewProgram.png)
	- Sau khi thêm chương trình đào tạo từ xa
	![Sau khi thêm chương trình từ xa](Screenshots/AfterAddProgram.png)
	- Đổi tên chương trình Đào tạo từ xa thành Đào tạo Online
	![Đổi tên chương trình](Screenshots/RenameProgram.png)

✅ **Thêm chức năng tìm kiếm:**
- Bộ dữ liệu gồm 3 sinh viên, được lưu trữ trong Data/students.json:
	- 2 sinh viên khoa Toán
	- 1 sinh viên khoa Điện tử Viễn Thông
- Tìm kiếm theo khoa Toán
	![Tìm kiếm theo khoa Toán](Screenshots/SearchFaculty.png)
- Tìm kiếm theo khoa Toán + Nguyễn Văn A, lưu ý phải có dấu + trong Querry
	![Tìm kiếm theo khoa và tên](Screenshots/SearchByFacultyAndName.png)

✅ **Hỗ trợ import/export dữ liệu:**
- Hỗ trợ Export Json và CSV, các nút Export ở góc trên bên phải màn hình
	![Xuất dữ liệu](Screenshots/Export.png)
	- File được Export trong local, nơi lưu file được in ra trong file Log và trong màn hình output
	![Xuất dữ liệu thành công](Screenshots/FeaExport.png)

✅ **Thêm logging mechanism:**
- File log.txt được lưu trong thư mục local cùng với các file Export
	![Logging mechanism](Screenshots/FeaLogging.png)

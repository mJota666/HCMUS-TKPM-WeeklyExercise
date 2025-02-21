✅ **Lưu trữ dữ liệu:**
- Bộ dữ liệu gồm 3 sinh viên, được lưu trữ trong Data/students.json:
	- 2 sinh viên khoa Toán
	- 1 sinh viên khoa Điện tử Viễn Thông
- `Students.json`
![[StoreData.png]]
- Dữ liệu được lưu trữ hiển thị lên màn hình
![[App.png]]

✅ **Cho phép đổi tên & thêm mới:**
- **Khoa (Faculty)**
	- Trước khi thêm khoa Môi trường
	![[BeforeAddFaculty.png]]
	- Sau khi thêm khoa Môi trường, list đã được cập nhật
	![[AfterAddFaculty.png]]
	- Sau khi đổi tên khoa Công nghệ thông tin thành CNTT
	![[RenameFaculty.png]]
- **Tình trạng sinh viên (Student Status)**
	- Trước khi thêm `StudentStatus`
	![[BeforeAddNewStatus.png]]
	- Sau khi thêm `StudentStatus` Du học
	![[AfterAddNewStatus.png]]
	- Sửa `StudentStatus` đang học thành `Active` 
	![[RenameStatus.png]]
- **Chương trình đào tạo (Program)**
	-  Trước khi thêm chương trình đào tạo mới
	![[BeforeAddNewProgram.png]]
	- Sau khi thêm chương trình đào tạo từ xa
	![[AfterAddProgram.png]]
	- Đổi tên chương trình Đào tạo từ xa thành Đào tạo Online
	![[RenameProgram.png]]
✅ **Thêm chức năng tìm kiếm:**
- Bộ dữ liệu gồm 3 sinh viên, được lưu trữ trong Data/students.json:
	- 2 sinh viên khoa Toán
	- 1 sinh viên khoa Điện tử Viễn Thông
- Tìm kiếm theo khoa Toán
	![[SearchFaculty.png]]
- Tìm kiếm theo khoa Toán + Nguyễn Văn A, lưu ý phải có dấu + trong Querry
	![[SearchByFacultyAndName.png]]

✅ **Hỗ trợ import/export dữ liệu:**
- Hỗ trợ Export Json và CSV, các nút Export ở góc trên bên phải màn hình
	![[Export.png]]
	- File được Export trong local, nơi lưu file được in ra trong file Log và trong màn hình output
	![[FeaExport.png]]

✅ **Thêm logging mechanism:**
- File log.txt được lưu trong thư mục local cùng với các file Export
![[FeaLogging.png]]
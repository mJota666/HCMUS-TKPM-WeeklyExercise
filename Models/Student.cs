using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace StudentManagementApp.Models
{
    public class Student : INotifyPropertyChanged
    {
        private string _mssv = string.Empty;
        [JsonPropertyName("mssv")]
        public string MSSV
        {
            get => _mssv;
            set
            {
                if (_mssv != value)
                {
                    _mssv = value;
                    OnPropertyChanged(nameof(MSSV));
                }
            }
        }

        private string _hoTen = string.Empty;
        [JsonPropertyName("hoTen")]
        public string HoTen
        {
            get => _hoTen;
            set
            {
                if (_hoTen != value)
                {
                    _hoTen = value;
                    OnPropertyChanged(nameof(HoTen));
                }
            }
        }

        private DateTime _ngaySinh = DateTime.MinValue;
        [JsonPropertyName("ngaySinh")]
        public DateTime NgaySinh
        {
            get => _ngaySinh;
            set
            {
                if (_ngaySinh != value)
                {
                    _ngaySinh = value;
                    OnPropertyChanged(nameof(NgaySinh));
                }
            }
        }

        private string _gioiTinh = string.Empty;
        [JsonPropertyName("gioiTinh")]
        public string GioiTinh
        {
            get => _gioiTinh;
            set
            {
                if (_gioiTinh != value)
                {
                    _gioiTinh = value;
                    OnPropertyChanged(nameof(GioiTinh));
                }
            }
        }

        private string _khoa = string.Empty;
        [JsonPropertyName("khoa")]
        public string Khoa
        {
            get => _khoa;
            set
            {
                if (_khoa != value)
                {
                    _khoa = value;
                    OnPropertyChanged(nameof(Khoa));
                }
            }
        }

        private string _khoaHoc = string.Empty;
        [JsonPropertyName("khoaHoc")]
        public string KhoaHoc
        {
            get => _khoaHoc;
            set
            {
                if (_khoaHoc != value)
                {
                    _khoaHoc = value;
                    OnPropertyChanged(nameof(KhoaHoc));
                }
            }
        }

        private string _chuongTrinh = string.Empty;
        [JsonPropertyName("chuongTrinh")]
        public string ChuongTrinh
        {
            get => _chuongTrinh;
            set
            {
                if (_chuongTrinh != value)
                {
                    _chuongTrinh = value;
                    OnPropertyChanged(nameof(ChuongTrinh));
                }
            }
        }

        private string _diaChi = string.Empty;
        [JsonPropertyName("diaChi")]
        public string DiaChi
        {
            get => _diaChi;
            set
            {
                if (_diaChi != value)
                {
                    _diaChi = value;
                    OnPropertyChanged(nameof(DiaChi));
                }
            }
        }

        private string _email = string.Empty;
        [JsonPropertyName("email")]
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        private string _soDienThoai = string.Empty;
        [JsonPropertyName("soDienThoai")]
        public string SoDienThoai
        {
            get => _soDienThoai;
            set
            {
                if (_soDienThoai != value)
                {
                    _soDienThoai = value;
                    OnPropertyChanged(nameof(SoDienThoai));
                }
            }
        }

        private string _tinhTrang = string.Empty;
        [JsonPropertyName("tinhTrang")]
        public string TinhTrang
        {
            get => _tinhTrang;
            set
            {
                if (_tinhTrang != value)
                {
                    _tinhTrang = value;
                    OnPropertyChanged(nameof(TinhTrang));
                }
            }
        }

        // New: Property to track when the student was created.
        private DateTime _creationTime = DateTime.Now;
        [JsonPropertyName("creationTime")]
        public DateTime CreationTime
        {
            get => _creationTime;
            set
            {
                if (_creationTime != value)
                {
                    _creationTime = value;
                    OnPropertyChanged(nameof(CreationTime));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

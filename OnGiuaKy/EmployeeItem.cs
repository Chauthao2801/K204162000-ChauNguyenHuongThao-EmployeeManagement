using System;

namespace OnGiuaKy
{
	public class EmployeeItem
	{
		public string MaNV { get; set; }
		public string HoTen { get; set; }
		public string GioiTinh { get; set; }
		public string Phone { get; set; }
		public DateTime HireDate { get; set; }
		public string TypeId { get; set; }
		public double DoanhSo { get; set; }
		public double PhuCapNhienLieu { get; set; }
		public double Salary {get; set; }
		public bool ThamNien
		{
			get
			{
				return (DateTime.Now - HireDate).TotalDays / 365 > 5;
			}
		}
	}
}
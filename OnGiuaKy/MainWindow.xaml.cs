using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration; // For ConfigurationManager
using System.Data.SqlClient; // For SqlConnection, SqlCommand
using System.Collections.ObjectModel;
using System.Data;
using System.Runtime.Remoting.Contexts;
using System.Collections;
using System.Data.Common;

namespace OnGiuaKy
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private bool IsValidData()
		{
			string maNV = txtMaNV.Text.Trim();
			string hoTen = txtHoTen.Text.Trim();
			string doanhSoText = txtDoanhSo.Text.Trim();
			string tienPhuCapText = txtPCNhienLieu.Text.Trim();

			// Check if MaNV and HoTen are not empty
			if (string.IsNullOrEmpty(maNV) || string.IsNullOrEmpty(hoTen))
			{
				MessageBox.Show("Mã NV và Họ Tên không được để trống.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			// Check if DoanhSo is valid for "Bán hàng"
			if (radBanHang.IsChecked == true && string.IsNullOrEmpty(doanhSoText))
			{
				MessageBox.Show("Vui lòng nhập Doanh Số.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			// Check if TienPhuCap is valid for "Giao nhận"
			if (radGiaoNhan.IsChecked == true && string.IsNullOrEmpty(tienPhuCapText))
			{
				MessageBox.Show("Vui lòng nhập Tiền Phụ Cấp Nhiên Liệu.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			return true;
		}

		public MainWindow()
		{
			InitializeComponent();
			WindowStartupLocation = WindowStartupLocation.CenterScreen;
			Closing += MainWindow_Closing;

			// Set default gender to "Nam"
			radNam.IsChecked = true;

			// Set default hire date to current date
			dtpNgayVaoLam.SelectedDate = DateTime.Now;

			// Set default employee type to "Bán hàng"
			radBanHang.IsChecked = true;

			//LoadDataToListView
			LoadDataToListView();

			lvDS.SelectionChanged += lvDS_SelectionChanged;



		}

		private void LoadDataToListView()
		{
			string strConn = "server = THAO-SURFACE; database=QuanLyNhanVien;uid=sa;pwd = 123456";
			SqlConnection con = new SqlConnection(strConn);
			{
				try
				{
					if (con.State == ConnectionState.Closed)
						con.Open();
					string sql = "SELECT * FROM Employee where IsDeleted = 0 order by HireDate asc";

					using (SqlCommand command = new SqlCommand(sql, con))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								// Create an instance of your Employee class
								EmployeeItem employee = new EmployeeItem();
								employee.MaNV = reader.GetString(1);
								employee.HoTen = reader.GetString(2);
								employee.GioiTinh = reader.GetString(3);
								employee.Phone = reader.GetString(4);
								employee.HireDate = reader.GetDateTime(5);
								employee.TypeId = reader.GetString(6);
								employee.DoanhSo = reader.GetDouble(7);
								employee.PhuCapNhienLieu = reader.GetDouble(8);

																
								lvDS.Items.Add(employee);


							}
						}
					}
				}
				catch (Exception ex) 
				{
					MessageBoxResult result = MessageBox.Show("Lỗi khi tải dữ liệu vào cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OKCancel, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
					if (result == MessageBoxResult.Cancel)
					{
						Clipboard.SetText(ex.Message);
					}
				}
			} 
		}

		private ObservableCollection<EmployeeItem> employeeList = new ObservableCollection<EmployeeItem>();

		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show("Are you sure you want to close the program?", "Confirm Close", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.No)
			{
				e.Cancel = true; // Cancel closing
			}
		}

		private void radBanHang_Checked(object sender, RoutedEventArgs e)
		{
			lblDoanhSo.Visibility = Visibility.Visible;
			lblPC.Visibility = Visibility.Collapsed;
			txtDoanhSo.Visibility = Visibility.Visible;
			txtPCNhienLieu.Visibility = Visibility.Collapsed;
		}

		private void radGiaoNhan_Checked(object sender, RoutedEventArgs e)
		{
			lblDoanhSo.Visibility = Visibility.Collapsed;
			lblPC.Visibility = Visibility.Visible;
			txtDoanhSo.Visibility = Visibility.Collapsed;
			txtPCNhienLieu.Visibility = Visibility.Visible;
		}

		private void btnThem_Click(object sender, RoutedEventArgs e)
		{
			// Clear input fields
			txtMaNV.Clear();
			txtHoTen.Clear();
			txtDienThoai.Clear();
			txtDoanhSo.Clear();
			txtPCNhienLieu.Clear();
			dtpNgayVaoLam.SelectedDate = DateTime.Now;

			// Set default gender to "Nam"
			radNam.IsChecked = true;

			// Set default employee type to "Bán hàng"
			radBanHang.IsChecked = true;

			// Set focus to the MaNV TextBox
			txtMaNV.Focus();
		}

		private void btnLuu_Click(object sender, RoutedEventArgs e)
		{
			// Validate input data
			if (IsValidData())
			{
				// Save valid data to the database
				SaveDataToDatabase();

				// Create a new Employee object
				EmployeeItem employee = new EmployeeItem();
				employee.MaNV = txtMaNV.Text;
				employee.HoTen = txtHoTen.Text;
				employee.GioiTinh = radNam.IsChecked == true ? "Nam" : "Nữ";
				employee.HireDate = dtpNgayVaoLam.SelectedDate.Value;
				employee.Phone = txtDienThoai.Text;
				employee.TypeId = radBanHang.IsChecked == true ? "Bán Hàng" : "Giao nhận";
				if (employee.TypeId == "Bán Hàng")
				{
					employee.DoanhSo = double.Parse(txtDoanhSo.Text);
				}
				else if (employee.TypeId == "Giao nhận")
				{
					employee.PhuCapNhienLieu = double.Parse(txtPCNhienLieu.Text);
				}

				// Add the employee to the ListView
				lvDS.Items.Add(employee);

				// Select the newly added row in the ListView
				lvDS.SelectedItem = employee;
			}
		}

		private void SaveDataToDatabase()
		{
			string strConn = "server = THAO-SURFACE; database=QuanLyNhanVien;uid=sa;pwd = 123456";
			SqlConnection con = new SqlConnection(strConn);
			{
				try
				{
					if (con.State == ConnectionState.Closed)
						con.Open();

					string insertQuery = "INSERT INTO Employee (EmpID, Name, Gender, Phone, HireDate, TypeName, DoanhSo, PhuCap) " +
						"VALUES (@MaNV, @HoTen, @GioiTinh, @Phone, @HireDate, @Type, @DoanhSo,@PhuCapNhienLieu)";

					using (SqlCommand command = new SqlCommand(insertQuery, con))
					{
						command.Parameters.AddWithValue("@MaNV", txtMaNV.Text);
						command.Parameters.AddWithValue("@HoTen", txtHoTen.Text);
						command.Parameters.AddWithValue("@GioiTinh", radNam.IsChecked == true ? "Nam" : "Nữ");
						command.Parameters.AddWithValue("@Phone", txtDienThoai.Text);
						command.Parameters.AddWithValue("@HireDate", dtpNgayVaoLam.SelectedDate);
						command.Parameters.AddWithValue("@Type", radBanHang.IsChecked == true ? "Bán Hàng" : "Giao nhận");
						command.Parameters.AddWithValue("@DoanhSo", txtDoanhSo.Text);
						command.Parameters.AddWithValue("@PhuCapNhienLieu", txtPCNhienLieu.Text);

						command.ExecuteNonQuery();
					}

					MessageBox.Show("Dữ liệu đã được lưu vào cơ sở dữ liệu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Lỗi khi lưu dữ liệu vào cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}


		private void lvDS_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Get the selected item
			if (lvDS.SelectedItem != null)
			{
				
				EmployeeItem selectedEmployee = lvDS.SelectedItem as EmployeeItem;

				txtMaNV.Text = selectedEmployee.MaNV.ToString();
				txtHoTen.Text = selectedEmployee.HoTen;

				dtpNgayVaoLam.SelectedDate = selectedEmployee.HireDate;
				txtDienThoai.Text = selectedEmployee.Phone;

				if (selectedEmployee.GioiTinh == "Nam")
					radNam.IsChecked = true;
				else
					radNu.IsChecked = true;
				if (selectedEmployee.TypeId == "Bán Hàng")
				{
					radBanHang.IsChecked = true;
					txtDoanhSo.Text = selectedEmployee.DoanhSo.ToString();
				}
				else
				{
					radGiaoNhan.IsChecked = true;
					txtPCNhienLieu.Text = selectedEmployee.PhuCapNhienLieu.ToString();
				}


			}
			else
			{
				// Clear input controls if no item is selected
				ClearInputControls();
			}
		}

		private void ClearInputControls()
		{
			txtMaNV.Clear();
			txtHoTen.Clear();
			txtDienThoai.Clear();
			txtDoanhSo.Clear();
			txtPCNhienLieu.Clear();
			radNam.IsChecked = false;
			radNu.IsChecked = false;
			radBanHang.IsChecked = false;
			radGiaoNhan.IsChecked = false;

		}
		//Cau 9
		private void btnSapXep_Click(object sender, RoutedEventArgs e)
		{
			// Sort the items in the ListView
			lvDS.Items.SortDescriptions.Clear();
			lvDS.Items.SortDescriptions.Add(new SortDescription("ThamNien", ListSortDirection.Descending));
			lvDS.Items.SortDescriptions.Add(new SortDescription("HoTen", ListSortDirection.Ascending));
		}

		private void btnThongke_Click(object sender, RoutedEventArgs e)
		{
			int soNhanVienBanHang = 0;
			int soNhanVienGiaoNhan = 0;
			double tongLuongBanHang = 0;
			double tongLuongGiaoNhan = 0;
			double luongCoBan = 7000000;

			foreach (EmployeeItem employee in lvDS.Items)
			{
				if (employee.TypeId == "Bán Hàng")
				{
					soNhanVienBanHang++;
					tongLuongBanHang += luongCoBan + employee.DoanhSo * 0.1;
				}
				else if (employee.TypeId == "Giao nhận")
				{
					soNhanVienGiaoNhan++;
					tongLuongGiaoNhan += luongCoBan + employee.PhuCapNhienLieu;
				}
			}

			MessageBox.Show("Công ty hiện có " + soNhanVienBanHang + " nhân viên bán hàng, " 
				+ soNhanVienGiaoNhan + " nhân viên giao nhận.\nTổng lương chi cho nhân viên bán hàng: " 
				+ tongLuongBanHang.ToString("C0") 
				+ "\nTổng lương chi cho nhân viên giao nhận: " 
				+ tongLuongGiaoNhan.ToString("C0"));
		}

		private void btnXoa_Click(object sender, RoutedEventArgs e)
		{
			if (lvDS.SelectedItem != null)
			{
				EmployeeItem selectedEmployee = (EmployeeItem)lvDS.SelectedItem;
				MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa nhân viên " + selectedEmployee.HoTen + " không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (result == MessageBoxResult.Yes)
				{
					string strConn = "server = THAO-SURFACE; database=QuanLyNhanVien;uid=sa;pwd = 123456";
					SqlConnection con = new SqlConnection(strConn);
					if (con.State == ConnectionState.Closed)
								con.Open();
					using (SqlCommand cmd = new SqlCommand("UPDATE Employee SET IsDeleted = 1 WHERE EmpID = @MaNV", con))
						{
							cmd.Parameters.AddWithValue("@MaNV", selectedEmployee.MaNV);
							cmd.ExecuteNonQuery();
						}
					}

					// Remove the employee from the list of employees and refresh the ListView
					employeeList.Remove(selectedEmployee);
					lvDS.Items.Refresh();

					// Select the next item in the ListView, if any
					if (lvDS.Items.Count > 0)
					{
						lvDS.SelectedIndex = 0;
					}
					else
					{
						// Reset the form to its default state
						txtHoTen.Text = "";
						txtMaNV.Text = "";
						txtDienThoai.Text = "";
						txtDoanhSo.Text = "";
						txtPCNhienLieu.Text = "";
						radNam.IsChecked = true;
						radNu.IsChecked = false;
						radBanHang.IsChecked = true;
					radGiaoNhan.IsChecked = false;
					dtpNgayVaoLam.SelectedDate = DateTime.Today;
					}
				}
			}

		private void btnSua_Click(object sender, RoutedEventArgs e)
		{
			if (lvDS.SelectedItem != null)
			{
				EmployeeItem nv = (EmployeeItem)lvDS.SelectedItem;

				// Update the employee's information
				nv.HoTen = txtHoTen.Text;
				nv.MaNV = txtMaNV.Text;
				nv.Phone = txtDienThoai.Text;
				
				nv.GioiTinh = (bool)radNam.IsChecked ? "Nam" : "Nữ";
				nv.TypeId = (bool)radBanHang.IsChecked ? "Bán Hàng" : "Giao nhận";


				nv.HireDate = dtpNgayVaoLam.SelectedDate.Value;

				if (nv.TypeId == "Bán Hàng")
				{
					nv.DoanhSo = double.Parse(txtDoanhSo.Text);
				}
				else if (nv.TypeId == "Giao nhận")
				{
					nv.PhuCapNhienLieu = double.Parse(txtPCNhienLieu.Text);
				}

				// Update the changes to the database
				string strConn = "server = THAO-SURFACE; database=QuanLyNhanVien;uid=sa;pwd = 123456";
				SqlConnection con = new SqlConnection(strConn);
				using (SqlCommand command = new SqlCommand("UPDATE Employee SET EmpID = @MaNV, Name = @HoTen, Gender=@GioiTinh, " +
					"Phone = @Phone, HireDate = @HireDate, TypeName = @Type, " +
					"DoanhSo = @DoanhSo, PhuCap = @PhuCapNhienLieu where EmpID = @MaNV", con))
					{
					con.Open();

					command.Parameters.AddWithValue("@MaNV", txtMaNV.Text);
						command.Parameters.AddWithValue("@HoTen", txtHoTen.Text);
						command.Parameters.AddWithValue("@GioiTinh", radNam.IsChecked == true ? "Nam" : "Nữ");
						command.Parameters.AddWithValue("@Phone", txtDienThoai.Text);
						command.Parameters.AddWithValue("@HireDate", dtpNgayVaoLam.SelectedDate);
						command.Parameters.AddWithValue("@Type", radBanHang.IsChecked == true ? "Bán Hàng" : "Giao nhận");
						command.Parameters.AddWithValue("@DoanhSo", txtDoanhSo.Text);
						command.Parameters.AddWithValue("@PhuCapNhienLieu", txtPCNhienLieu.Text);

						command.ExecuteNonQuery();
					}
				}

				// Refresh the ListView to show the updated information
				lvDS.Items.Refresh();

				MessageBox.Show(
					"Your change has been updated",
					"Confirm",
					MessageBoxButton.OK, MessageBoxImage.Information);

			}
		}
	}
	


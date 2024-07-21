using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using PRN221_SE1729_Group11_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PRN221_SE1729_Group11_Project
{
    /// <summary>
    /// Interaction logic for ManageBooking.xaml
    /// </summary>
    public partial class ManageBooking : Page
    {
            private ObservableCollection<Booking> Bookings;
            private PRN_PROJECTContext _context;

            public ManageBooking()
            {
                InitializeComponent();
                InitializeDbContext();
                Bookings = new ObservableCollection<Booking>();
                lvBookings.ItemsSource = Bookings;

                InitializeComboBoxes();
                LoadBookings();
                LoadComboBoxData();
            }

            private void InitializeDbContext()
            {
                try
                {
                    _context = new PRN_PROJECTContext();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to initialize database context: " + ex.Message);
                }
            }

            private void LoadBookings()
            {
                if (_context == null) return;

                try
                {
                    Bookings.Clear();
                var customers = _context.Customers.ToList();
                cbCustomer.ItemsSource = customers;

                var products = _context.Products.ToList();
                cbProduct.ItemsSource = products;

                var bookings = _context.Bookings.Include(b => b.CidNavigation).Include(b => b.PidNavigation).ToList();
                lvBookings.ItemsSource = bookings;
                foreach (var booking in bookings)
                    {
                        Bookings.Add(booking);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load bookings: " + ex.Message);
                }
            }

            private void btnAdd_Click(object sender, RoutedEventArgs e)
            {
                if (_context == null) return;

                if (ValidateInputs())
                {
                    var newBooking = new Booking
                    {
                        Cid = (int?)cbCustomer.SelectedValue,
                        Pid = (int?)cbProduct.SelectedValue,
                        RentDate = dpRentDate.SelectedDate,
                        PayStatus = chkPayStatus.IsChecked == true ? 1 : 0,
                        BookingStatus = rbNotYet.IsChecked == true ? 0 : rbBooking.IsChecked == true ? 1 : 2,
                        PayProof = txtPayProof.Text,
                        Note = txtNote.Text
                    };
                try
                {
                    var selectedCustomer = _context.Customers.Find(newBooking.Cid);
                    if (selectedCustomer != null)
                    {
                        selectedCustomer.NumberOfOrder += 1;
                    }
                    var selectedProduct = _context.Products.Find(newBooking.Pid);
                    if (selectedProduct != null)
                    {
                        selectedProduct.RentedTime += 1;
                    }
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to add booking: " + ex.Message);
                }
                AddBooking(newBooking);
                }
            }

            private void btnEdit_Click(object sender, RoutedEventArgs e)
            {
                if (_context == null) return;

                if (lvBookings.SelectedItem is Booking selectedBooking)
                {
                    if (ValidateInputs())
                    {
                        selectedBooking.Cid = (int?)cbCustomer.SelectedValue;
                        selectedBooking.Pid = (int?)cbProduct.SelectedValue;
                        selectedBooking.RentDate = dpRentDate.SelectedDate;
                        selectedBooking.PayStatus = chkPayStatus.IsChecked == true ? 1 : 0;
                        selectedBooking.BookingStatus = rbNotYet.IsChecked == true ? 0 : rbBooking.IsChecked == true ? 1 : 2;
                        selectedBooking.PayProof = txtPayProof.Text;
                        selectedBooking.Note = txtNote.Text;

                        EditBooking(selectedBooking);
                    }
                }
            }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null) return;

            if (lvBookings.SelectedItem is Booking selectedBooking)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this book?", "Delete Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        try
                        {
                            var selectedCustomer = _context.Customers.Find(selectedBooking.Cid);
                            if (selectedCustomer != null)
                            {
                                selectedCustomer.NumberOfOrder -= 1;
                            }
                            var selectedProduct = _context.Products.Find(selectedBooking.Pid);
                            if (selectedProduct != null)
                            {
                                selectedProduct.RentedTime -= 1;
                            }
                            _context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to add booking: " + ex.Message);
                        }
                        _context.Bookings.Remove(selectedBooking);
                        _context.SaveChanges();
                        Bookings.Remove(selectedBooking);
                        ClearInputs();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to delete booking: " + ex.Message);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
            {
                ClearInputs();
                LoadBookings();
            }

            private void lvBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (lvBookings.SelectedItem is Booking selectedBooking)
                {
                    cbCustomer.SelectedValue = selectedBooking.Cid;
                    cbProduct.SelectedValue = selectedBooking.Pid;
                    dpRentDate.SelectedDate = selectedBooking.RentDate;
                    chkPayStatus.IsChecked = selectedBooking.PayStatus == 1;
                    rbNotYet.IsChecked = selectedBooking.BookingStatus == 0;
                    rbDone.IsChecked = selectedBooking.BookingStatus == 2;
                rbBooking.IsChecked = selectedBooking.BookingStatus == 1;
                    txtPayProof.Text = selectedBooking.PayProof;
                    txtNote.Text = selectedBooking.Note;
                }
            }

            private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
            {
                FilterAndSortBookings();
            }

            private void cbSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                FilterAndSortBookings();
            }

            private void cbSortField_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                FilterAndSortBookings();
            }

            private void rbAscending_Checked(object sender, RoutedEventArgs e)
            {
                FilterAndSortBookings();
            }

            private void rbDescending_Checked(object sender, RoutedEventArgs e)
            {
                FilterAndSortBookings();
            }

            private void FilterAndSortBookings()
            {
                if (_context == null) return;

                try
                {
                    var query = _context.Bookings.AsQueryable();

                    // Filter
                    var searchText = txtSearch.Text;
                    var searchField = (cbSearchField.SelectedItem as ComboBoxItem)?.Content.ToString();
                    if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(searchField))
                    {
                        switch (searchField)
                        {
                            case "ID":
                                if (int.TryParse(searchText, out int bookingId))
                                    query = query.Where(b => b.BookingId == bookingId);
                                break;
                            case "Customer ID":
                                if (int.TryParse(searchText, out int cid))
                                    query = query.Where(b => b.Cid == cid);
                                break;
                            case "Product ID":
                                if (int.TryParse(searchText, out int pid))
                                    query = query.Where(b => b.Pid == pid);
                                break;
                            case "Rent Date":
                                if (DateTime.TryParse(searchText, out DateTime rentDate))
                                    query = query.Where(b => b.RentDate == rentDate);
                                break;
                            case "Pay Status":
                                if (int.TryParse(searchText, out int payStatus))
                                    query = query.Where(b => b.PayStatus == payStatus);
                                break;
                            case "Booking Status":
                                if (int.TryParse(searchText, out int bookingStatus))
                                    query = query.Where(b => b.BookingStatus == bookingStatus);
                                break;
                            case "Pay Proof":
                                query = query.Where(b => b.PayProof.Contains(searchText));
                                break;
                            case "Note":
                                query = query.Where(b => b.Note.Contains(searchText));
                                break;
                        }
                    }

                    // Sort
                    var sortField = (cbSortField.SelectedItem as ComboBoxItem)?.Content.ToString();
                    bool ascending = rbAscending.IsChecked == true;

                    if (!string.IsNullOrEmpty(sortField))
                    {
                        switch (sortField)
                        {
                            case "ID":
                                query = ascending ? query.OrderBy(b => b.BookingId) : query.OrderByDescending(b => b.BookingId);
                                break;
                            case "Customer ID":
                                query = ascending ? query.OrderBy(b => b.Cid) : query.OrderByDescending(b => b.Cid);
                                break;
                            case "Product ID":
                                query = ascending ? query.OrderBy(b => b.Pid) : query.OrderByDescending(b => b.Pid);
                                break;
                            case "Rent Date":
                                query = ascending ? query.OrderBy(b => b.RentDate) : query.OrderByDescending(b => b.RentDate);
                                break;
                            case "Pay Status":
                                query = ascending ? query.OrderBy(b => b.PayStatus) : query.OrderByDescending(b => b.PayStatus);
                                break;
                            case "Booking Status":
                                query = ascending ? query.OrderBy(b => b.BookingStatus) : query.OrderByDescending(b => b.BookingStatus);
                                break;
                            case "Pay Proof":
                                query = ascending ? query.OrderBy(b => b.PayProof) : query.OrderByDescending(b => b.PayProof);
                                break;
                            case "Note":
                                query = ascending ? query.OrderBy(b => b.Note) : query.OrderByDescending(b => b.Note);
                                break;
                        }
                    }

                    // Load sorted and filtered data
                    Bookings.Clear();
                    foreach (var booking in query.ToList())
                    {
                        Bookings.Add(booking);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to filter and sort bookings: " + ex.Message);
                }
            }

        private bool ValidateInputs()
        {
            if (cbCustomer.SelectedItem is null)
            {
                MessageBox.Show("Please choose customer.");
                return false;
            }
            if (cbProduct.SelectedItem is null)
            {
                MessageBox.Show("Please choose product.");
                return false;
            }
            if (dpRentDate.SelectedDate is null)
            {
                MessageBox.Show("Please choose Rent Date.");
                return false;
            }
            return true;
        }

        private void ClearInputs()
            {
                cbCustomer.SelectedValue = null;
                cbProduct.SelectedValue = null;
                dpRentDate.SelectedDate = null;
                chkPayStatus.IsChecked = false;
                rbNotYet.IsChecked = true;
                rbBooking.IsChecked = false;
                rbDone.IsChecked = false;
                txtPayProof.Text = string.Empty;
                txtNote.Text = string.Empty;
            }

            private void InitializeComboBoxes()
            {
                cbSearchField.SelectedIndex = 0;
                cbSortField.SelectedIndex = 0;
                rbAscending.IsChecked = true;
            }

        private void btnProduct_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManageProducts());
        }

        private void btnNewCustomer_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManageCustomer());
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var bookings = lvBookings.ItemsSource as IEnumerable<Booking>;
            if (bookings == null || !bookings.Any())
            {
                MessageBox.Show("No data to export.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "Save an Excel File"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Bookings");

                    worksheet.Cells[1, 1].Value = "BookingId";
                    worksheet.Cells[1, 2].Value = "Customer";
                    worksheet.Cells[1, 3].Value = "Product";
                    worksheet.Cells[1, 4].Value = "RentDate";
                    worksheet.Cells[1, 5].Value = "PayStatus";
                    worksheet.Cells[1, 6].Value = "BookingStatus";
                    worksheet.Cells[1, 7].Value = "PayProof";
                    worksheet.Cells[1, 8].Value = "Note";

                    int row = 2;
                    foreach (var booking in bookings)
                    {
                        var customer = _context.Customers
                        .AsNoTracking()
                        .FirstOrDefault(p => p.Cid == booking.Cid);
                        string customerName = "Unknown";
                        if (customer != null) customerName = customer.CustomerName;
                        var product = _context.Products
                        .AsNoTracking()
                        .FirstOrDefault(p => p.Pid == booking.Pid);
                        string productName = "Unknown";
                        if (product != null) productName = product.ProductName;
                        worksheet.Cells[row, 1].Value = booking.BookingId;
                        worksheet.Cells[row, 2].Value = customerName;
                        worksheet.Cells[row, 3].Value = productName;
                        worksheet.Cells[row, 4].Value = booking.RentDate?.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 5].Value = booking.PayStatus;
                        worksheet.Cells[row, 6].Value = booking.BookingStatus;
                        worksheet.Cells[row, 7].Value = booking.PayProof;
                        worksheet.Cells[row, 8].Value = booking.Note;
                        row++;
                    }

                    var fileInfo = new FileInfo(saveFileDialog.FileName);
                    package.SaveAs(fileInfo);

                    MessageBox.Show("Data exported successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void AddBooking(Booking booking)
        {
            using (var context = new PRN_PROJECTContext())
            {
                // Check for existing booking with the same product and date
                var existingBooking = context.Bookings
                    .FirstOrDefault(b => b.Pid == booking.Pid && b.RentDate == booking.RentDate);

                if (existingBooking != null)
                {
                    MessageBox.Show("The product is already booked that day.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                context.Bookings.Add(booking);
                context.SaveChanges();
                MessageBox.Show("Booking added successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadBookings();
            }
        }

        private void EditBooking(Booking booking)
        {
            using (var context = new PRN_PROJECTContext())
            {
                // Check for existing booking with the same product and date, excluding the current booking
                var existingBooking = context.Bookings
                    .FirstOrDefault(b => b.Pid == booking.Pid && b.RentDate == booking.RentDate && b.BookingId != booking.BookingId);

                if (existingBooking != null)
                {
                    MessageBox.Show("The product is already booked that day.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var existing = context.Bookings.Find(booking.BookingId);
                if (existing != null)
                {
                    existing.Cid = booking.Cid;
                    existing.Pid = booking.Pid;
                    existing.RentDate = booking.RentDate;
                    existing.PayStatus = booking.PayStatus;
                    existing.BookingStatus = booking.BookingStatus;
                    existing.PayProof = booking.PayProof;
                    existing.Note = booking.Note;
                    context.SaveChanges();
                    MessageBox.Show("Booking edited successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadBookings();
                }
                else
                {
                    MessageBox.Show("Booking not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadComboBoxData()
        {
            using (var context = new PRN_PROJECTContext())
            {
                var customers = context.Customers.ToList();
                var products = context.Products.ToList();

                if (customers.Any())
                {
                    cbCustomer.ItemsSource = customers;
                }
                else
                {
                    MessageBox.Show("No customers found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (products.Any())
                {
                    cbProduct.ItemsSource = products;
                }
                else
                {
                    MessageBox.Show("No products found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void txtSearchCustomer_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterComboBox(cbCustomer, txtSearchCustomer.Text);
        }

        private void txtSearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterComboBox(cbProduct, txtSearchProduct.Text);
        }

        private void FilterComboBox(ComboBox comboBox, string filterText)
        {
            if (_context == null) return;

            try
            {
                if (comboBox.ItemsSource is IEnumerable<Customer>)
                {
                    var query = _context.Customers.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(filterText))
                    {
                        query = query.Where(customer => customer.CustomerName.ToLower().Contains(filterText.ToLower()) ||
                                                        customer.PhoneNumber.ToLower().Contains(filterText.ToLower()));
                    }
                    comboBox.ItemsSource = query.ToList();
                }
                else if (comboBox.ItemsSource is IEnumerable<Product>)
                {
                    var query = _context.Products.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(filterText))
                    {
                        query = query.Where(product => product.ProductName.ToLower().Contains(filterText.ToLower()) ||
                                                       product.Relate.ToLower().Contains(filterText.ToLower()));
                    }
                    comboBox.ItemsSource = query.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to filter items: " + ex.Message);
            }
        }
    }
}

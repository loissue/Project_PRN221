using Microsoft.Win32;
using OfficeOpenXml;
using PRN221_SE1729_Group11_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PRN221_SE1729_Group11_Project
{
    /// <summary>
    /// Interaction logic for ManageCustomer.xaml
    /// </summary>
    public partial class ManageCustomer : Page
    {
        private ObservableCollection<Customer> Customers;
        private PRN_PROJECTContext _context;

        public ManageCustomer()
        {
            InitializeComponent();
            InitializeDbContext();
            Customers = new ObservableCollection<Customer>();
            lvCustomers.ItemsSource = Customers;

            InitializeComboBoxes();
            LoadCustomers();
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

        private void LoadCustomers()
        {
            if (_context == null) return;

            try
            {
                Customers.Clear();
                var customersFromDb = _context.Customers.ToList();
                foreach (var customer in customersFromDb)
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load customers: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null) return;

            if (ValidateInputs())
            {
                var newCustomer = new Customer
                {
                    CustomerName = txtCustomerName.Text,
                    Dob = dpDob.SelectedDate,
                    IdentificationCard = txtIdentificationCard.Text,
                    Address = txtAddress.Text,
                    NumberOfOrder = int.TryParse(txtNumberOfOrders.Text, out int numberOfOrders) ? numberOfOrders : (int?)null,
                    PhoneNumber = txtPhoneNumber.Text,
                };

                try
                {
                    _context.Customers.Add(newCustomer);
                    _context.SaveChanges();
                    lvCustomers.Items.Refresh();
                    Customers.Add(newCustomer);
                    ClearInputs();
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Failed to add customer: " + ex.Message);
                }
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null) return;

            if (lvCustomers.SelectedItem is Customer selectedCustomer)
            {
                if (ValidateInputs())
                {
                    selectedCustomer.CustomerName = txtCustomerName.Text;
                    selectedCustomer.Dob = dpDob.SelectedDate;
                    selectedCustomer.IdentificationCard = txtIdentificationCard.Text;
                    selectedCustomer.Address = txtAddress.Text;
                    selectedCustomer.NumberOfOrder = int.TryParse(txtNumberOfOrders.Text, out int numberOfOrders) ? numberOfOrders : (int?)null;
                    selectedCustomer.PhoneNumber = txtPhoneNumber.Text;
                    try
                    {
                        _context.Customers.Update(selectedCustomer);
                        _context.SaveChanges();
                        lvCustomers.Items.Refresh();
                        ClearInputs();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to edit customer: " + ex.Message);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null) return;

            if (lvCustomers.SelectedItem is Customer selectedCustomer)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this customer? \n All of his/her bookings will also be delete .", "Delete Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Delete associated bookings
                        var associatedBookings = _context.Bookings.Where(b => b.Cid == selectedCustomer.Cid).ToList();
                        _context.Bookings.RemoveRange(associatedBookings);

                        // Delete the customer
                        _context.Customers.Remove(selectedCustomer);
                        _context.SaveChanges();
                        Customers.Remove(selectedCustomer);
                        ClearInputs();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to delete customer: " + ex.Message);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
            LoadCustomers();
        }

        private void lvCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvCustomers.SelectedItem is Customer selectedCustomer)
            {
                txtCid.Text = selectedCustomer.Cid.ToString();
                txtCustomerName.Text = selectedCustomer.CustomerName;
                dpDob.SelectedDate = selectedCustomer.Dob;
                txtIdentificationCard.Text = selectedCustomer.IdentificationCard;
                txtAddress.Text = selectedCustomer.Address;
                txtNumberOfOrders.Text = selectedCustomer.NumberOfOrder?.ToString();
                txtPhoneNumber.Text = selectedCustomer.PhoneNumber;
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAndSortCustomers();
        }

        private void cbSearchField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterAndSortCustomers();
        }

        private void cbSortField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterAndSortCustomers();
        }

        private void rbAscending_Checked(object sender, RoutedEventArgs e)
        {
            FilterAndSortCustomers();
        }

        private void rbDescending_Checked(object sender, RoutedEventArgs e)
        {
            FilterAndSortCustomers();
        }

        private void FilterAndSortCustomers()
        {
            if (_context == null) return;

            try
            {
                var query = _context.Customers.AsQueryable();

                // Filter
                var searchText = txtSearch.Text;
                var searchField = (cbSearchField.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(searchField))
                {
                    switch (searchField)
                    {
                        case "ID":
                            if (int.TryParse(searchText, out int cid))
                                query = query.Where(c => c.Cid == cid);
                            break;
                        case "Name":
                            query = query.Where(c => c.CustomerName.Contains(searchText));
                            break;
                        case "Identification Card":
                            query = query.Where(c => c.IdentificationCard.Contains(searchText));
                            break;
                        case "Address":
                            query = query.Where(c => c.Address.Contains(searchText));
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
                            query = ascending ? query.OrderBy(c => c.Cid) : query.OrderByDescending(c => c.Cid);
                            break;
                        case "Name":
                            query = ascending ? query.OrderBy(c => c.CustomerName) : query.OrderByDescending(c => c.CustomerName);
                            break;
                        case "Date of Birth":
                            query = ascending ? query.OrderBy(c => c.Dob) : query.OrderByDescending(c => c.Dob);
                            break;
                        case "Number of Orders":
                            query = ascending ? query.OrderBy(c => c.NumberOfOrder) : query.OrderByDescending(c => c.NumberOfOrder);
                            break;
                    }
                }

                // Load sorted and filtered data
                Customers.Clear();
                foreach (var customer in query.ToList())
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to filter and sort customers: " + ex.Message);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Customer Name cannot be empty.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text) || txtPhoneNumber.Text.Length<10 || txtPhoneNumber.Text.Length>11)
            {
                MessageBox.Show("Wrong Phone Number format, or phone number is empty.");
                return false;
            }
            if(dpDob.SelectedDate is null)
            {
                MessageBox.Show("PLease choose dob.");
                return false;
            }
            else
            {
                if (dpDob.SelectedDate > DateTime.Today)
                {
                    MessageBox.Show("Wrong Dob, this customer has'n been born yet.");
                    return false;
                }
            }
            return true;
        }

        private void ClearInputs()
        {
            txtCid.Text = string.Empty;
            txtCustomerName.Text = string.Empty;
            dpDob.SelectedDate = null;
            txtIdentificationCard.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtNumberOfOrders.Text = "0";
            txtPhoneNumber.Text = string.Empty;
        }

        private void InitializeComboBoxes()
        {
            cbSearchField.SelectedIndex = 0;
            cbSortField.SelectedIndex = 0;
            rbAscending.IsChecked = true;
        }

        private void btnBooking_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManageBooking());
        }

        private void btnProducts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManageProducts());
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var customers = lvCustomers.ItemsSource as IEnumerable<Customer>;
            if (customers == null || !customers.Any())
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
                    var worksheet = package.Workbook.Worksheets.Add("Customers");

                    worksheet.Cells[1, 1].Value = "CID";
                    worksheet.Cells[1, 2].Value = "CustomerName";
                    worksheet.Cells[1, 3].Value = "Dob";
                    worksheet.Cells[1, 4].Value = "IdentificationCard";
                    worksheet.Cells[1, 5].Value = "Address";
                    worksheet.Cells[1, 6].Value = "NumberOfOrder";
                    worksheet.Cells[1, 7].Value = "PhoneNumber";
                    int row = 2;
                    foreach (var customer in customers)
                    {
                        worksheet.Cells[row, 1].Value = customer.Cid;
                        worksheet.Cells[row, 2].Value = customer.CustomerName;
                        worksheet.Cells[row, 3].Value = customer.Dob?.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 4].Value = customer.IdentificationCard;
                        worksheet.Cells[row, 5].Value = customer.Address;
                        worksheet.Cells[row, 6].Value = customer.NumberOfOrder;
                        worksheet.Cells[row, 7].Value = customer.PhoneNumber;
                        row++;
                    }

                    var fileInfo = new FileInfo(saveFileDialog.FileName);
                    package.SaveAs(fileInfo);

                    MessageBox.Show("Data exported successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}

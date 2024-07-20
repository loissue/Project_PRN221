using PRN221_SE1729_Group11_Project.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    NumberOfOrder = int.TryParse(txtNumberOfOrders.Text, out int numberOfOrders) ? numberOfOrders : (int?)null
                };

                try
                {
                    _context.Customers.Add(newCustomer);
                    _context.SaveChanges();
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
                try
                {
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
            return !string.IsNullOrEmpty(txtCustomerName.Text) &&
                   dpDob.SelectedDate != null &&
                   !string.IsNullOrEmpty(txtIdentificationCard.Text) &&
                   !string.IsNullOrEmpty(txtAddress.Text);
        }

        private void ClearInputs()
        {
            txtCid.Text = string.Empty;
            txtCustomerName.Text = string.Empty;
            dpDob.SelectedDate = null;
            txtIdentificationCard.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtNumberOfOrders.Text = string.Empty;
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
    }
}

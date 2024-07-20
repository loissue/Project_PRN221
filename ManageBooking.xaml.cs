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
                    var bookingsFromDb = _context.Bookings.ToList();
                    foreach (var booking in bookingsFromDb)
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
                        Cid = int.TryParse(txtCustomerId.Text, out int cid) ? cid : (int?)null,
                        Pid = int.TryParse(txtProductId.Text, out int pid) ? pid : (int?)null,
                        RentDate = dpRentDate.SelectedDate,
                        PayStatus = int.TryParse(txtPayStatus.Text, out int payStatus) ? payStatus : (int?)null,
                        BookingStatus = int.TryParse(txtBookingStatus.Text, out int bookingStatus) ? bookingStatus : (int?)null,
                        PayProof = txtPayProof.Text,
                        Note = txtNote.Text
                    };

                    try
                    {
                        _context.Bookings.Add(newBooking);
                        _context.SaveChanges();
                        Bookings.Add(newBooking);
                        ClearInputs();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to add booking: " + ex.Message);
                    }
                }
            }

            private void btnEdit_Click(object sender, RoutedEventArgs e)
            {
                if (_context == null) return;

                if (lvBookings.SelectedItem is Booking selectedBooking)
                {
                    if (ValidateInputs())
                    {
                        selectedBooking.Cid = int.TryParse(txtCustomerId.Text, out int cid) ? cid : (int?)null;
                        selectedBooking.Pid = int.TryParse(txtProductId.Text, out int pid) ? pid : (int?)null;
                        selectedBooking.RentDate = dpRentDate.SelectedDate;
                        selectedBooking.PayStatus = int.TryParse(txtPayStatus.Text, out int payStatus) ? payStatus : (int?)null;
                        selectedBooking.BookingStatus = int.TryParse(txtBookingStatus.Text, out int bookingStatus) ? bookingStatus : (int?)null;
                        selectedBooking.PayProof = txtPayProof.Text;
                        selectedBooking.Note = txtNote.Text;

                        try
                        {
                            _context.Bookings.Update(selectedBooking);
                            _context.SaveChanges();
                            lvBookings.Items.Refresh();
                            ClearInputs();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to edit booking: " + ex.Message);
                        }
                    }
                }
            }

            private void btnDelete_Click(object sender, RoutedEventArgs e)
            {
                if (_context == null) return;

                if (lvBookings.SelectedItem is Booking selectedBooking)
                {
                    try
                    {
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

            private void btnRefresh_Click(object sender, RoutedEventArgs e)
            {
                ClearInputs();
                LoadBookings();
            }

            private void lvBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (lvBookings.SelectedItem is Booking selectedBooking)
                {
                    txtCustomerId.Text = selectedBooking.Cid.ToString();
                    txtProductId.Text = selectedBooking.Pid.ToString();
                    dpRentDate.SelectedDate = selectedBooking.RentDate;
                    txtPayStatus.Text = selectedBooking.PayStatus?.ToString();
                    txtBookingStatus.Text = selectedBooking.BookingStatus?.ToString();
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
                return !string.IsNullOrEmpty(txtCustomerId.Text) &&
                       !string.IsNullOrEmpty(txtProductId.Text) &&
                       dpRentDate.SelectedDate != null;
            }

            private void ClearInputs()
            {
                txtCustomerId.Text = string.Empty;
                txtProductId.Text = string.Empty;
                dpRentDate.SelectedDate = null;
                txtPayStatus.Text = string.Empty;
                txtBookingStatus.Text = string.Empty;
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
    }
}

using BlApi;
using BO;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PL.Order;

namespace PL.Courier
{
    public partial class MainCourierWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();
        private int _courierId;

        // Helper flag to prevent password update loops
        private bool _isPasswordSyncing = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        // --- Dependency Properties ---

        public BO.Courier Courier
        {
            get { return (BO.Courier)GetValue(CourierProperty); }
            set { SetValue(CourierProperty, value); }
        }
        public static readonly DependencyProperty CourierProperty =
            DependencyProperty.Register("Courier", typeof(BO.Courier), typeof(MainCourierWindow));

        public bool HasActiveOrder
        {
            get { return (bool)GetValue(HasActiveOrderProperty); }
            set { SetValue(HasActiveOrderProperty, value); }
        }
        public static readonly DependencyProperty HasActiveOrderProperty =
            DependencyProperty.Register("HasActiveOrder", typeof(bool), typeof(MainCourierWindow));

        public bool HasNoActiveOrder => !HasActiveOrder;
        public bool IsVehicleEditable => HasNoActiveOrder;

        public MainCourierWindow(int courierId)
        {
            InitializeComponent();
            _courierId = courierId;
            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                Courier = s_bl.Courier.SearchCourier(_courierId, _courierId);

                // Update password in the hidden box (PasswordBox does not support Binding)
                if (Courier.Password != null)
                {
                    _isPasswordSyncing = true;
                    pbPassword.Password = Courier.Password;
                    _isPasswordSyncing = false;
                }

                HasActiveOrder = (Courier.OrderInCare != null);

                OnPropertyChanged(nameof(HasNoActiveOrder));
                OnPropertyChanged(nameof(IsVehicleEditable));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
                Close();
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Password Logic ---

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // If the user types in the hidden box, update the object
            if (!_isPasswordSyncing)
            {
                Courier.Password = pbPassword.Password;
            }
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between visible and hidden modes
            if (pbPassword.Visibility == Visibility.Visible)
            {
                // Switch to visible mode
                pbPassword.Visibility = Visibility.Collapsed;
                txtVisiblePassword.Visibility = Visibility.Visible;

                // Update visible text (Binding handles this, but ensuring sync)
                // txtVisiblePassword is bound via Binding, so it updates automatically
            }
            else
            {
                // Switch to hidden mode (dots)
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                pbPassword.Visibility = Visibility.Visible;

                // Sync back in case the visible text changed
                _isPasswordSyncing = true;
                pbPassword.Password = Courier.Password;
                _isPasswordSyncing = false;
            }
        }

        // --- Main Actions ---

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure password is updated in the object before sending (if edited in the visible box)
                if (txtVisiblePassword.Visibility == Visibility.Visible)
                    Courier.Password = txtVisiblePassword.Text;
                else
                    Courier.Password = pbPassword.Password;

                s_bl.Courier.UpdateCourier(_courierId, Courier);
                MessageBox.Show("Details updated successfully!", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RefreshData();
            }
        }

        private void BtnFinishOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Courier.OrderInCare == null) return;

            try
            {
                // DeliveryId already exists in the OrderInCare object from Tools
                int deliveryId = Courier.OrderInCare.DeliveryId;

                // Close the order
                s_bl.Order.CloseOrder(_courierId, _courierId, deliveryId);

                MessageBox.Show("Order delivered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finishing order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
        {
            // פתח את חלון בחירת ההזמנות
            OrdersToPick pickWindow = new OrdersToPick(_courierId);
            if (pickWindow.ShowDialog() == true)
            {
                // Window closed with DialogResult = true
                // Refresh the current delivery display
                LoadCurrentDelivery(); // Call whatever method updates the "Current Delivery" display
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            OrdersHistory historyWindow = new OrdersHistory(_courierId);
            historyWindow.ShowDialog();
        }
        

        // Add this method to the MainCourierWindow class to resolve CS0103
        private void LoadCurrentDelivery()
        {
            // Refresh the courier data, which updates the current delivery display
            RefreshData();
        }

          
    }
}
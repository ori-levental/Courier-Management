using BlApi;
using BO;
using PL.Order;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Courier
{
    /// <summary>
    /// Interaction logic for MainCourierWindow.
    /// Implements the Observer pattern to synchronize UI state with Business Layer changes.
    /// </summary>
    public partial class MainCourierWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();
        private int _courierId;

        // Helper for password syncing to prevent infinite loops
        private bool _isPasswordSyncing = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Dependency Properties

        // Using DependencyProperties to enable built-in WPF binding engine optimizations
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

        // --- Addition: List of options for completion status (excluding Cancelled) ---
        public IEnumerable<BO.ShipmentCompletionStatus> DeliveryOutcomeOptions
        {
            get { return (IEnumerable<BO.ShipmentCompletionStatus>)GetValue(DeliveryOutcomeOptionsProperty); }
            set { SetValue(DeliveryOutcomeOptionsProperty, value); }
        }
        public static readonly DependencyProperty DeliveryOutcomeOptionsProperty =
            DependencyProperty.Register("DeliveryOutcomeOptions", typeof(IEnumerable<BO.ShipmentCompletionStatus>), typeof(MainCourierWindow));

        // --- Addition: The status selected by the courier ---
        public BO.ShipmentCompletionStatus SelectedOutcome
        {
            get { return (BO.ShipmentCompletionStatus)GetValue(SelectedOutcomeProperty); }
            set { SetValue(SelectedOutcomeProperty, value); }
        }
        public static readonly DependencyProperty SelectedOutcomeProperty =
            DependencyProperty.Register("SelectedOutcome", typeof(BO.ShipmentCompletionStatus), typeof(MainCourierWindow),
                new PropertyMetadata(BO.ShipmentCompletionStatus.Provided)); // Default: Provided

        #endregion

        #region Calculated Properties

        // These properties depend on the state of HasActiveOrder.
        // PropertyChanged notifications are manually triggered in RefreshData().
        public bool HasNoActiveOrder => !HasActiveOrder;
        public bool IsVehicleEditable => HasNoActiveOrder;

        #endregion

        #region Observer Implementation

        /// <summary>
        /// Observer callback invoked by the Business Layer.
        /// Executes UI refresh on the Dispatcher thread to avoid Cross-Thread Exceptions.
        /// </summary>
        private void courierObserver() => this.Dispatcher.Invoke(RefreshData);

        #endregion

        public MainCourierWindow(int courierId)
        {
            InitializeComponent();
            _courierId = courierId;

            // Explicitly setting DataContext to allow Property binding in XAML
            this.DataContext = this;

            // --- Initialize status list (manual filtering) ---
            DeliveryOutcomeOptions = new List<BO.ShipmentCompletionStatus>
            {
                BO.ShipmentCompletionStatus.Provided,
                BO.ShipmentCompletionStatus.Refused,
            };

            // Set default selection
            SelectedOutcome = BO.ShipmentCompletionStatus.Provided;

            // Lifecycle event registration for Observer subscription management
            this.Loaded += Window_Loaded;
            this.Closed += Window_Closed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
            // Listen to Order changes (status updates) and Courier changes (details)
            s_bl.Order.AddObserver(courierObserver);
            s_bl.Courier.AddObserver(courierObserver);
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            s_bl.Order.RemoveObserver(courierObserver);
            s_bl.Courier.RemoveObserver(courierObserver);
        }

        /// <summary>
        /// Synchronizes the local Courier state with the database and notifies the UI.
        /// </summary>
        private void RefreshData()
        {
            try
            {
                // FIX: Use SearchCourier correctly
                Courier = s_bl.Courier.SearchCourier(_courierId, _courierId);

                // Sync password to PasswordBox UI
                if (Courier.Password != null && pbPassword != null)
                {
                    _isPasswordSyncing = true;
                    pbPassword.Password = Courier.Password;
                    _isPasswordSyncing = false;
                }

                // FIX: Use OrderInCare property
                HasActiveOrder = (Courier.OrderInCare != null);

                // Manually notify UI of changes in non-Dependency calculated properties
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasNoActiveOrder)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVehicleEditable)));
            }
            catch (Exception ex)
            {
                // Critical data fetching errors are caught here to prevent UI thread termination
                MessageBox.Show($"Data Synchronization Error: {ex.Message}", "Sync Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        #region UI Handlers

        private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
        {
            // The observer will automatically update the main window once an order is assigned in the child window.
            new OrdersToPick(_courierId).ShowDialog();
        }

        private void BtnFinishOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verify there is an active order
                if (Courier.OrderInCare != null)
                {
                    // FIX: Use the status selected in the ComboBox instead of a hardcoded one
                    s_bl.Order.CloseOrder(_courierId, _courierId, Courier.OrderInCare.DeliveryId, SelectedOutcome);

                    MessageBox.Show($"Order marked as {SelectedOutcome} successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    // RefreshData() is called automatically by the Observer!
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            OrdersHistory historyWindow = new OrdersHistory(_courierId);
            historyWindow.ShowDialog();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure password is sync
                if (txtVisiblePassword.Visibility == Visibility.Visible)
                    Courier.Password = txtVisiblePassword.Text;
                else
                    Courier.Password = pbPassword.Password;

                s_bl.Courier.UpdateCourier(Courier.Id, Courier);
                MessageBox.Show("Details updated successfully!", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                // Observer will trigger refresh
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RefreshData(); // Revert changes in UI
            }
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordSyncing)
            {
                Courier.Password = pbPassword.Password;
            }
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (pbPassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = pbPassword.Password;
                pbPassword.Visibility = Visibility.Collapsed;
                txtVisiblePassword.Visibility = Visibility.Visible;
            }
            else
            {
                pbPassword.Password = txtVisiblePassword.Text;
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                pbPassword.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}
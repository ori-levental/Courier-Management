using BlApi;
using BO;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using PL.Order;

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
            _courierId = courierId;
            InitializeComponent();

            // Explicitly setting DataContext to allow Property binding in XAML
            this.DataContext = this;

            // Lifecycle event registration for Observer subscription management
            this.Loaded += (s, e) => s_bl.Order.AddObserver(courierObserver);
            this.Closed += (s, e) => s_bl.Order.RemoveObserver(courierObserver);

            RefreshData();
        }

        /// <summary>
        /// Synchronizes the local Courier state with the database and notifies the UI.
        /// </summary>
        private void RefreshData()
        {
            try
            {
                Courier = s_bl.Courier.GetCourierById(_courierId);
                HasActiveOrder = Courier.CurrentDelivery != null;

                // Manually notify UI of changes in non-Dependency calculated properties
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasNoActiveOrder)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVehicleEditable)));
            }
            catch (Exception ex)
            {
                // Critical data fetching errors are caught here to prevent UI thread termination
                MessageBox.Show($"Data Synchronization Error: {ex.Message}", "Sync Failure", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (Courier.CurrentDelivery != null)
                {
                    s_bl.Order.CloseOrder(_courierId, _courierId, Courier.CurrentDelivery.DeliveryId);
                    MessageBox.Show("Delivery completed successfully.", "Success");
                    // RefreshData() call is optional here if the BL triggers the observer immediately.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to close order: {ex.Message}");
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            new OrdersHistory(_courierId).ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        #endregion
    }
}
using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PL.Courier
{
    /// <summary>
    /// Interaction logic for OrdersHistory.xaml
    /// Displays the closed order history for a specific courier.
    /// </summary>
    public partial class OrdersHistory : Window, INotifyPropertyChanged
    {
        #region BL Access
        static readonly IBl s_bl = Factory.Get();
        #endregion

        #region Properties

        // The Courier ID being viewed
        private int CourierId { get; set; }

        // Dependency Property: ClosedOrdersList (Bound to ListView)
        public IEnumerable<ClosedDeliveryDisplayWrapper> ClosedOrdersList
        {
            get { return (IEnumerable<ClosedDeliveryDisplayWrapper>)GetValue(ClosedOrdersListProperty); }
            set { SetValue(ClosedOrdersListProperty, value); }
        }
        public static readonly DependencyProperty ClosedOrdersListProperty =
            DependencyProperty.Register("ClosedOrdersList", typeof(IEnumerable<ClosedDeliveryDisplayWrapper>), typeof(OrdersHistory));

        // Dependency Property: CourierInfo (Display courier name)
        public string CourierInfo
        {
            get { return (string)GetValue(CourierInfoProperty); }
            set { SetValue(CourierInfoProperty, value); }
        }
        public static readonly DependencyProperty CourierInfoProperty =
            DependencyProperty.Register("CourierInfo", typeof(string), typeof(OrdersHistory), new PropertyMetadata(""));

        // Dependency Property: TotalDeliveries (Statistics)
        public int TotalDeliveries
        {
            get { return (int)GetValue(TotalDeliveriesProperty); }
            set { SetValue(TotalDeliveriesProperty, value); }
        }
        public static readonly DependencyProperty TotalDeliveriesProperty =
            DependencyProperty.Register("TotalDeliveries", typeof(int), typeof(OrdersHistory), new PropertyMetadata(0));

        // Dependency Property: SuccessfulDeliveries (Statistics)
        public int SuccessfulDeliveries
        {
            get { return (int)GetValue(SuccessfulDeliveriesProperty); }
            set { SetValue(SuccessfulDeliveriesProperty, value); }
        }
        public static readonly DependencyProperty SuccessfulDeliveriesProperty =
            DependencyProperty.Register("SuccessfulDeliveries", typeof(int), typeof(OrdersHistory), new PropertyMetadata(0));

        // Dependency Property: CancelledDeliveries (Statistics)
        public int CancelledDeliveries
        {
            get { return (int)GetValue(CancelledDeliveriesProperty); }
            set { SetValue(CancelledDeliveriesProperty, value); }
        }
        public static readonly DependencyProperty CancelledDeliveriesProperty =
            DependencyProperty.Register("CancelledDeliveries", typeof(int), typeof(OrdersHistory), new PropertyMetadata(0));

        // Dependency Property: AverageProcessingTime (Statistics)
        public string AverageProcessingTime
        {
            get { return (string)GetValue(AverageProcessingTimeProperty); }
            set { SetValue(AverageProcessingTimeProperty, value); }
        }
        public static readonly DependencyProperty AverageProcessingTimeProperty =
            DependencyProperty.Register("AverageProcessingTime", typeof(string), typeof(OrdersHistory), new PropertyMetadata("00:00:00"));

        // Dependency Property: SelectedEndType (Filter dropdown)
        public string? SelectedEndType
        {
            get { return (string?)GetValue(SelectedEndTypeProperty); }
            set { SetValue(SelectedEndTypeProperty, value); }
        }
        public static readonly DependencyProperty SelectedEndTypeProperty =
            DependencyProperty.Register("SelectedEndType", typeof(string), typeof(OrdersHistory),
                new PropertyMetadata(null, PropertyChanged_SelectedFilter));

        // Dependency Property: SelectedOrderType (Filter dropdown)
        public string? SelectedOrderType
        {
            get { return (string?)GetValue(SelectedOrderTypeProperty); }
            set { SetValue(SelectedOrderTypeProperty, value); }
        }
        public static readonly DependencyProperty SelectedOrderTypeProperty =
            DependencyProperty.Register("SelectedOrderType", typeof(string), typeof(OrdersHistory),
                new PropertyMetadata(null, PropertyChanged_SelectedFilter));

        /// <summary>
        /// Shared property change callback for both filter properties.
        /// </summary>
        private static void PropertyChanged_SelectedFilter(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (OrdersHistory)d;
            
            // Only apply filters if _allClosedOrders has been populated
            if (window._allClosedOrders.Count > 0)
            {
                window.ApplyFiltersAndSort();
            }
        }

        // Internal list for filtering/sorting operations
        private List<BO.ClosedDeliveryInList> _allClosedOrders = new();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor that initializes the window with a specific courier ID.
        /// </summary>
        /// <param name="courierId">The ID of the courier whose history to display.</param>
        public OrdersHistory(int courierId = 0)
        {
            InitializeComponent();

            CourierId = courierId;
            
            // Set the DataContext so bindings work properly
            this.DataContext = this;

            // Register lifecycle events
            this.Loaded += Window_Loaded;
            this.Closed += Window_Closed;
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Refresh the list of closed orders.
        /// </summary>
        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => SafeExec(() => LoadClosedOrders());

        /// <summary>
        /// Clear filters and reload with all closed orders.
        /// </summary>
        private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SelectedEndType = null;
            SelectedOrderType = null;
            CmbEndType.SelectedIndex = 0;
            CmbOrderType.SelectedIndex = 0;
            SafeExec(() => LoadClosedOrders());
        }

        /// <summary>
        /// Close the window.
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        #endregion

        #region Internal Logic

        /// <summary>
        /// Load the list of closed orders for this courier.
        /// </summary>
        private void LoadClosedOrders()
        {
            try
            {
                // Get all closed orders for this courier without filters
                _allClosedOrders = s_bl.Order.CloseOrderByCourier(CourierId, CourierId, null, ClosedDeliveryInListEnum.DeliveryEndTime).ToList();
                
                // Apply filters and sort
                ApplyFiltersAndSort();

                // Calculate statistics
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Apply filters and sort to the closed orders list.
        /// </summary>
        private void ApplyFiltersAndSort()
        {
            // Guard clause: if no orders loaded yet, skip filtering
            if (_allClosedOrders == null || _allClosedOrders.Count == 0)
            {
                ClosedOrdersList = new List<ClosedDeliveryDisplayWrapper>();
                return;
            }

            var filtered = _allClosedOrders.AsEnumerable();

            // Get the selected filter values from ComboBox
            // SelectedItem from ComboBoxItem will be the ComboBoxItem object itself
            // We need to get the Content property or use SelectedValuePath
            string? endTypeFilter = SelectedEndType;
            string? orderTypeFilter = SelectedOrderType;

            // Apply DeliveryEndType filter
            if (!string.IsNullOrEmpty(endTypeFilter) && endTypeFilter != "All")
            {
                filtered = filtered.Where(o => 
                {
                    if (o.DeliveryEndType.HasValue)
                    {
                        string enumValue = o.DeliveryEndType.Value.ToString();
                        return enumValue == endTypeFilter;
                    }
                    return false;
                });
            }

            // Apply OrderType filter
            if (!string.IsNullOrEmpty(orderTypeFilter) && orderTypeFilter != "All")
            {
                filtered = filtered.Where(o => 
                {
                    string enumValue = o.OrderType.ToString();
                    return enumValue == orderTypeFilter;
                });
            }

            // Always sort by DeliveryEndTime descending (most recent first)
            filtered = filtered.OrderByDescending(o => o.DeliveryEndTime);

            // Update the display list
            var finalList = filtered.Select(o => new ClosedDeliveryDisplayWrapper(o)).ToList();
            ClosedOrdersList = finalList;
        }

        /// <summary>
        /// Calculate and update statistics from closed orders.
        /// </summary>
        private void UpdateStatistics()
        {
            if (_allClosedOrders.Count == 0)
            {
                TotalDeliveries = 0;
                SuccessfulDeliveries = 0;
                CancelledDeliveries = 0;
                AverageProcessingTime = "00:00:00";
                return;
            }

            TotalDeliveries = _allClosedOrders.Count;
            SuccessfulDeliveries = _allClosedOrders.Count(o => o.DeliveryEndType == ShipmentCompletionStatus.Provided);
            CancelledDeliveries = _allClosedOrders.Count(o => o.DeliveryEndType == ShipmentCompletionStatus.Cancelled);

            // Calculate average processing time
            var avgTimeSpan = TimeSpan.FromSeconds(_allClosedOrders.Average(o => o.TotalProcessingTime.TotalSeconds));
            AverageProcessingTime = avgTimeSpan.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Load courier information (name).
        /// </summary>
        private void LoadCourierInfo()
        {
            try
            {
                var courier = s_bl.Courier.SearchCourier(CourierId, CourierId);
                CourierInfo = $"Order History - {courier.FullName ?? "Unknown Courier"}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courier info: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CourierInfo = "Order History - Unknown Courier";
            }
        }

        /// <summary>
        /// Observer method triggered by BL when order list changes.
        /// </summary>
        private void orderListObserver()
        {
            // BL events run on a background thread - use Dispatcher to update UI safely
            this.Dispatcher.Invoke(() =>
            {
                LoadClosedOrders();
            });
        }

        #endregion

        #region Lifecycle Events

        /// <summary>
        /// Called when the window is fully loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Load courier information
            LoadCourierInfo();

            // 2. Load closed orders
            LoadClosedOrders();

            // 3. Register for updates (Observer Pattern)
            s_bl.Order.AddObserver(orderListObserver);
        }

        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        private void Window_Closed(object? sender, EventArgs e)
        {
            // Unregister the observer to prevent memory leaks
            s_bl.Order.RemoveObserver(orderListObserver);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region Helpers

        /// <summary>
        /// Executes an action within a try-catch block to handle BL exceptions globally.
        /// </summary>
        private void SafeExec(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Display Wrapper

        /// <summary>
        /// Wrapper class for ClosedDeliveryInList to provide formatted display properties.
        /// </summary>
        public class ClosedDeliveryDisplayWrapper
        {
            private readonly BO.ClosedDeliveryInList _delivery;

            public ClosedDeliveryDisplayWrapper(BO.ClosedDeliveryInList delivery)
            {
                _delivery = delivery;
            }

            public int DeliveryId => _delivery.DeliveryId;
            public int OrderId => _delivery.OrderId;
            public ShipmentCompletionStatus? DeliveryEndType => _delivery.DeliveryEndType;
            public string DeliveryEndTypeDisplay => _delivery.DeliveryEndType?.ToString() ?? "Unknown";
            public DateTime? DeliveryEndTime => _delivery.DeliveryEndTime;
            public string DeliveryEndTimeDisplay => _delivery.DeliveryEndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
            public string FullAddress => _delivery.FullAddress;
            public double ActualDistanceKm => _delivery.ActualDistanceKm ?? 0.0;
            public string ActualDistanceDisplay => $"{(_delivery.ActualDistanceKm ?? 0.0):F2} km";
            public TimeSpan TotalProcessingTime => _delivery.TotalProcessingTime;
            public string TotalProcessingTimeDisplay => _delivery.TotalProcessingTime.ToString(@"hh\:mm\:ss");
            public OrderType OrderType => _delivery.OrderType;
            public BO.ShippingType ShippingType => _delivery.ShippingType;
        }

        #endregion
    }
}

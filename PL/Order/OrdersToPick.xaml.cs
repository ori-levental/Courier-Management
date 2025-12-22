using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for OrdersToPick.xaml
    /// Displays open orders available for a courier to pickup, filtered by distance.
    /// </summary>
    public partial class OrdersToPick : Window, INotifyPropertyChanged
    {
        #region BL Access
        static readonly IBl s_bl = Factory.Get();
        #endregion

        #region Properties

        // The Courier ID that is logged in and browsing orders
        private int CourierId { get; set; }

        // Dependency Property: OrderList (Bound to ListView)
        public IEnumerable<OrderDisplayWrapper> OrderList
        {
            get { return (IEnumerable<OrderDisplayWrapper>)GetValue(OrderListProperty); }
            set { SetValue(OrderListProperty, value); }
        }
        public static readonly DependencyProperty OrderListProperty =
            DependencyProperty.Register("OrderList", typeof(IEnumerable<OrderDisplayWrapper>), typeof(OrdersToPick));

        // Dependency Property: SelectedOrder (Bound to ListView SelectedItem)
        public OrderDisplayWrapper? SelectedOrder
        {
            get { return (OrderDisplayWrapper?)GetValue(SelectedOrderProperty); }
            set { SetValue(SelectedOrderProperty, value); }
        }
        public static readonly DependencyProperty SelectedOrderProperty =
            DependencyProperty.Register("SelectedOrder", typeof(OrderDisplayWrapper), typeof(OrdersToPick),
                new PropertyMetadata(null, (d, e) => ((OrdersToPick)d).UpdateIsOrderSelected()));

        // Dependency Property: CourierInfo (Display courier name)
        public string CourierInfo
        {
            get { return (string)GetValue(CourierInfoProperty); }
            set { SetValue(CourierInfoProperty, value); }
        }
        public static readonly DependencyProperty CourierInfoProperty =
            DependencyProperty.Register("CourierInfo", typeof(string), typeof(OrdersToPick), new PropertyMetadata(""));

        // Dependency Property: MaxDistance (Display courier's max distance)
        public double MaxDistance
        {
            get { return (double)GetValue(MaxDistanceProperty); }
            set { SetValue(MaxDistanceProperty, value); }
        }
        public static readonly DependencyProperty MaxDistanceProperty =
            DependencyProperty.Register("MaxDistance", typeof(double), typeof(OrdersToPick), new PropertyMetadata(0.0));

        // Dependency Property: MaxDistanceDisplay (Formatted display)
        public string MaxDistanceDisplay
        {
            get { return (string)GetValue(MaxDistanceDisplayProperty); }
            set { SetValue(MaxDistanceDisplayProperty, value); }
        }
        public static readonly DependencyProperty MaxDistanceDisplayProperty =
            DependencyProperty.Register("MaxDistanceDisplay", typeof(string), typeof(OrdersToPick), 
                new PropertyMetadata("0.00 km"));

        // Dependency Property: IsOrderSelected (Enable/Disable Pick button)
        public bool IsOrderSelected
        {
            get { return (bool)GetValue(IsOrderSelectedProperty); }
            set { SetValue(IsOrderSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsOrderSelectedProperty =
            DependencyProperty.Register("IsOrderSelected", typeof(bool), typeof(OrdersToPick), new PropertyMetadata(false));

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor that initializes the window with a specific courier ID.
        /// </summary>
        /// <param name="courierId">The ID of the courier viewing available orders.</param>
        public OrdersToPick(int courierId = 0)
        {
            InitializeComponent();

            CourierId = courierId;

            // Register lifecycle events
            this.Loaded += Window_Loaded;
            this.Closed += Window_Closed;
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Double-click on a row to pick the order.
        /// </summary>
        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedOrder != null)
            {
                PickSelectedOrder();
            }
        }

        /// <summary>
        /// Pick the selected order and assign it to the courier.
        /// </summary>
        private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOrder != null)
            {
                PickSelectedOrder();
            }
        }

        /// <summary>
        /// Refresh the list of available orders.
        /// </summary>
        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => SafeExec(() => LoadOrderList());

        /// <summary>
        /// Close the window.
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        #endregion

        #region Internal Logic

        /// <summary>
        /// Assign the selected order to the courier and close the window.
        /// </summary>
        private void PickSelectedOrder()
        {
            var order = SelectedOrder;
            if (order == null)
            {
                MessageBox.Show("Please select an order.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SafeExec(() =>
            {
                s_bl.Order.OrderSelection(CourierId, CourierId, order.OrderId);
                MessageBox.Show($"Order {order.OrderId} has been assigned to you!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Signal successful order selection to parent window
                this.DialogResult = true;
                this.Close();
            });
        }

        /// <summary>
        /// Load the list of open orders available for this courier.
        /// </summary>
        private void LoadOrderList()
        {
            try
            {
                // Get open orders for this courier (filtered by distance automatically)
                var openOrders = s_bl.Order.GetOpenOrdersForCourier(CourierId, CourierId, null, null).ToList();
                
                // Wrap orders with display properties
                OrderList = openOrders.Select(o => new OrderDisplayWrapper(o)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load courier information (name and max distance).
        /// </summary>
        private void LoadCourierInfo()
        {
            try
            {
                var courier = s_bl.Courier.SearchCourier(CourierId, CourierId);
                CourierInfo = $"Courier: {courier.FullName ?? "Unknown Courier"}";
                MaxDistance = courier.DistanceToDelivery ?? double.MaxValue;
                MaxDistanceDisplay = $"{MaxDistance:F2} km";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courier info: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CourierInfo = "Courier: Unknown";
                MaxDistanceDisplay = "0.00 km";
            }
        }

        /// <summary>
        /// Update IsOrderSelected based on SelectedOrder value.
        /// </summary>
        private void UpdateIsOrderSelected()
        {
            IsOrderSelected = SelectedOrder != null;
        }

        /// <summary>
        /// Observer method triggered by BL when order list changes.
        /// </summary>
        private void orderListObserver()
        {
            // CRITICAL: BL events run on a background thread.
            // We must use Dispatcher to update the UI thread safely.
            this.Dispatcher.Invoke(() =>
            {
                LoadOrderList();
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

            // 2. Load available orders
            LoadOrderList();

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
        /// Wrapper class for OpenOrderInList to provide formatted display properties.
        /// </summary>
        public class OrderDisplayWrapper
        {
            private readonly BO.OpenOrderInList _order;

            public OrderDisplayWrapper(BO.OpenOrderInList order)
            {
                _order = order;
            }

            public int OrderId => _order.OrderId;
            public string FullAddress => _order.FullAddress;
            public double AirDistance => _order.AirDistance;
            public string AirDistanceDisplay => $"{_order.AirDistance:F2}";
            public BO.OrderType OrderType => _order.OrderType;
            public bool IsHeavy => _order.IsHeavy;
            public TimeSpan TimeRemaining => _order.TimeRemaining;
            public string TimeRemainingDisplay => _order.TimeRemaining.ToString(@"hh\:mm\:ss");
            public BO.ScheduleStatus ScheduleStatus => _order.ScheduleStatus;
        }

        #endregion
    }
}

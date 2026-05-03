using BlApi;
using BO;
using PL.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for OrdersToPick.xaml
    /// Displays open orders available for a courier to pickup.
    /// </summary>
    public partial class OrdersToPick : Window, INotifyPropertyChanged
    {
        #region BL Access
        static readonly IBl s_bl = Factory.Get();
        #endregion

        #region Stage 7 - Observer Mutex
        private readonly ObserverMutex _orderListMutex = new(); //stage 7
        #endregion

        #region Properties

        private int CourierId { get; set; }

        public IEnumerable<OrderDisplayWrapper> OrderList
        {
            get { return (IEnumerable<OrderDisplayWrapper>)GetValue(OrderListProperty); }
            set { SetValue(OrderListProperty, value); }
        }
        public static readonly DependencyProperty OrderListProperty =
            DependencyProperty.Register("OrderList", typeof(IEnumerable<OrderDisplayWrapper>), typeof(OrdersToPick));

        public OrderDisplayWrapper? SelectedOrder
        {
            get { return (OrderDisplayWrapper?)GetValue(SelectedOrderProperty); }
            set { SetValue(SelectedOrderProperty, value); }
        }
        public static readonly DependencyProperty SelectedOrderProperty =
            DependencyProperty.Register("SelectedOrder", typeof(OrderDisplayWrapper), typeof(OrdersToPick),
                new PropertyMetadata(null, (d, e) => ((OrdersToPick)d).UpdateIsOrderSelected()));

        public string CourierInfo
        {
            get { return (string)GetValue(CourierInfoProperty); }
            set { SetValue(CourierInfoProperty, value); }
        }
        public static readonly DependencyProperty CourierInfoProperty =
            DependencyProperty.Register("CourierInfo", typeof(string), typeof(OrdersToPick), new PropertyMetadata(""));

        public string MaxDistanceDisplay
        {
            get { return (string)GetValue(MaxDistanceDisplayProperty); }
            set { SetValue(MaxDistanceDisplayProperty, value); }
        }
        public static readonly DependencyProperty MaxDistanceDisplayProperty =
            DependencyProperty.Register("MaxDistanceDisplay", typeof(string), typeof(OrdersToPick),
                new PropertyMetadata("0.00 km"));

        public bool IsOrderSelected
        {
            get { return (bool)GetValue(IsOrderSelectedProperty); }
            set { SetValue(IsOrderSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsOrderSelectedProperty =
            DependencyProperty.Register("IsOrderSelected", typeof(bool), typeof(OrdersToPick), new PropertyMetadata(false));

        // Dependency Property: SortBy (Sort dropdown)
        public string? SortBy
        {
            get { return (string?)GetValue(SortByProperty); }
            set { SetValue(SortByProperty, value); }
        }
        public static readonly DependencyProperty SortByProperty =
            DependencyProperty.Register("SortBy", typeof(string), typeof(OrdersToPick),
                new PropertyMetadata("Time Remaining", (d, e) => ((OrdersToPick)d).LoadOrderList()));

        #endregion

        #region Constructor

        public OrdersToPick(int courierId)
        {
            InitializeComponent();
            CourierId = courierId;
            LoadOrderList();
            s_bl.Order.AddObserver(orderListObserver);
            this.Closed += Window_Closed;
        }

        #endregion

        #region UI Event Handlers

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedOrder != null) PickSelectedOrder();
        }

        private void BtnPickOrder_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOrder != null) PickSelectedOrder();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => SafeExec(() => LoadOrderList());

        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        #endregion

        #region Internal Logic

        /// <summary>
        /// Assigns the selected order to the courier.
        /// Updated for Async execution (Stage 7).
        /// </summary>
        private async void PickSelectedOrder()
        {
            var order = SelectedOrder;
            if (order == null) return;

            try
            {
                // Change to Stage 7: Asynchronous call with await
                // This will allow the screen to stay alive while the BL calculates a route to the server
                await Task.Run(() => s_bl.Order.OrderSelectionAsync(CourierId, CourierId, order.OrderId));

                MessageBox.Show($"Order {order.OrderId} has been assigned to you!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

                // Close the window after successful selection
                this.Close();
            }
            catch (BO.BlNotNullableException ex) when (ex.Message.Contains("Simulator is running"))
            {
                // Stage 7: Operation blocked by simulator
                MessageBox.Show("Operation not allowed while simulator is running.\nPlease stop the simulator first.", 
                    "Simulator Active", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads the list of available orders with sorting.
        /// </summary>
        private void LoadOrderList()
        {
            try
            {
                var openOrders = s_bl.Order.GetOpenOrdersForCourier(CourierId, CourierId, null, null).ToList();
                
                // Apply sorting based on selected criteria
                var sortedOrders = SortBy switch
                {
                    "Air Distance" => openOrders.OrderBy(o => o.AirDistance),
                    "Order ID" => openOrders.OrderBy(o => o.OrderId),
                    "Order Type" => openOrders.OrderBy(o => o.OrderType),
                    "Schedule Status" => openOrders.OrderBy(o => o.ScheduleStatus),
                    _ => openOrders.OrderBy(o => o.TimeRemaining) // Default: Time Remaining
                };
                
                OrderList = sortedOrders.Select(o => new OrderDisplayWrapper(o)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads courier details and sets max distance display.
        /// </summary>
        private void LoadCourierInfo()
        {
            try
            {
                var courier = s_bl.Courier.SearchCourier(CourierId, CourierId);
                CourierInfo = $"Courier: {courier.FullName ?? "Unknown Courier"}";

                double dist = courier.DistanceToDelivery ?? double.MaxValue;

                // Handle logic for displaying "Unlimited" instead of large numbers
                if (courier.DistanceToDelivery == null || dist > 40000)
                {
                    MaxDistanceDisplay = "Unlimited";
                }
                else
                {
                    MaxDistanceDisplay = $"{dist:F2} km";
                }
            }
            catch
            {
                CourierInfo = "Courier: Unknown";
                MaxDistanceDisplay = "N/A";
            }
        }

        private void UpdateIsOrderSelected() => IsOrderSelected = SelectedOrder != null;

        /// <summary>
        /// Observer method triggered by BL when order list changes.
        /// Implements Stage 7 multithreading support with ObserverMutex.
        /// </summary>
        private void orderListObserver()
        {
            #region Stage 7 (for multithreading)
            if (_orderListMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Dispatcher.BeginInvoke(async () =>
            {
                // The actual work to be done on the UI thread
                LoadOrderList();

                // After completing the work, check if a restart was requested
                if (await _orderListMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    orderListObserver();
            });
            #endregion Stage 7 (for multithreading)
        }

        #endregion

        #region Lifecycle Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCourierInfo();
            LoadOrderList();
            s_bl.Order.AddObserver(orderListObserver);
        }

        private void Window_Closed(object? sender, EventArgs e) =>
            s_bl.Order.RemoveObserver(orderListObserver);

        #endregion

        #region Helpers

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SafeExec(Action action)
        {
            try { action(); }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public class OrderDisplayWrapper
        {
            private readonly BO.OpenOrderInList _order;
            public OrderDisplayWrapper(BO.OpenOrderInList order) { _order = order; }
            public int OrderId => _order.OrderId;
            public string FullAddress => _order.FullAddress;
            public double AirDistance => _order.AirDistance;
            public string AirDistanceDisplay => $"{_order.AirDistance:F2}";
            public BO.OrderType OrderType => _order.OrderType;
            public bool IsHeavy => _order.IsHeavy;
            public TimeSpan TimeRemaining => _order.TimeRemaining;
            public string TimeRemainingDisplay
            {
                get
                {
                    // if is more than 24 hours
                    if (_order.TimeRemaining.TotalDays >= 1)
                        return "Over one day";
                    // else
                    return _order.TimeRemaining.ToString(@"hh\:mm\:ss");
                }
            }
            public string TravelTimeDisplay => _order.ActualTimeEstimation?.ToString(@"hh\:mm\:ss") ?? "-";
            public BO.ScheduleStatus ScheduleStatus => _order.ScheduleStatus;
        }

        #endregion
    }
}
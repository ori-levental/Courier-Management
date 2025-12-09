using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for OrderInListWindow.xaml
    /// </summary>
    public partial class OrderInListWindow : Window
    {
        #region BL Access
        // Access point to the Business Logic layer
        static readonly IBl s_bl = Factory.Get();
        #endregion

        #region Properties

        // Data source for the ComboBox options. 
        // Must be a collection (IEnumerable), not a single value.
        public IEnumerable<BO.OrderInListEnum> SortOptions { get; set; }

        // --- Dependency Property: OrderList ---
        // Bound to the ListView in XAML to display the data.
        public IEnumerable<BO.OrderInList> OrderList
        {
            get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
            set { SetValue(OrderListProperty, value); }
        }

        public static readonly DependencyProperty OrderListProperty =
            DependencyProperty.Register("OrderList", typeof(IEnumerable<BO.OrderInList>), typeof(OrderInListWindow));

        // --- Dependency Property: CurrentSort ---
        // Bound to the ComboBox SelectedItem to track the user's choice.
        public BO.OrderInListEnum CurrentSort
        {
            get { return (BO.OrderInListEnum)GetValue(CurrentSortProperty); }
            set { SetValue(CurrentSortProperty, value); }
        }

        public static readonly DependencyProperty CurrentSortProperty =
            DependencyProperty.Register("CurrentSort", typeof(BO.OrderInListEnum), typeof(OrderInListWindow),
                new PropertyMetadata(BO.OrderInListEnum.OrderId)); // Set default sort to OrderId

        // --- Dependency Property: OrderStatusFilter (The Real Data) ---
        // Logic: null = All, Provided = Completed, Cancelled = Cancelled, etc.
        public BO.ShipmentCompletionStatus? OrderStatusFilter
        {
            get { return (BO.ShipmentCompletionStatus?)GetValue(OrderStatusFilterProperty); }
            set { SetValue(OrderStatusFilterProperty, value); }
        }

        public static readonly DependencyProperty OrderStatusFilterProperty =
            DependencyProperty.Register("OrderStatusFilter", typeof(BO.ShipmentCompletionStatus?), typeof(OrderInListWindow),
                new PropertyMetadata(null)); // Default is null (Show All)


        // --- Wrapper Property: FilterUI (The Trick) ---
        // Handles the conversion logic between the UI CheckBox state and the Data Logic.
        // Binds to the CheckBox in XAML.
        public bool? FilterUI
        {
            get
            {
                // Convert Data -> UI
                // Data: Provided (Completed) -> UI: True (V)
                // Data: Cancelled           -> UI: Null (X)
                // Data: Null (All)          -> UI: False (Empty)

                if (OrderStatusFilter == ShipmentCompletionStatus.Provided) return true;
                if (OrderStatusFilter == ShipmentCompletionStatus.Cancelled) return null;
                return false;
            }
            set
            {
                // Convert UI -> Data
                // UI: True (V)    -> Data: Provided (Completed)
                // UI: Null (X)    -> Data: Cancelled
                // UI: False (Empty)-> Data: Null (All)

                if (value == true) OrderStatusFilter = ShipmentCompletionStatus.Provided;
                else if (value == null) OrderStatusFilter = ShipmentCompletionStatus.Cancelled;
                else OrderStatusFilter = null;

                // Trigger data refresh immediately
                queryOrderList();
            }
        }

        #endregion

        #region Constructor
        public OrderInListWindow()
        {
            // Populate the SortOptions list from the Enum values before InitializeComponent
            SortOptions = Enum.GetValues(typeof(BO.OrderInListEnum)).Cast<BO.OrderInListEnum>();

            InitializeComponent();

            // Register Lifecycle events manually to ensure they fire
            this.Loaded += Window_Loaded;
            this.Closed += Window_Closed;
        }
        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Triggered when the user changes the sort selection in the ComboBox.
        /// </summary>
        private void CbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => queryOrderList();

        /// <summary>
        /// Placeholder for opening the Add Order window.
        /// </summary>
        private void BtnAddOrder_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Add Order Window - Coming Soon");

        #endregion

        #region Internal Logic & Observers

        /// <summary>
        /// Centralized method to fetch the list from BL based on current filters and sort.
        /// </summary>
        private void queryOrderList()
        {
            try
            {
                // Get the Manager ID from configuration (assuming Manager context)
                int managerId = s_bl.Admin.GetConfig().ManagerId;

                // Fetch the list from BL, passing BOTH the Filter (OrderStatusFilter) and the Sort (CurrentSort)
                OrderList = s_bl.Order.ListOfOrder(managerId, OrderInListEnum.OrderStatus, OrderStatusFilter, CurrentSort);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Observer method triggered by BL when data changes.
        /// </summary>
        private void orderListObserver() =>
            // CRITICAL: BL events run on a background thread.
            // We must use the Dispatcher to update the UI thread safely.
            this.Dispatcher.Invoke(() =>
            {
                queryOrderList();
            });

        #endregion

        #region Lifecycle Events

        /// <summary>
        /// Called when the window is fully loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Load data immediately upon opening
            queryOrderList();

            // 2. Register for updates (Observer Pattern)
            s_bl.Order.AddObserver(orderListObserver);
        }

        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        private void Window_Closed(object? sender, EventArgs e) => s_bl.Order.RemoveObserver(orderListObserver);
        // Unregister the observer to prevent memory leaks and errors

        #endregion
    }
}

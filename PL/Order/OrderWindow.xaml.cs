using BlApi;
using BO;
using PL.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace PL.Order
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : PL.NetworkAwareWindow
    {
        static readonly IBl s_bl = Factory.Get();

        #region Stage 7 - Observer Mutex
        // Mutex to prevent UI thread saturation during simulation
        private readonly ObserverMutex _orderMutex = new();
        #endregion

        // --- Dependency Properties ---

        public BO.Order CurrentOrder
        {
            get { return (BO.Order)GetValue(CurrentOrderProperty); }
            set { SetValue(CurrentOrderProperty, value); }
        }
        public static readonly DependencyProperty CurrentOrderProperty =
            DependencyProperty.Register("CurrentOrder", typeof(BO.Order), typeof(OrderWindow));

        public bool IsAddMode
        {
            get { return (bool)GetValue(IsAddModeProperty); }
            set { SetValue(IsAddModeProperty, value); }
        }
        public static readonly DependencyProperty IsAddModeProperty =
            DependencyProperty.Register("IsAddMode", typeof(bool), typeof(OrderWindow));

        public string ButtonContent
        {
            get { return (string)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }
        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(string), typeof(OrderWindow));


        // --- Constructors ---

        public OrderWindow()
        {
            InitializeComponent();

            IsAddMode = true;
            ButtonContent = "Add Order";

            CurrentOrder = new BO.Order
            {
                Id = 0,
                OrderingName = "",
                PhoneNumber = "",
                FullAddress = "",
                Description = "",
                StartOrderTime = s_bl.Admin.GetClock(),
                OrderType = BO.OrderType.Private,
                DeliveryHistory = new List<DeliveryPerOrderInList>(),
                OrderStatus = OrderStatus.Open,
                MaxArrivalTime = s_bl.Admin.GetClock() + s_bl.Admin.GetConfig().MaxDeliveryTime,
                TimeRemaining = s_bl.Admin.GetConfig().MaxDeliveryTime
            };
        }

        public OrderWindow(int orderId)
        {
            InitializeComponent();

            IsAddMode = false;
            ButtonContent = "Update Order";

            try
            {
                int managerId = s_bl.Admin.GetConfig().ManagerId;
                CurrentOrder = s_bl.Order.OrderDetails(managerId, orderId);

                // Register observer only for existing orders
                this.Loaded += (s, e) => s_bl.Order.AddObserver(orderRefresher);
                this.Closed += (s, e) => s_bl.Order.RemoveObserver(orderRefresher);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        // --- Event Handlers ---

        private async void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentOrder == null) return;
            int managerId = s_bl.Admin.GetConfig().ManagerId;

            // 1. Perform the network operation (using base class helper)
            try
            {
                bool isSuccess = await ExecuteNetworkActionAsync(async () =>
                {
                    try
                    {
                        if (IsAddMode)
                            await s_bl.Order.AddOrderAsync(managerId, CurrentOrder);
                        else
                            await s_bl.Order.UpdateOrderAsync(managerId, CurrentOrder);
                    }
                    catch (BO.BlNotNullableException ex) when (ex.Message.Contains("Simulator is running"))
                    {
                        // Stage 7: Operation blocked by simulator
                        throw new Exception("Operation not allowed while simulator is running.\nPlease stop the simulator first.");
                    }

                }, "Calculating Coordinates...", "Order Processed Successfully");

                // 2. Only if the operation was successful - display a message and close
                if (isSuccess)
                {
                    MessageBox.Show(IsAddMode ? "Order added!" : "Order updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel this order?", "Confirm Cancel",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    int managerId = s_bl.Admin.GetConfig().ManagerId;
                    s_bl.Order.CancelOrder(managerId, CurrentOrder.Id);

                    MessageBox.Show("Order cancelled successfully.");
                    Close();
                }
                catch (BO.BlNotNullableException ex) when (ex.Message.Contains("Simulator is running"))
                {
                    // Stage 7: Operation blocked by simulator
                    MessageBox.Show("Operation not allowed while simulator is running.\nPlease stop the simulator first.",
                        "Simulator Active", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Cancellation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- Observer Logic ---

        /// <summary>
        /// Observer callback invoked by the Business Layer for Order updates.
        /// Refreshes order details when changes are detected in the system.
        /// </summary>
        private void orderRefresher()
        {
            #region Stage 7 (for multithreading)
            // Check if we are already updating to prevent flooding
            if (_orderMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Dispatcher.BeginInvoke(async () =>
            {
                // The actual work to be done on the UI thread
                try
                {
                    // If we are in insert mode, there is nothing to refresh
                    if (IsAddMode) return;

                    int managerId = s_bl.Admin.GetConfig().ManagerId;

                    // Re-fetch the order from the BL
                    var updatedOrder = s_bl.Order.OrderDetails(managerId, CurrentOrder.Id);

                    // Update the displayed object (this will automatically update the screen thanks to Binding)
                    CurrentOrder = updatedOrder;
                }
                catch
                {
                    // If the order was deleted or an error occurred, we will close the window
                    Close();
                }

                // After completing the work, check if a restart was requested (throttling mechanism)
                if (await _orderMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    orderRefresher();
            });
            #endregion Stage 7 (for multithreading)
        }
    }
}
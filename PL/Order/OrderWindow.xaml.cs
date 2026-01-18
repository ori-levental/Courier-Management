using BlApi;
using BO;
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
                OrderStatus = ShipmentCompletionStatus.Open,
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

            // 1. Perform the network operation (without messages and without closing)
            bool isSuccess = await ExecuteNetworkActionAsync(async () =>
            {
                if (IsAddMode)
                    await s_bl.Order.AddOrderAsync(managerId, CurrentOrder);
                else
                    await s_bl.Order.UpdateOrderAsync(managerId, CurrentOrder);

            }, "Calculating Coordinates...", "Order Processed Successfully");

            // 2. Only if the operation was successful (the screen is already green at this point) - display a message and close
            if (isSuccess)
            {
                MessageBox.Show(IsAddMode ? "Order added!" : "Order updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
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
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        private void orderRefresher()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // If we are in insert mode, there is nothing to refresh
                    if (IsAddMode) return;

                    int managerId = s_bl.Admin.GetConfig().ManagerId;

                    // Re-fetch the order from the BL
                    var updatedOrder = s_bl.Order.OrderDetails(managerId, CurrentOrder.Id);

                    // Update the displayed object (this will automatically update the screen thanks to the Binding)
                    CurrentOrder = updatedOrder;
                }
                catch
                {
                    // If the order was deleted or an error occurred, we will close the window
                    Close();
                }
            });
        }
    }
}
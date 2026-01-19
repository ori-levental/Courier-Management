using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // Required for ListCollectionView
using BlApi;

namespace PL.Order
{
    public partial class OrderListWindow : Window
    {
        static readonly IBl s_bl = Factory.Get();

        // --- Properties ---

        public IEnumerable<BO.OrderInListEnum> SortOptions { get; set; }
        public IEnumerable<BO.OrderStatus> StatusOptions { get; set; }

        // NOTE: OrderList is now bound to an IEnumerable, but we will assign a ListCollectionView to it
        public System.Collections.IEnumerable OrderList
        {
            get { return (System.Collections.IEnumerable)GetValue(OrderListProperty); }
            set { SetValue(OrderListProperty, value); }
        }
        public static readonly DependencyProperty OrderListProperty =
            DependencyProperty.Register("OrderList", typeof(System.Collections.IEnumerable), typeof(OrderListWindow));

        public BO.OrderInListEnum CurrentSort
        {
            get { return (BO.OrderInListEnum)GetValue(CurrentSortProperty); }
            set { SetValue(CurrentSortProperty, value); }
        }
        public static readonly DependencyProperty CurrentSortProperty =
            DependencyProperty.Register("CurrentSort", typeof(BO.OrderInListEnum), typeof(OrderListWindow),
                new PropertyMetadata(BO.OrderInListEnum.OrderId));

        public BO.ShipmentCompletionStatus? SelectedStatusFilter
        {
            get { return (BO.ShipmentCompletionStatus?)GetValue(SelectedStatusFilterProperty); }
            set { SetValue(SelectedStatusFilterProperty, value); }
        }
        public static readonly DependencyProperty SelectedStatusFilterProperty =
            DependencyProperty.Register("SelectedStatusFilter", typeof(BO.ShipmentCompletionStatus?), typeof(OrderListWindow), new PropertyMetadata(null));

        // NEW: Grouping Property
        public bool IsGrouped
        {
            get { return (bool)GetValue(IsGroupedProperty); }
            set { SetValue(IsGroupedProperty, value); }
        }
        public static readonly DependencyProperty IsGroupedProperty =
            DependencyProperty.Register("IsGrouped", typeof(bool), typeof(OrderListWindow),
                new PropertyMetadata(false, (d, e) => ((OrderListWindow)d).queryOrderList()));
        // Callback to refresh list when checkbox changes

        // --- Constructor ---
        public OrderListWindow()
        {
            SortOptions = Enum.GetValues(typeof(BO.OrderInListEnum)).Cast<BO.OrderInListEnum>();
            StatusOptions = Enum.GetValues(typeof(BO.OrderStatus)).Cast<BO.OrderStatus>();

            InitializeComponent();
            this.Loaded += Window_Loaded;
            this.Closed += Window_Closed;
        }

        // --- Event Handlers ---

        private void CbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => queryOrderList();
        private void CbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => queryOrderList();

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            SelectedStatusFilter = null;
            queryOrderList();
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e) => MainWindow.SafeExec(() => new OrderWindow().Show());

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = sender as ListViewItem;
            if (row?.Content is BO.OrderInList order)
            {
                try
                {
                    var win = new OrderWindow(order.OrderId);
                    win.Owner = this;
                    win.Show();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BO.OrderInList order)
            {
                if (MessageBox.Show($"Are you sure you want to CANCEL Order {order.OrderId}?", "Cancel Order",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        int managerId = s_bl.Admin.GetConfig().ManagerId;
                        s_bl.Order.CancelOrder(managerId, order.OrderId);
                        queryOrderList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            e.Handled = true;
        }

        // --- Logic ---

        private void queryOrderList()
        {
            try
            {
                int managerId = s_bl.Admin.GetConfig().ManagerId;

                // 1. Prepare Filter Params
                BO.OrderInListEnum? filterBy = null;
                object? filterValue = null;

                if (SelectedStatusFilter != null)
                {
                    filterBy = BO.OrderInListEnum.OrderStatus;
                    filterValue = SelectedStatusFilter;
                }

                // 2. Fetch Data from BL
                var rawList = s_bl.Order.ListOfOrder(managerId, filterBy, filterValue, CurrentSort);

                // 3. Wrap in CollectionView to support Grouping
                // We use ListCollectionView to enable GroupDescriptions
                ListCollectionView view = new ListCollectionView(rawList.ToList());

                // 4. Apply Grouping if CheckBox is Checked
                if (IsGrouped)
                {
                    // Group by 'OrderStatus' property
                    view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BO.OrderInList.OrderStatus)));
                }

                // 5. Update UI
                OrderList = view;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading list: {ex.Message}");
            }
        }

        private void orderListObserver() => this.Dispatcher.Invoke(() => { queryOrderList(); });

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            queryOrderList();
            s_bl.Order.AddObserver(orderListObserver);
        }
        private void Window_Closed(object? sender, EventArgs e) => s_bl.Order.RemoveObserver(orderListObserver);
    }

    /// <summary>
    /// convertot for prosses time
    /// </summary>
    public class DurationToTextConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                if (ts.TotalDays >= 1) return "Over one day";
                return ts.ToString(@"hh\:mm\:ss");
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    }
}
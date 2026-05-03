using System;
using System.Windows;
using BlApi;
using BO;
using PL.Helpers;

namespace PL.Courier
{
    /// <summary>
    /// Interaction logic for CourierWindow.xaml
    /// </summary>
    public partial class CourierWindow : Window
    {
        // Access to BL
        static readonly IBl s_bl = Factory.Get();

        #region Stage 7 - Observer Mutex
        private readonly ObserverMutex _courierMutex = new(); //stage 7
        #endregion

        // --- Dependency Properties ---

        // The Courier object being edited/viewed
        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }
        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow));

        // Flag to determine if we are adding a new courier (True) or updating (False)
        public bool IsAddMode
        {
            get { return (bool)GetValue(IsAddModeProperty); }
            set { SetValue(IsAddModeProperty, value); }
        }
        public static readonly DependencyProperty IsAddModeProperty =
            DependencyProperty.Register("IsAddMode", typeof(bool), typeof(CourierWindow));

        // Text for the main button
        public string ButtonContent
        {
            get { return (string)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }
        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(string), typeof(CourierWindow));


        // --- Constructors ---

        /// <summary>
        /// Constructor for ADDING a new courier
        /// </summary>
        public CourierWindow()
        {
            InitializeComponent();

            IsAddMode = true;
            ButtonContent = "Add Courier";

            // Initialize a new empty courier object
            CurrentCourier = new BO.Courier
            {
                Id = 0,
                FullName = "",
                PhoneNumber = "",
                Email = "",
                Password = "",
                IsActive = true,
                EmploymentStartDate = DateTime.Now
            };
        }

        /// <summary>
        /// Constructor for UPDATING/VIEWING an existing courier
        /// </summary>
        /// <param name="courierId">The ID of the courier to fetch</param>
        public CourierWindow(int courierId)
        {
            InitializeComponent();

            IsAddMode = false;
            ButtonContent = "Update Courier";

            try
            {
                // Fetch full details from BL
                CurrentCourier = s_bl.Courier.SearchCourier(s_bl.Admin.GetConfig().ManagerId, courierId);
                this.Loaded += Window_Loaded;
                this.Closed += Window_Closed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courier: {ex.Message}");
                Close();
            }

        }

        // --- Event Handlers ---

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsAddMode)
                {
                    // Call BL to ADD
                    s_bl.Courier.AddCourier(s_bl.Admin.GetConfig().ManagerId, CurrentCourier);
                    MessageBox.Show($"Courier {CurrentCourier.Id} added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Call BL to UPDATE
                    s_bl.Courier.UpdateCourier(s_bl.Admin.GetConfig().ManagerId, CurrentCourier);
                    MessageBox.Show($"Courier {CurrentCourier.Id} updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Close the window after success
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
                MessageBox.Show($"Operation Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event for Deleting the courier from this screen
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Ask for confirmation
            if (MessageBox.Show($"Are you sure you want to delete courier {CurrentCourier.FullName}?",
                                "Confirm Delete",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    int managerId = s_bl.Admin.GetConfig().ManagerId;

                    // Call BL to delete
                    s_bl.Courier.DeleteCourier(managerId, CurrentCourier.Id);

                    MessageBox.Show("Courier deleted successfully.");
                    Close(); // Close window after deletion
                }
                catch (BO.BlNotNullableException ex) when (ex.Message.Contains("Simulator is running"))
                {
                    // Stage 7: Operation blocked by simulator
                    MessageBox.Show("Operation not allowed while simulator is running.\nPlease stop the simulator first.", 
                        "Simulator Active", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- Observer Logic ---

        /// <summary>
        /// A method that is called automatically when there is any change in the system (Observer)
        /// </summary>
        private void courierObserver()
        {
            #region Stage 7 (for multithreading)
            if (_courierMutex.CheckAndSetLoadInProgressOrRestartRequired())
                return;

            Dispatcher.BeginInvoke(async () =>
            {
                // The actual work to be done on the UI thread
                if (IsAddMode) return; // Nothing to update if we are creating a new messenger that does not yet exist

                try
                {
                    int managerId = s_bl.Admin.GetConfig().ManagerId;
                    // Re-fetch the most up-to-date data from the BL
                    CurrentCourier = s_bl.Courier.SearchCourier(managerId, CurrentCourier.Id);
                }
                catch
                {
                    // If the messenger has been deleted by someone else in the meantime - we will close the window
                    Close();
                }

                // After completing the work, check if a restart was requested
                if (await _courierMutex.UnsetLoadInProgressAndCheckRestartRequested())
                    courierObserver();
            });
            #endregion Stage 7 (for multithreading)
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Register to listen only if we are in edit mode
            if (!IsAddMode)
                s_bl.Courier.AddObserver(courierObserver);
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            // Remove the port listener to prevent memory leaks and errors
            if (!IsAddMode)
                s_bl.Courier.RemoveObserver(courierObserver);
        }
    }
}
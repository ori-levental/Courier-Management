using BlApi;
using BO;
using PL.Courier;
using System;
using System.Windows;
using System.Windows.Input; // Required for Mouse.OverrideCursor

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Access to the Business Layer (BL)
        // Using static initialization as requested. 
        static readonly IBl s_bl = Factory.Get();

        public MainWindow()
        {
            InitializeComponent();

            // Register lifecycle events
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        #region Dependency Properties

        // -------------------------------------------------------------------
        // Dependency Property for System Clock
        // -------------------------------------------------------------------
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        // -------------------------------------------------------------------
        // Dependency Property for Configuration Object
        // -------------------------------------------------------------------
        public BO.Config Configuration
        {
            get { return (BO.Config)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(BO.Config), typeof(MainWindow));

        #endregion

        #region Observers

        // Observer for Clock updates
        private void clockObserver()
        {
            // CRITICAL: BL events run on a background thread. 
            // We must use Dispatcher to update the UI thread.
            this.Dispatcher.Invoke(() =>
            {
                CurrentTime = s_bl.Admin.GetClock();
            });
        }

        // Observer for Configuration updates
        private void configObserver()
        {
            // CRITICAL: Must use Dispatcher for UI updates from background threads.
            this.Dispatcher.Invoke(() =>
            {
                Configuration = s_bl.Admin.GetConfig();
            });
        }

        #endregion

        #region Lifecycle Events

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initial fetch of data
            CurrentTime = s_bl.Admin.GetClock();
            Configuration = s_bl.Admin.GetConfig();

            // Register observers to listen for changes
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        // Fixed signature: Window.Closed uses EventArgs, not RoutedEventArgs
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            // Unregister observers to prevent memory leaks
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        #endregion

        #region Buttons & Logic

        // --- Time Manipulation ---
        // Using SafeExec wrapper to handle potential BL exceptions
        private void BtnAddMinute_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Minute));
        private void BtnAddHour_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Hour));
        private void BtnAddDay_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Day));
        private void BtnAddMonth_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Month));
        private void BtnAddYear_Click(object sender, RoutedEventArgs e) => SafeExec(() => s_bl.Admin.ForwardClock(TimeUnit.Year));

        // --- Database Actions ---

        /// <summary>
        /// Helper method to close all open windows except the current Main Window.
        /// This prevents data inconsistencies when resetting the DB.
        /// </summary>

        private async void BtnInitDB_Click(object sender, RoutedEventArgs e)
        {
            // Confirm user intent
            if (MessageBox.Show("Are you sure you want to Initialize DB? Existing data will be overwritten.",
                                "Confirm Initialization",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // 1. Change cursor to hourglass to indicate work in progress
                    Mouse.OverrideCursor = Cursors.Wait;

                    // 2. close all windows except the main window
                    CloseAllWindows();

                    // 3. Run the heavy BL operation on a background thread
                    // This prevents the UI from freezing while the DB is being built
                    await Task.Run(() => s_bl.Admin.InitializeDB());

                    // 4. Critical Delay: Wait for the file system (XML) to finish writing.
                    // This ensures we don't read stale data from the disk.
                    await Task.Delay(1000);

                    // 5. Force UI Refresh
                    // Fetching a NEW object instance from the BL. 
                    // WPF detects the reference change and updates all bound TextBoxes automatically.
                    Configuration = s_bl.Admin.GetConfig();
                    CurrentTime = s_bl.Admin.GetClock();

                    MessageBox.Show("Database Initialized successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during initialization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // 6. Always restore the default cursor
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private async void BtnResetDB_Click(object sender, RoutedEventArgs e)
        {
            // Critical warning for data loss
            if (MessageBox.Show("RESET DB? All data will be lost forever!",
                                "Critical Warning",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                try
                {
                    // 1. Change cursor to hourglass to indicate work in progress
                    Mouse.OverrideCursor = Cursors.Wait;

                    // 2. close all windows except the main window
                    CloseAllWindows();

                    // 3. Run ResetDB on a background thread
                    await Task.Run(() => s_bl.Admin.ResetDB());

                    // 4. Critical Delay: Wait for XML files to be deleted/recreated
                    await Task.Delay(1000);

                    // 5. Force UI Refresh
                    // Fetching the fresh (empty/default) configuration from BL.
                    Configuration = s_bl.Admin.GetConfig();
                    CurrentTime = s_bl.Admin.GetClock();

                    MessageBox.Show("Database Reset successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during reset: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        // --- Configuration & Navigation ---

        private void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SafeExec(() =>
            {
                // Cast the IConfig object back to BO.Config to update the BL
                s_bl.Admin.SetConfig((BO.Config)Configuration);
                MessageBox.Show("Configuration updated successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
        private void BtnShowList_Click(object sender, RoutedEventArgs e) => SafeExec(() => new CourierListWindow().Show());


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

        private void CloseAllWindows()
        {
            // Create a temporary list to hold windows to close.
            // We cannot close them directly inside the loop because it modifies the collection 
            // we are iterating over, which causes an Exception.
            List<Window> windowsToClose = new List<Window>();

            foreach (Window window in Application.Current.Windows)
            {
                // If the window is not the current main window, mark it for closing
                if (window != this)
                {
                    windowsToClose.Add(window);
                }
            }

            // Close the collected windows
            foreach (var window in windowsToClose)
            {
                window.Close();
            }
        }

        #endregion
    }
}
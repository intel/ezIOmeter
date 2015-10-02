using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

using ezIOmeter_Lib;
using System.Windows.Media.Animation;
using System.Data;
using System.Runtime.InteropServices;

namespace ezIOmeter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static ezIOmeter_Utility ezIOmeter_utility;
        private static IOmeterWrapper io_meter;
        private static DispatcherTimer eventTimmer;
        public static BackgroundWorker workload_background_thread;

        public BindingList<Result> ResultsToDisplay { get; set; }

        public BindingList<DriveData> DrivesToDisplay { get; set; }

        [DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);

        public MainWindow()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                // Attach to the console when running with arguments.
                AttachConsole(-1);
                Console.WriteLine("");

                // Run the console version of the tests.
                RunConsoleTests(args);

                // Don't display the GUI.
                this.Close();
            }
            else
            {
                RunGUITEsts();
            }
        }

        private void RunGUITEsts()
        {
            InitializeComponent();

            this.DataContext = this;

            // Setup utility helper class.
            ezIOmeter_utility = new ezIOmeter_Utility();

            // Display a list of connected drives to the user.
            DrivesToDisplay = new BindingList<DriveData>();
            ezIOmeter_utility.PopulateDrivesToSelect(DrivesToDisplay, SelectedDrive_cmb);

            // Display results of the test to the user.
            ResultsToDisplay = new BindingList<Result>();

            // Setup ezIOmeter
            InitializeEzIOmeter();

            // Setup delay timmer.
            eventTimmer = new DispatcherTimer();
            eventTimmer.Tick += DismissMessage_Timmer;
            eventTimmer.Interval = new TimeSpan(0, 0, 10);
        }

        private void RunConsoleTests(string[] args)
        {
            String current_path = "";
            String iometer_path = "";
            String config_file_path = "";
            String drive_letter = "";
            IOmeterWrapper io_meter;

            // Set the current working directory path.
            current_path = Environment.CurrentDirectory;

            // Check for valid command args.
            if (args.Length == 2 && args[1] == "--help")
            {
                ezIOmeter_utility.PrintArgUseage();
                return;
            }

            for (int count = 0; count < args.Length; count++)
            {
                // Get the user specified drive letter.
                if (args[count].ToUpper() == "/D" || args[count].ToUpper() == "\\D")
                    drive_letter = args[count + 1];
            }

            // Set the IOmeter path.
            iometer_path = current_path + "\\IOmeter";

            // Set the settings.conf file path.
            config_file_path = current_path + "\\settings.conf";

            // Initialize IOmeter tool.         
            io_meter = new IOmeterWrapper(current_path, iometer_path, config_file_path, new IOmeterWrapper.DisplayMessage(DisplayMessage));

            // Load IOmeter Workloads
            if (!io_meter.LoadIOmeterConfigFiles(current_path + "\\IOmeterConfigFiles"))
            {
                PrintConsoleError(String.Format("Unable to load .icf files from: '{0}'", current_path + "\\IOmeterConfigFiles"));
                return;
            }

            // Run the IOmeter Workloads
            io_meter.RunIOmeterTests(drive_letter);

            // Generate a summary result file.
            io_meter.CreateResultsSummaryFile();
        }

        private void SetupWorkloadBackroundWorker()
        {
            workload_background_thread = new BackgroundWorker();
            workload_background_thread.WorkerReportsProgress = true;
            workload_background_thread.WorkerSupportsCancellation = true;
            workload_background_thread.DoWork += Workload_Background_Thread_DoWork;
            workload_background_thread.ProgressChanged += Workload_Background_Thread_ProgressChanged;
            workload_background_thread.RunWorkerCompleted += Workload_Background_Thread_RunWorkerCompleted;            
        }

        private void InitializeEzIOmeter()
        {
            // Initialize IOmeter tool.         
            io_meter = new IOmeterWrapper(ezIOmeter_utility.WorkingDirectory, ezIOmeter_utility.IOmeterPath, ezIOmeter_utility.ConfigFilePath, new IOmeterWrapper.DisplayMessage(DisplayMessage));

            // Load IOmeter Workloads
            if (!io_meter.LoadIOmeterConfigFiles(ezIOmeter_utility.IOmeterConfigFilesPath))
                throw new Exception(String.Format("Unable to load .icf files from: '{0}'", ezIOmeter_utility.IOmeterConfigFilesPath));

            // Setup the bucket values.
            ezIOmeter_utility.SetupBuckets(this.Resources, io_meter);

            // Setup the initial workloads.
            ezIOmeter_utility.SetupWorkloads(ResultsToDisplay, io_meter);
        }

        private void DisplayToastMessage(String message_to_display)
        {
            // If there is an existing message hide it and reset the toast message.
            HideToastMessage();

            // Show the new toast message.
            ToastMessage_tb.Text = message_to_display;
            toast_message_container.Visibility = System.Windows.Visibility.Visible;            
        }

        private void HighlightRunningTest(String test_to_bold)
        {
            for (int count = 0; count < ResultsDisplay_dg.Items.Count; count++)
            {
                if (test_to_bold.Contains(ResultsToDisplay[count].WorkloadName))
                    ResultsDisplay_dg.SelectedValue = ResultsToDisplay[count];
            }
        }

        private void HideToastMessage()
        {
            // Hide the snackbar.
            toast_message_container.Visibility = System.Windows.Visibility.Collapsed;
            ToastMessage_tb.Text = "";
        }

        private void StopIOmeterTests()
        {
            // Reenable Controls
            SelectedDrive_cmb.IsEnabled = true;
            ResultsDisplay_dg.IsHitTestVisible = true;
            ResultsDisplay_dg.Opacity = 1;
            ResultsDisplay_dg.UnselectAll();

            // Unhide Run Tests Button
            RunTests_btn.Visibility = System.Windows.Visibility.Visible;

            // Hide Stop Tests Button
            StopTests_btn.Visibility = System.Windows.Visibility.Collapsed;

            // Hide the progress bar.
            RunningTestProgress_pb.Visibility = System.Windows.Visibility.Collapsed;

            // Reset instruction message.
            Welcome_lbl.Text = "Please select a drive to test and which workloads to run. After slecting a drive and workloads click RUN TESTS to begin testing.";

			// Get the selected drive to run the test(s) on.
			DriveData tempDriveData = (DriveData)SelectedDrive_cmb.SelectedItem;

			// Clean up after the IOmeter test.  
			io_meter.DeleteIOmeterTestFile(tempDriveData.DriveLetter);
		
            HideToastMessage();
        }

        private void PrintConsoleError(String error_message)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error_message);
            Console.ForegroundColor = oldColor;
        }

        private void DisplayMessage(String message)
        {
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }

        void Menu_Clicked(object sender, RoutedEventArgs e)
        {
            NavigationDrawer_nav.Visibility = System.Windows.Visibility.Visible;
            NavigationDrawer_nav.SetDawerWidth(this.Width, AppToolBar_container.ActualHeight);
            NavigationDrawer_nav.OpenDrawer();
        }

        void RunTests_Click(object sender, RoutedEventArgs e)
        {
            // Disable Controls.
            SelectedDrive_cmb.IsEnabled = false;
            ResultsDisplay_dg.IsHitTestVisible = false;
            ResultsDisplay_dg.Opacity = 0.80;
            
            // Hide Run Tests Button
            RunTests_btn.Visibility = System.Windows.Visibility.Collapsed;

            // Show Stop Tests Button
            StopTests_btn.Visibility = System.Windows.Visibility.Visible;

            // Unhide the progress bar.
            RunningTestProgress_pb.Visibility = System.Windows.Visibility.Visible;

            //Get the selected drive to run the test(s) on.
            DriveData tempDriveData = (DriveData)SelectedDrive_cmb.SelectedItem;

            // Clean up after the IOmeter test.  
            io_meter.DeleteIOmeterTestFile(tempDriveData.DriveLetter);

            // Setup mechanism to keep gui responsive.
            SetupWorkloadBackroundWorker();

            // Start running the tests in the background.
            workload_background_thread.RunWorkerAsync(tempDriveData);

            // Display running test message.
            Welcome_lbl.Text = "Running selected workloads. Please press STOP TESTS if you wish to cancel the workloads.";
        }

        void StopTests_Click(object sender, RoutedEventArgs e)
        {
            io_meter.KillIOmeter();

            workload_background_thread.CancelAsync();

            StopIOmeterTests();
        }

        void SelectAll_Clicked(object sender, RoutedEventArgs e)
        {
            for (int count = 0; count < ResultsDisplay_dg.Items.Count; count++)
            {
                // Get the values of the current row result.
                Result tempResult = ResultsDisplay_dg.Items[count] as Result;

                // Select or deselect all workloads.
                CheckBox selectAll_chk = (CheckBox)sender;
                bool selectAll = (bool)selectAll_chk.IsChecked;
                tempResult.RunWorkLoad = selectAll;
            }
        }

        void Drive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Enable running tests.
            RunTests_btn.IsEnabled = true;           
        }

        void Workload_Background_Thread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StopIOmeterTests();
        }

        void Workload_Background_Thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                DisplayToastMessage(String.Format("{0}", (String)e.UserState));
                HighlightRunningTest((String)e.UserState);
                return;
            }

            // Create a summary of the workloads.
            io_meter.CreateResultsSummaryFile();

            // Get the new summary values.
            io_meter.LoadSummaryResults();

            // Show the user the results.
            ezIOmeter_utility.AddSummaryResults(ResultsToDisplay, io_meter);
        }

        void Workload_Background_Thread_DoWork(object sender, DoWorkEventArgs e)
        {
            DriveData tempDriveData = null;
            if (e.Argument != null)
                tempDriveData = (DriveData)e.Argument;
			
            for (int count = 0; count < ResultsToDisplay.Count; count++)
            {
                // Allow the running test(s) to be canceled.
                if (workload_background_thread.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                // Get the selected workload to run.
                Result tempResult = ResultsToDisplay[count];

                // Skip workloads user doesn't want to run.
                if (!tempResult.RunWorkLoad)
                    continue;

                // Display a snackbar progress message.
                if(!workload_background_thread.CancellationPending)
                    workload_background_thread.ReportProgress(0, String.Format("Running workload {0}.", tempResult.WorkloadName));

                // Execute IOmeter test.
                io_meter.RunIOmeterTest(tempDriveData.DriveLetter, tempResult.WorkloadName);

                // Update the results display.
                workload_background_thread.ReportProgress(0);

                int number_of_tests = ezIOmeter_utility.NumberOfTestsToRun(ResultsToDisplay);
                if ((count + 1) != number_of_tests && number_of_tests > 1 && !workload_background_thread.CancellationPending)
                {
                    // Wait (x)sec before running the next workload.
                    String temp_delay_sec = io_meter.UserConfigSettings.GetAppSetting("sleep_between_tests_sec");
                    int delay_test_for_sec = Convert.ToInt32(temp_delay_sec);
                    while (delay_test_for_sec != 0 && !workload_background_thread.CancellationPending)
                    {
                        workload_background_thread.ReportProgress(0, String.Format("Waiting {0} second(s) before running next workload.", delay_test_for_sec.ToString()));
                        System.Threading.Thread.Sleep(1000); // Sleep for one second
                        delay_test_for_sec--;
                    }
                }
            }
        }

        void DismissMessage_Click(object sender, RoutedEventArgs e)
        {
            HideToastMessage();
        }

        void DismissMessage_Timmer(object sender, EventArgs e)
        {
            eventTimmer.Stop();
            HideToastMessage();
        }
    }
}

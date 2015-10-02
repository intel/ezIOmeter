/*
 *Copyright (c) 2015, Intel Corporation All rights reserved.

 *Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

 *1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

 *2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

 *3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

 *THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ezIOmeter_Lib;

namespace ezIOmeter
{
    public class ezIOmeter_Utility
    {
        private String current_path;
        private String iometer_path;
        private String config_file_path;
        private String icf_path;

        public String WorkingDirectory
        {
            get
            {
                if(String.IsNullOrEmpty(current_path))
                    current_path = Environment.CurrentDirectory;

                return current_path;
            }
        }

        public String IOmeterPath
        {
            get
            {
                if (String.IsNullOrEmpty(iometer_path))
                    iometer_path = String.Format("{0}\\IOmeter", WorkingDirectory);

                return iometer_path;
            }
        }

        public String ConfigFilePath
        {
            get
            {
                if (String.IsNullOrEmpty(config_file_path))
                    config_file_path = String.Format("{0}\\settings.conf", WorkingDirectory);

                return config_file_path;
            }
        }

        public String IOmeterConfigFilesPath
        {
            get
            {
                if (String.IsNullOrEmpty(icf_path))
                    icf_path = String.Format("{0}\\IOmeterConfigFiles", WorkingDirectory);

                return icf_path;
            }
        }

        public int NumberOfTestsToRun(BindingList<Result> results_to_display)
        {
            int number_of_tests = 0;

            for (int count = 0; count < results_to_display.Count; count++)
            {
                // Get the selected workload to run.
                Result tempResult = results_to_display[count];

                if (tempResult.RunWorkLoad)
                    number_of_tests++;
            }

            return number_of_tests;
        }

        public void SetupWorkloads(BindingList<Result> results_to_display, IOmeterWrapper io_meter)
        {
            string[] workload_order = io_meter.WorkloadOrder;
            for (int count = 0; count < workload_order.Length; count++)
            {
                // Create a empty result for each workload and in the proper order.
                Result temp_result = new Result();
                temp_result.WorkloadName = workload_order[count];
                temp_result.RunWorkLoad = true;

                results_to_display.Add(temp_result);
            }

            // Show the user the results.
            AddSummaryResults(results_to_display, io_meter);
        }

        public void AddSummaryResults(BindingList<Result> results_to_display, IOmeterWrapper io_meter)
        {
            // Get Workload Results
            List<Result> temp_results = io_meter.WorkloadResults;
            for (int count = 0; count < temp_results.Count; count++)
            {
                Result temp_result = GetResult(temp_results[count].WorkloadName, results_to_display);
                if (temp_result != null)
                {
                    // Update IOps data.
                    temp_result.MeanIOps = temp_results[count].MeanIOps;
                    temp_result.StdevIOps = temp_results[count].StdevIOps;
                    temp_result.MinIOps = temp_results[count].MinIOps;
                    temp_result.MaxIOps = temp_results[count].MaxIOps;
                    // Update MBps data.
                    temp_result.MeanMBps = temp_results[count].MeanMBps;
                    temp_result.StdevMBps = temp_results[count].StdevMBps;
                    temp_result.MinMBps = temp_results[count].MinMBps;
                    temp_result.MaxMBps = temp_results[count].MaxMBps;
                    // Update latency data.
                    temp_result.Latency = temp_results[count].Latency;
					// Update Thread data
					temp_result.Threads = temp_results[count].Threads;
                }
                else
                {
                    // No existing result found so add it to the list.
                    results_to_display.Add(temp_results[count]);
                }
            }
        }

        public void PopulateDrivesToSelect(BindingList<DriveData> drives_to_display, ComboBox selected_drive_cmb)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo tempDriveInfo in allDrives)
            {
                // Only add drives to the list that are ready to be used.
                if (tempDriveInfo.IsReady)
                {
                    DriveData tempDriveData = new DriveData();
                    tempDriveData.DriveLetter = tempDriveInfo.Name.Replace('\\', ' ').Trim();
                    tempDriveData.DriveLabel = tempDriveInfo.VolumeLabel;
                    tempDriveData.DriveFreeSpace = (tempDriveInfo.TotalFreeSpace / 1048576).ToString(); // Convert byte to Megabyte

                    drives_to_display.Add(tempDriveData);
                }
            }

            selected_drive_cmb.ItemsSource = drives_to_display;
        }

        public void SetupBuckets(ResourceDictionary resource_dictonary, IOmeterWrapper io_meter)
        {
            RangeConverter tempRangeConverter;

            // Setup the Mean IOps Buckets.
            tempRangeConverter = (RangeConverter)resource_dictonary["meanIOps_converter"];
            tempRangeConverter.Bucket0 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("mean_iops_bucket0"));
            tempRangeConverter.Bucket1 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("mean_iops_bucket1"));
            if (io_meter.UserConfigSettings.GetAppSetting("mean_iops_bucket_order") == "backward")
                tempRangeConverter.ReverseOrder = true;

            // Setup the Stdev IOps Buckets.
            tempRangeConverter = (RangeConverter)resource_dictonary["stdevIOps_converter"];
            tempRangeConverter.Bucket0 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("stdev_iops_bucket0"));
            tempRangeConverter.Bucket1 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("stdev_iops_bucket1"));
            if (io_meter.UserConfigSettings.GetAppSetting("stdev_iops_bucket_order") == "backward")
                tempRangeConverter.ReverseOrder = true;

            // Setup the Min IOps Buckets.
            tempRangeConverter = (RangeConverter)resource_dictonary["minIOps_converter"];
            tempRangeConverter.Bucket0 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("min_iops_bucket0"));
            tempRangeConverter.Bucket1 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("min_iops_bucket1"));
            if (io_meter.UserConfigSettings.GetAppSetting("min_iops_bucket_order") == "backward")
                tempRangeConverter.ReverseOrder = true;

            // Setup the Max IOps Buckets.
            tempRangeConverter = (RangeConverter)resource_dictonary["maxIOps_converter"];
            tempRangeConverter.Bucket0 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("max_iops_bucket0"));
            tempRangeConverter.Bucket1 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("max_iops_bucket1"));
            if (io_meter.UserConfigSettings.GetAppSetting("max_iops_bucket_order") == "backward")
                tempRangeConverter.ReverseOrder = true;

            // Setup the MeanMBps Buckets.
            tempRangeConverter = (RangeConverter)resource_dictonary["meanMBps_converter"];
            tempRangeConverter.Bucket0 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("mean_mbps_bucket0"));
            tempRangeConverter.Bucket1 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("mean_mbps_bucket1"));
            if (io_meter.UserConfigSettings.GetAppSetting("mean_mbps_bucket_order") == "backward")
                tempRangeConverter.ReverseOrder = true;

            // Setup the Stdev IOps Buckets.
            tempRangeConverter = (RangeConverter)resource_dictonary["stdevMBps_converter"];
            tempRangeConverter.Bucket0 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("stdev_mbps_bucket0"));
            tempRangeConverter.Bucket1 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("stdev_mbps_bucket1"));
            if (io_meter.UserConfigSettings.GetAppSetting("stdev_mbps_bucket_order") == "backward")
                tempRangeConverter.ReverseOrder = true;

            // Setup the Min IOps Buckets.
            tempRangeConverter = (RangeConverter)resource_dictonary["minMBps_converter"];
            tempRangeConverter.Bucket0 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("min_mbps_bucket0"));
            tempRangeConverter.Bucket1 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("min_mbps_bucket1"));
            if (io_meter.UserConfigSettings.GetAppSetting("min_mbps_bucket_order") == "backward")
                tempRangeConverter.ReverseOrder = true;

            // Setup the Max IOps Buckets.
            tempRangeConverter = (RangeConverter)resource_dictonary["maxMBps_converter"];
            tempRangeConverter.Bucket0 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("max_mbps_bucket0"));
            tempRangeConverter.Bucket1 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("max_mbps_bucket1"));
            if (io_meter.UserConfigSettings.GetAppSetting("max_mbps_bucket_order") == "backward")
                tempRangeConverter.ReverseOrder = true;

            // Setup the Latency Buckets.
            tempRangeConverter = (RangeConverter)resource_dictonary["latency_converter"];
            tempRangeConverter.Bucket0 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("latency_bucket0"));
            tempRangeConverter.Bucket1 = Convert.ToDouble(io_meter.UserConfigSettings.GetAppSetting("latency_bucekt1"));
            if (io_meter.UserConfigSettings.GetAppSetting("latency_bucket_order") == "backward")
                tempRangeConverter.ReverseOrder = true;
        }

        public void PrintArgUseage()
        {
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("ezIOmeter allows command line access to automate, the collection of IO performance metrics.");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("");
            Console.WriteLine("Switches:");
            Console.WriteLine("/D - The drive letter to run the test on.");
            Console.WriteLine("/P - Write/Read pseudo.");
            Console.WriteLine("");
            Console.WriteLine("Example:");
            Console.WriteLine("ezIOmeter.exe /D d");
            Console.WriteLine("");
        }

        private Result GetResult(String result_to_get, BindingList<Result> results_to_display)
        {
            Result tempResult = null;

            for (int result_count = 0; result_count < results_to_display.Count; result_count++)
            {
                if (results_to_display[result_count].WorkloadName == result_to_get)
                {
                    // Found an existing result.
                    tempResult = results_to_display[result_count];
                    return tempResult;
                }
            }

            // If no results were found return null.
            return tempResult;
        }
    }
}

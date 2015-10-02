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
using System.Diagnostics;
using MathNet.Numerics.Statistics;
using System.Globalization;

namespace ezIOmeter_Lib
{
    public class IOmeterWrapper
    {
        public delegate void DisplayMessage(String message);
        private event DisplayMessage display_message;

        private List<Result> workload_results;
        private ConfigParser user_config_settings;
        private String current_path;
        private String io_meter_path;
        private String test_duration_sec;
        private string[] config_files;
        private string[] workload_order;
        private Process command_to_execute;

        public string[] WorkloadOrder
        {
            get { return workload_order; }
        }

        public ConfigParser UserConfigSettings
        {
            get { return user_config_settings; }
        }

        public List<Result> WorkloadResults
        {
            get { return workload_results; }
        }
        
        public IOmeterWrapper(String current_path_p, String io_meter_path_p, String config_file_path_p, DisplayMessage display_message_p)
        {
            // Store the passed in message handler.
            display_message += display_message_p;

            // Load the user specified settings.
            user_config_settings = new ConfigParser();
            if (!user_config_settings.LoadAppSettings(config_file_path_p))
                display_message(user_config_settings.GetConfigFileUseage());

            // Store the current application path.
            current_path = current_path_p;

            // Store the path to IOmeter.exe. 
            io_meter_path = io_meter_path_p;

            // Store the worload exicution order.
            workload_order = user_config_settings.GetAppSetting("workload_run_order").Split(',');

            // Get the user's specified test runtime.
            test_duration_sec = user_config_settings.GetAppSetting("workload_duration_sec");

            // Setup results placeholder.
            workload_results = new List<Result>();

            // Store a handle to the running iomter process.
            command_to_execute = new Process();
        }

        public bool LoadIOmeterConfigFiles(String config_files_path_p)
        {
            display_message("Loading configuration setting...");

            // Get the list of icf files to run.
            config_files = Directory.GetFiles(config_files_path_p, "*.icf");
            
            if (config_files.Length > 0)
                return true;

            return false;
        }

        public void RunIOmeterTest(String drive_letter, String workload_to_run)
        {
            for (int count = 0; count < config_files.Length; count++)
            {
                String file_path = config_files[count];

                // Only run the tests we specified.
                if (!file_path.Contains(workload_to_run))
                    continue;

                // Get the file name from the file path.
                String file_name = Path.GetFileNameWithoutExtension(file_path);

                display_message(String.Format("Running workload {0}...", workload_to_run));

                ExecuteIOmeterTest(file_path, drive_letter, file_name);
            }
        }

        public void RunIOmeterTests(String drive_letter)
        {
            for (int order_count = 0; order_count < workload_order.Length; order_count++)
            { 
                for (int count = 0; count < config_files.Length; count++)
                {                   
                    // Get the .icf file path.
                    String file_path = config_files[count];

                    // Get the file name from the file path.
                    String file_name = Path.GetFileNameWithoutExtension(file_path);

                    // Check to see if we have the right workload to run.
                    if (workload_order[order_count] != file_name)
                        continue;

                    display_message(String.Format("Running workload {0} of {1}...", (order_count + 1), workload_order.Length));

                    ExecuteIOmeterTest(file_path, drive_letter, file_name);

                    if (order_count != workload_order.Length - 1)
                    {
                        // Wait (x)sec before running the next workload.
                        String delay_sec = user_config_settings.GetAppSetting("sleep_between_tests_sec");
                        display_message(String.Format("Waiting {0} second(s) before running next workload.", delay_sec));
                        System.Threading.Thread.Sleep(Convert.ToInt32(delay_sec) * 1000); // Cnovert seconds to miliseconds.
                    }
                }
            }

            DeleteIOmeterTestFile(drive_letter);
        }

        public void CreateResultsSummaryFile()
        {
            // Get the path to the result files.
            String results_path = current_path + "\\Results\\";
			StreamWriter summary_writer;
			String timeStamp = DateTime.Now.ToString(new CultureInfo("en-US"));
            List<Workload> workloads_results = ProcessResultFiles(results_path);
			
			//Write the file header only once.
			if (File.Exists(results_path + "summary.csv"))
			{
				// Append to the file
				 summary_writer = new StreamWriter(results_path + "summary.csv", true);
			}
			else
			{
				// Create the file and write the header.
				summary_writer = new StreamWriter(results_path + "summary.csv");
				summary_writer.WriteLine("Workload,Mean IOps,Stdev IOps,Min IOps,Max IOps,Mean MBps,Stdev MBps,Min MBps,Max MBps,Latency (Avg Response),Threads Used, Timestamp");
			}

            for (int order_count = 0; order_count < workload_order.Length; order_count++)
            {
                for(int count = 0; count < workloads_results.Count; count++)
                {
                    Workload temp_workload = workloads_results[count];
                    if (temp_workload.file_name == workload_order[order_count])
                    {
                        String temp_line = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", temp_workload.file_name, temp_workload.meanIOps, temp_workload.stdevIOps, 
															temp_workload.minIOps, temp_workload.maxIOps, temp_workload.meanMBps, temp_workload.stdevMBps, 
															temp_workload.minMBps, temp_workload.maxMBps,temp_workload.avgResponseTime, temp_workload.threadsUsed,timeStamp);
                        summary_writer.WriteLine(temp_line);
                    }
                }
            }

            summary_writer.Flush();
            summary_writer.Close();

            // Show the user the locatoin of the summary data.
            display_message(String.Format("The summary file can be located at '{0}'.", current_path + "\\Results\\summary.csv"));
        }

        public int NumberOfLogicalProcessors()
        {
            int core_count = 0;
            core_count = Environment.ProcessorCount;
            return core_count;
        }

        public bool ExecuteCommand(String command, String arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(command, arguments);
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas";

            command_to_execute = new Process();
            command_to_execute.StartInfo = startInfo;
            command_to_execute.Start();
            command_to_execute.WaitForExit();

            if (command_to_execute.ExitCode != 0)
                return false;

            return true;
        }

        public void LoadSummaryResults()
        {
            workload_results = new List<Result>();

            // Get the path to the result files.
            String results_path = current_path + "\\Results\\";

            if (File.Exists(results_path + "summary.csv"))
            {
				// Read the last line of summary.csv to update the GUI with the previous test results
				String temp_line = File.ReadLines(results_path + "summary.csv").Last();
				string[] temp_line_parts = temp_line.Split(',');

                // Load Result Values
                Result tempResult = new Result();
                tempResult.RunWorkLoad = true;
                tempResult.WorkloadName = temp_line_parts[(int)Result.ResultHeader.Workload];
                tempResult.MeanIOps = System.Convert.ToDouble(temp_line_parts[(int)Result.ResultHeader.Mean_IOps]);
                tempResult.StdevIOps = System.Convert.ToDouble(temp_line_parts[(int)Result.ResultHeader.Stdev_IOps]);
                tempResult.MinIOps = System.Convert.ToDouble(temp_line_parts[(int)Result.ResultHeader.Min_IOps]);
                tempResult.MaxIOps = System.Convert.ToDouble(temp_line_parts[(int)Result.ResultHeader.Max_IOps]);
                tempResult.MeanMBps = System.Convert.ToDouble(temp_line_parts[(int)Result.ResultHeader.Mean_MBps]);
                tempResult.StdevMBps = System.Convert.ToDouble(temp_line_parts[(int)Result.ResultHeader.Stdev_MBps]);
                tempResult.MinMBps = System.Convert.ToDouble(temp_line_parts[(int)Result.ResultHeader.Min_MBps]);
                tempResult.MaxMBps = System.Convert.ToDouble(temp_line_parts[(int)Result.ResultHeader.Max_MBps]);
                tempResult.Latency = System.Convert.ToDouble(temp_line_parts[(int)Result.ResultHeader.Latency]);
				tempResult.Threads = System.Convert.ToInt16(temp_line_parts[(int)Result.ResultHeader.Threads]);

                workload_results.Add(tempResult);
            }
        }

        public void DeleteIOmeterTestFile(String drive_letter)
        {
            // Delete auto generated IOmeter test file.
            CleanUpIOmeterFile(drive_letter + "\\iobw.tst");
        }

        public void KillIOmeter()
        {
            if(!command_to_execute.HasExited)
                command_to_execute.Kill();
        }

        private void ExecuteIOmeterTest(String file_path, String drive_letter, String file_name)
        {
            // Get temp .icf path.
            String temp_io_meter_config_file = current_path + "\\temp.icf";

            // Get results path.
            String results_path = current_path + "\\Results\\" + file_name + ".csv";

            // Generate a temp icf.
            CreateTempIOmeterConfigFile(file_path, temp_io_meter_config_file, drive_letter, test_duration_sec);
			
            // Execute IOmeter test.
            try
            {
                // Bug: Changed IOmeter to user bars instead of guage.
                String command = io_meter_path + "\\IOmeter.exe";
                String args = String.Format("/c \"{0}\" /r \"{1}\" /t 10 /m 0 /S", temp_io_meter_config_file, results_path);
                ExecuteCommand(command, args);
            }
            catch (Exception e)
            {
                Debug.WriteLine("IOmeter.exe Error:" + e.Message);
            }
        }

        private List<Workload> ProcessResultFiles(String results_path)
        {
            List<Workload> temp_workloads_data = new List<Workload>();

            string[] result_files = Directory.GetFiles(results_path, "inst*.csv");
            for (int order_count = 0; order_count < workload_order.Length; order_count++)
            {
                for (int count = 0; count < result_files.Length; count++)
                {
                    // Get the .csv file path.
                    String file_path = result_files[count];

                    // Get the file name from the file path.
                    String file_name = Path.GetFileNameWithoutExtension(file_path);

                    // Check to see if we have the right workload to run.
                    String temp_file_name = "inst" + workload_order[order_count];
                    if (file_name != temp_file_name)
                        continue;

                    int worker_num = 0;
					int maxWorkerNum = 1;
                    bool is_first_entry = true;
                    double intervalSumIOps = 0;
                    double intervalSumMBps = 0;
                    List<double> IOps = new List<double>();
                    List<double> MBps = new List<double>();
                    List<double> avgResponseTime = new List<double>();
                    DescriptiveStatistics statsIOps = new DescriptiveStatistics(IOps, true);
                    DescriptiveStatistics statsMBps = new DescriptiveStatistics(MBps, true);
                    DescriptiveStatistics statsAvgRespTime = new DescriptiveStatistics(avgResponseTime, true);


                    Workload temp_workload = new Workload();
                    temp_workload.orderNum = order_count;
                    temp_workload.file_name = workload_order[order_count];

                    // Process the IOmeter generated result files one line at time.
                    StreamReader workload_file_reader = new StreamReader(file_path);
                    while (!workload_file_reader.EndOfStream)
                    {
                        String temp_line = workload_file_reader.ReadLine();
                        string[] temp_line_parts = temp_line.Split(',');

                        // Check to see if we are at a data row.
                        if (temp_line_parts.Length < 3)
                            continue;

                        // Calculate Workers
                        if (temp_line_parts[(int)Workload.ColumnHeader.TargetType] == "WORKER" && temp_line_parts[(int)Workload.ColumnHeader.TargetName].Contains("Worker"))
                        {
                            // Get the current IOps and MBps measurements.
                            double temp_IOps = Convert.ToDouble(temp_line_parts[(int)Workload.ColumnHeader.IOps]);
                            double temp_MBps = Convert.ToDouble(temp_line_parts[(int)Workload.ColumnHeader.MBps]);
                            double temp_latency = Convert.ToDouble(temp_line_parts[(int)Workload.ColumnHeader.AvgRespTime]);

                            // Store the running sum for the current interval.
                            intervalSumIOps += temp_IOps;
                            intervalSumMBps += temp_MBps;

                            // Check to see if we need to skip the first row worker number.
                            if (worker_num == 0 && is_first_entry)
                                is_first_entry = false;
                            else
                                worker_num = Convert.ToInt32(temp_line_parts[(int)Workload.ColumnHeader.TargetName].Split(' ')[1]);

                            // Check to see if the next value is the start of the next interval.
                            if (worker_num == 1)
                            {
                                // Store the completed interval sum.
                                IOps.Add(intervalSumIOps);
                                MBps.Add(intervalSumMBps);

                                // Reset sum for next interval.
                                intervalSumIOps = 0;
                                intervalSumMBps = 0;
                            }                            

							// Find the number of threads used per workload
							if(maxWorkerNum < worker_num)
							{
								maxWorkerNum = worker_num;
							}

                            // Store the average response time.
                            avgResponseTime.Add(temp_latency);
                        }
                    }

                    workload_file_reader.Close();

                    // Remove IOmeter generated result files.
                    CleanUpIOmeterFile(file_path);
                    CleanUpIOmeterFile(file_path.Replace(temp_file_name, workload_order[order_count]));

                    statsIOps = new DescriptiveStatistics(IOps, true);
                    statsMBps = new DescriptiveStatistics(MBps, true);
                    statsAvgRespTime = new DescriptiveStatistics(avgResponseTime, true);

                    // Calculate Average IOps.
                    if (!Double.IsNaN(statsIOps.Mean))
                        temp_workload.meanIOps = statsIOps.Mean;

                    // Calculate IOps Standard Deviation.
                    if (!Double.IsNaN(statsIOps.StandardDeviation))
                        temp_workload.stdevIOps = statsIOps.StandardDeviation;

                    // Calculate max and min MBps. 
                    if (!Double.IsNaN(statsIOps.Minimum))
                        temp_workload.minIOps = statsIOps.Minimum;
                    if (!Double.IsNaN(statsIOps.Maximum))
                        temp_workload.maxIOps = statsIOps.Maximum;

                    // Calculate Average MBps.
                    if (!Double.IsNaN(statsMBps.Mean))
                        temp_workload.meanMBps = statsMBps.Mean;

                    // Calculate MBps Standard Deviation.
                    if (!Double.IsNaN(statsMBps.StandardDeviation))
                        temp_workload.stdevMBps = statsMBps.StandardDeviation;

                    // Calculate max and min IOps.
                    if (!Double.IsNaN(statsMBps.Minimum))
                        temp_workload.minMBps = statsMBps.Minimum;
                    if (!Double.IsNaN(statsMBps.Maximum))
                        temp_workload.maxMBps = statsMBps.Maximum;

                    // Calculate the Average Response Time (Latency).
                    if (!Double.IsNaN(statsAvgRespTime.Mean))
                        temp_workload.avgResponseTime = statsAvgRespTime.Mean;

					// Set the number of threads used
					temp_workload.threadsUsed = maxWorkerNum;

                    // Store the filled out workload.
                    temp_workloads_data.Add(temp_workload);
                }
            }

            return temp_workloads_data;
        }

        private void CleanUpIOmeterFile(String result_path)
        {
            if (File.Exists(result_path))
			{
				try
				{
					File.Delete(result_path);
				}
				catch(Exception e)
				{
					Debug.WriteLine("File Operation/Access error: " + e.Message);
				}
			}
                
        }

        private void CreateTempIOmeterConfigFile(String io_meter_config_file, String temp_io_meter_config_file, String drive_letter, String seconds_to_run)
        {
            String temp_file = "";
			int numOfProcessors = NumberOfLogicalProcessors();
			

            StreamReader config_file_stream = new StreamReader(io_meter_config_file);
            while (!config_file_stream.EndOfStream)
            {
                // Get the curret file line.
                String current_line = config_file_stream.ReadLine();

                // Perserve the original line.
                temp_file += current_line + "\n";

                if (current_line == "'Target")
                {
                    // Get the next line in the file.
                    String next_line = config_file_stream.ReadLine();

                    // Save the user specified drive letter.
                    temp_file += "	" + drive_letter.ToUpper() + ":\n";

                    continue;
                }

                if (current_line == "'Run Time")
                {
                    // Skip hours, minutes and seconds line.
                    String next_line = config_file_stream.ReadLine();
                    temp_file += next_line + "\n";

                    // Get the next line in the file.
                    next_line = config_file_stream.ReadLine();

                    // Save the user specified run time.
                    temp_file += "0          0          " + seconds_to_run + "\n";

                    continue;
                }

                if (current_line == "'Default Disk Workers to Spawn")
                {
                    // Get the next line in the file.
                    String next_line = config_file_stream.ReadLine();

                    // Save the number of logical processors to use.
					temp_file += "	" + numOfProcessors + ":\n";

                    continue;
                }
            }

            config_file_stream.Close();

            // Write out the temp icf.
            File.WriteAllText(temp_io_meter_config_file, temp_file);
            
        }
    }
}

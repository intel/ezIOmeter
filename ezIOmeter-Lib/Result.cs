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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ezIOmeter_Lib
{
    public class Result : INotifyPropertyChanged
    {

        public enum ResultHeader
        {
            Workload = 0,
            Mean_IOps = 1,
            Stdev_IOps = 2,
            Min_IOps = 3,
            Max_IOps = 4,
            Mean_MBps = 5,
            Stdev_MBps = 6,
            Min_MBps = 7,
            Max_MBps = 8,
            Latency = 9,
			Threads = 10
        };

        private String workloadName;
        private bool runWorkload;
        private double meanIOps;
        private double stdevIOps;
        private double minIOps;
        private double maxIOps;
        private double meanMBps;
        private double stdevMBps;
        private double minMBps;
        private double maxMBps;
        private double latency;
		private int threads;

        public event PropertyChangedEventHandler PropertyChanged;

        public String WorkloadName
        {
            get { return workloadName; }
            set 
            {
                workloadName = value;

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public bool RunWorkLoad
        {
            get { return runWorkload; }
            set 
            { 
                runWorkload = value;

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public double MeanIOps
        {
            get { return meanIOps; }
            set 
            {
				meanIOps = Math.Round(value, 2);

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public double StdevIOps
        {
            get { return stdevIOps; }
            set 
            {
				stdevIOps = Math.Round(value, 2);

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public double MinIOps
        {
            get { return minIOps; }
            set 
            {
				minIOps = Math.Round(value, 2);

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public double MaxIOps
        {
            get { return maxIOps; }
            set 
            {
				maxIOps = Math.Round(value, 2);

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public double MeanMBps
        {
            get { return meanMBps; }
            set 
            {
				meanMBps = Math.Round(value, 2);

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public double StdevMBps
        {
            get { return stdevMBps; }
            set 
            {
				stdevMBps = Math.Round(value, 2);

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public double MinMBps
        {
            get { return minMBps; }
            set 
            {
				minMBps = Math.Round(value, 2);

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public double MaxMBps
        {
            get { return maxMBps; }
            set 
            {
				maxMBps = Math.Round(value, 2);

                // Property Updated
                NotifyPropertyChanged();
            }
        }

        public double Latency
        {
            get { return latency; }
            set 
            {

				latency = Math.Round(value, 2);

                // Property Updated
                NotifyPropertyChanged();
            }
        }
		
		public int Threads
		{
			get { return threads; }
			set
			{
				threads = value;

				// Property Updated
				NotifyPropertyChanged();
			}
		}

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

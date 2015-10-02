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

namespace ezIOmeter_Lib
{
    public class Workload
    {
        public enum ColumnHeader
        {
            // Column Position for inst results from IOmeter v1.1.0
            TimeStamp = 0,
            TargetType = 1,
            TargetName = 2,
            IOps = 7,
            MBps = 10,
            AvgRespTime = 18
        };

        public String file_name;
        public bool startTimeSet;
        public double meanIOps;
        public double stdevIOps;
        public double minIOps;
        public double maxIOps;
        public double meanMBps;
        public double stdevMBps;
        public double minMBps;
        public double maxMBps;
        public double avgResponseTime;
		public int threadsUsed;
        public int orderNum;         

        public Workload()
        {
            file_name = "";
            startTimeSet = false;
            meanIOps = 0;
            stdevIOps = 0;
            minIOps = 0;
            maxIOps = 0;
            meanMBps = 0;
            stdevMBps = 0;
            minMBps = 0;
            maxMBps = 0;
            avgResponseTime = 0;
            orderNum = 0;
			threadsUsed = 0;
        }
    }
}

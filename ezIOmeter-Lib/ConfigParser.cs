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

namespace ezIOmeter_Lib
{
    public class ConfigParser
    {
        private Dictionary<String, String> app_settings_kvp;

        public ConfigParser()
        {
            app_settings_kvp = new Dictionary<String, String>();
        }

        public String GetAppSetting(String setting_key_p)
        {
            // Get the loaded user setting.
            if(app_settings_kvp.ContainsKey(setting_key_p))
                return app_settings_kvp[setting_key_p];

            return "";
        }

        public bool LoadAppSettings(String app_config_file)
        {
            if (File.Exists(app_config_file))
            {
                StreamReader app_settings_reader = new StreamReader(app_config_file);
                while (!app_settings_reader.EndOfStream)
                {
                    String temp_line = app_settings_reader.ReadLine().Trim();

                    // Skip blank lines and ignore comments in the config file.
                    if (String.IsNullOrEmpty(temp_line) || temp_line[0] == '#')
                        continue;

                    // Only add settings that are in the correct format.
                    string[] temp_records = temp_line.Split('=');
                    if (temp_records.Length == 2)
                    {
                        app_settings_kvp.Add(temp_records[0], temp_records[1].Replace("\"", ""));
                    }
                }

                return true;
            }

            return false;
        }

        public String GetConfigFileUseage()
        {
            String temp_error_message = "Error: settings.conf not found.\n" +
                                        "Example settings.conf: \n" +
                                        "  # Number of seconds each workload is run.\n" +
                                        "  workload_duration_sec=120\n" +
                                        "  # A comma seperatd list to determine what order to run workloads.\n" +
                                        "  # Note: Use same name as .icf but wihtout the extension.\n" +
                                        "  workload_run_order=prefill,4KBrandWQD128,4KBrandRQD128,256KBseqWQD128,256KBseqRQD128\n" +
                                        "  # Number of seconds to wait before preforming the next workload.\n" +
                                        "  sleep_between_tests_sec=300;\n";
            return temp_error_message;
        }
    }
}

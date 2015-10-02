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
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace ezIOmeter
{
    public class RangeConverter : IValueConverter
    {
        public bool ReverseOrder { get; set; }
        public double Bucket0 { get; set; }
        public double Bucket1 { get; set; }

        public RangeConverter()
        {
            ReverseOrder = false;
        }

        public object Convert(object value_to_test, Type type, object paramater, CultureInfo cultureInfo)
        {
            double tempvalue = (double)value_to_test;
            object tempBrushConverter = null;

            if (!ReverseOrder)
            {
                // Default Order
                if (tempvalue <= Bucket0)
                    tempBrushConverter = new BrushConverter().ConvertFromString("#7FFF0000");
                else if (tempvalue > Bucket0 && tempvalue <= Bucket1)
                    tempBrushConverter = new BrushConverter().ConvertFromString("#7FF9E719");
                else if (tempvalue > Bucket1)
                    tempBrushConverter = new BrushConverter().ConvertFromString("#7F00F500");
                else
                    tempBrushConverter = Brushes.White;
            }
            else
            {
                if (tempvalue <= Bucket0)
                    tempBrushConverter = new BrushConverter().ConvertFromString("#7F00F500");
                else if (tempvalue > Bucket0 && tempvalue <= Bucket1)
                    tempBrushConverter = new BrushConverter().ConvertFromString("#7FF9E719");
                else if (tempvalue > Bucket1)
                    tempBrushConverter = new BrushConverter().ConvertFromString("#7FFF0000");
                else
                    tempBrushConverter = Brushes.White;
            }

            return (tempBrushConverter as SolidColorBrush);
        }

        public object ConvertBack(object obj, Type type, object obj2, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }
}

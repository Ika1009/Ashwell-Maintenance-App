﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ashwell_Maintenance.Customs
{
    class LinelessEntry : Entry
    {
        public static BindableProperty CursorColorProperty = BindableProperty.Create(
            nameof(CursorColor), typeof(Color), typeof(LinelessEntry), Colors.Red);

        public Color CursorColor
        {
            get => (Color)GetValue(CursorColorProperty);
            set => SetValue(CursorColorProperty, value);
        }
        static LinelessEntry()
        {

        }
    }
}

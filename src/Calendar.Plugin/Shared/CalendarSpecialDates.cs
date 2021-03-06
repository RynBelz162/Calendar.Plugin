﻿using System.Collections.Generic;
using Xamarin.Forms;

namespace Calendar.Plugin.Shared
{
    public partial class Calendar : ContentView
    {

        #region SpecialDates

        public static readonly BindableProperty SpecialDatesProperty =
            BindableProperty.Create(nameof(SpecialDates), typeof(ICollection<SpecialDate>), typeof(Calendar), new List<SpecialDate>(),
                propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalandarChanges.MaxMin));

        public ICollection<SpecialDate> SpecialDates
        {
            get => (ICollection<SpecialDate>)GetValue(SpecialDatesProperty);
            set => SetValue(SpecialDatesProperty, value);
        }

        #endregion

        public void RaiseSpecialDatesChanged()
        {
            ChangeCalendar(CalandarChanges.MaxMin);
        }

        protected void SetButtonSpecial(CalendarButton button, SpecialDate special)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                button.IsEnabled = special.Selectable;
                button.IsSelected = false;

                button.BackgroundPattern = special.BackgroundPattern;
                button.BackgroundImage = special.BackgroundImage;

                if (special.FontSize.HasValue) button.FontSize = special.FontSize.Value;
                if (special.BorderWidth.HasValue) button.BorderWidth = special.BorderWidth.Value;
                if (special.BackgroundColor.HasValue) button.TintColor = special.BackgroundColor.Value;
                if (special.FontAttributes.HasValue) button.FontAttributes = special.FontAttributes.Value;
                if (!string.IsNullOrEmpty(special.FontFamily)) button.FontFamily = special.FontFamily;

                button.BorderWidth = special.BorderWidth ?? BorderWidth;
                button.TintBorderColor = special.BorderColor ?? BorderColor;
                button.TextColor = special.TextColor ?? DatesTextColor;
            });
        }
    }
}

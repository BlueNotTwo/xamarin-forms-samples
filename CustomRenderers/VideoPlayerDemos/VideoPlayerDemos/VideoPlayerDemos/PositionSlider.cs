using System;
using Xamarin.Forms;



// TODO: Change to MediaHelpers

namespace VideoPlayerDemos
{
    public class PositionSlider : Slider
    {
        public static readonly BindableProperty DurationProperty =
            BindableProperty.Create("Duration", typeof(TimeSpan), typeof(PositionSlider), new TimeSpan(1),
                                    propertyChanged: (bindable, oldValue, newValue) =>
                                    {
                                        PositionSlider slider = (PositionSlider)bindable;
                                        double seconds = ((TimeSpan)newValue).TotalSeconds;
                                        slider.Maximum = seconds <= 0 ? 1 : seconds; 
                                    });

        public TimeSpan Duration
        {
            set { SetValue(DurationProperty, value); }
            get { return (TimeSpan)GetValue(DurationProperty); }
        }

        public static readonly BindableProperty PositionProperty =
            BindableProperty.Create("Position", typeof(TimeSpan), typeof(PositionSlider), new TimeSpan(0),
                                    defaultBindingMode: BindingMode.TwoWay,
                                    propertyChanged: (bindable, oldValue, newValue) =>
                                    {
                                        PositionSlider slider = (PositionSlider)bindable;
                                        double seconds = ((TimeSpan)newValue).TotalSeconds;
                                        slider.Value = seconds;
                                    });

        public TimeSpan Position
        {
            set { SetValue(PositionProperty, value); }
            get { return (TimeSpan)GetValue(PositionProperty); }
        }

        public PositionSlider()
        {
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == Slider.ValueProperty.PropertyName)
                {
                    TimeSpan newPosition = TimeSpan.FromSeconds(Value);

                    if (Math.Abs(newPosition.Seconds - Position.Seconds) / Duration.Seconds > 0.01)
                    {
                        Position = newPosition;
                    }
                }
            };
        }
    }
}

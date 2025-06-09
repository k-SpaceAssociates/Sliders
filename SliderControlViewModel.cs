using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Threading;

namespace Sliders
{
    public partial class SliderControlViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer = new();
        private const int DurationInSeconds = 36;
        private const double MaxValue = 1800;
        private double _stepSize;
        private bool _countingUp = true;

        [ObservableProperty]
        private double sliderValue;

        [ObservableProperty]
        private double followerSliderValue;

        [ObservableProperty]
        private bool follow = true;  // default true

        [ObservableProperty]
        private bool v1803 =false;   // default true

        public SliderControlViewModel()
        {
            _stepSize = MaxValue / (DurationInSeconds / 0.05); // update every 50ms

            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += (s, e) => UpdateSlider();
            _timer.Start();
        }

        private void UpdateSlider()
        {
            // Animate primary slider
            if (_countingUp)
            {
                SliderValue += _stepSize;
                if (SliderValue >= MaxValue)
                {
                    SliderValue = MaxValue;
                    _countingUp = false;
                }
            }
            else
            {
                SliderValue -= _stepSize;
                if (SliderValue <= 0)
                {
                    SliderValue = 0;
                    _countingUp = true;
                }
            }

            // Control follower behavior
            if (Follow && V1803)
            {
                FollowerSliderValue = SliderValue;
            }
        }
    }
}
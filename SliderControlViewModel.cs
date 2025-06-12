using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using kSAAutomationUtils;
using kSAAutomationUtils.ViewModels;
using kSATxtCmdNETSDk;
using log4net;
using log4net.Config;
using Sliders.Properties;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Sliders
{
    public partial class SliderControlViewModel : ObservableObject
    {
 
        private readonly DispatcherTimer _timer = new();
        private const int DurationInSeconds = 36;
        private const double MaxValue =1800;
        private const double CanvasWidth = 740; // Approximate usable width in pixels
        private const double ScaleFactor = CanvasWidth / MaxValue; // ≈ 0.422
        private double _stepSize;
        private bool _countingUp = true;

       
        [ObservableProperty]
        private double sliderMinimum = 0;

        [ObservableProperty]
        private double sliderMaximum = 1800;

        [ObservableProperty]
        private double sliderValue;

        [ObservableProperty]
        private double followerSliderValue;

        [ObservableProperty]
        private double verticalSliderMinimum = 0;

        [ObservableProperty]
        private double verticalSliderMaximum = 100;

        [ObservableProperty]
        private double verticalSliderLeft1;

        [ObservableProperty]
        private double verticalSliderLeft2;

        [ObservableProperty]
        private bool follow = true;  // default true

        [ObservableProperty]
        private bool v1803 =true;   // default true

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

            // Sync follower
            if (Follow && V1803)
            {
                FollowerSliderValue = SliderValue;
            }

            double vmidpoint = (VerticalSliderMinimum + VerticalSliderMaximum) / 2.0;
            double sliderHeight = 80; // as defined in XAML
            double midY = sliderHeight * (vmidpoint - VerticalSliderMinimum) / (VerticalSliderMaximum - VerticalSliderMinimum);


            // Apply scaling to get pixel offset
            VerticalSliderLeft1 = SliderValue * ScaleFactor;
            VerticalSliderLeft2 = FollowerSliderValue * ScaleFactor;

        }

        public double VerticalSliderMidpointTop
        {
            get
            {
                double sliderHeight = 40;
                double topOffset = 25;
                double midpoint = (VerticalSliderMinimum + VerticalSliderMaximum) / 2.0;
                double midY = sliderHeight * (1 - (midpoint - VerticalSliderMinimum) / (VerticalSliderMaximum - VerticalSliderMinimum));
                return topOffset + midY;
            }
        }
    


        public bool OnClosing()
        {
            string confirmMessage = "Are you sure you want to exit?";
            bool close = MessageBox.Show(confirmMessage, "Confirm Exit", MessageBoxButton.YesNo) == MessageBoxResult.Yes;

            if (close)
            {

                // give it a moment to finish up
                Thread.Sleep(100);
            }
            return close;
        }

    }

}
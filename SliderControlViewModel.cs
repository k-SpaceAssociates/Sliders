using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Sliders
{
    public partial class SliderControlViewModel : ObservableObject
    {

        private readonly DispatcherTimer _timer = new();
        public const int DurationInSeconds = 36;
        public const double HMaxValue = 2000;
        public const double VMaxValue = 100;
        private const double CanvasWidth = 740; // Approximate usable width in pixels
        private const double CanvasHeight = 100; // Approximate usable height in pixels
        private const double HScaleFactor = CanvasWidth / HMaxValue; // ≈ 0.422
        private const double VScaleFactor = CanvasWidth / VMaxValue;
        public double _stepSize;
        private bool _countingUp = true;

        ///////////////////////////////////////////////////////////////////////////
        public ObservableCollection<string> CommandsToRun { get; } = new()
        {
            "list",
            "fakeHpos",
            "direction",
            "position",
            //"follow", //Bug sending follow command does not get it only sets.
        };

        public ObservableCollection<string> StageList { get; } = new() { };
        int maxStages = 4;
        public ObservableCollection<string> StagePositions { get; } = new() { };

        [ObservableProperty]
        bool follow = true;

        [ObservableProperty]
        string? direction = "";
        ///////////////////////////////////////////////////////////////////////////

        [ObservableProperty]
        private bool dummy = false; //dummy when true will animate the slider on a timer with fake data on the horizontal sliders and non on veritcal except UI input

        [ObservableProperty]
        private bool fakeStageHoriz = true; //fakeStageHoriz when true will tell the stage controller to fake the horizontal stage positions

        [ObservableProperty]
        private double sliderMinimum = 0;

        [ObservableProperty]
        private double sliderMaximum = 1800;

        //[ObservableProperty]
        //private double sliderValue;
        private double _sliderValue;
        public double SliderValue
        {
            get => _sliderValue;
            set
            {
                if (_sliderValue != value)
                {
                    _sliderValue = value;
                    Debug.WriteLine($"[ViewModel] SliderValue set to {_sliderValue}");
                    OnPropertyChanged(); // or OnPropertyChanged(nameof(SliderValue));
                }
            }
        }

        [ObservableProperty]
        private double followerSliderValue;

        [ObservableProperty]
        private double verticalSliderMinimum = 0;

        [ObservableProperty]
        private double verticalSliderMaximum = 500;

        [ObservableProperty]
        private double verticalSliderLeft1;

        [ObservableProperty]
        private double verticalSliderTop1;

        [ObservableProperty]
        private double verticalSliderLeft2;

        [ObservableProperty]
        private double verticalSliderTop2;

        [ObservableProperty]
        private double verticalStageValue1;

        [ObservableProperty]
        private double verticalStageValue2;

        [ObservableProperty]
        private bool v1803 = true;   // default true

        public SliderControlViewModel()
        {
            if(dummy)
            {
                _stepSize = 100;//Updated to make steps of 100 //HMaxValue / (DurationInSeconds / 0.05); // update every 50ms

                _timer.Interval = TimeSpan.FromMilliseconds(1000); //Slow down the dummy animation to 1 second per step to be more realistic
                _timer.Tick += (s, e) => UpdateSlider();
                _timer.Start();
            }
        }

        double[] result = { 0, 0, 0, 0 };
        public void UpdateSlider()
        {
            if (Dummy)
            {
                // Animate primary slider
                if (_countingUp)
                {
                    SliderValue += _stepSize;
                    if (SliderValue >= HMaxValue)
                    {
                        SliderValue = HMaxValue;
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
                Follow =true;
                if (Follow)
                {
                    FollowerSliderValue = SliderValue;
                }
                string result;
                if(StagePositions!=null && StagePositions.Count > 0)
                {
                    foreach(string stagepos in StagePositions)
                    {
                       result = stagepos;
                    }
                }

                // Apply scaling to get pixel offset
                VerticalSliderLeft1 = SliderValue * HScaleFactor;
                VerticalSliderLeft2 = FollowerSliderValue * HScaleFactor;
                Debug.WriteLine($"SliderValue: {SliderValue}, FollowerSliderValue: {FollowerSliderValue}");
                Debug.WriteLine($"Vertical1: {VerticalSliderLeft1}, Vertical2: {VerticalSliderLeft2}");
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (StagePositions != null && StagePositions.Count == 4 && result[0] != double.Parse(StagePositions[0]))
                    {
                        result[0] = double.Parse(StagePositions[0]); //StageH1
                        SliderValue = result[0];

                        if (Follow)
                        {
                            FollowerSliderValue = SliderValue;
                        }
                        else
                        {
                            result[1] = double.Parse(StagePositions[1]); //StageH2
                            FollowerSliderValue = result[1];
                        }
                        // Apply scaling to get pixel offset
                        VerticalSliderLeft1 = SliderValue * HScaleFactor;
                        VerticalSliderLeft2 = FollowerSliderValue * HScaleFactor;

                        result[2] = double.Parse(StagePositions[2]); //StageV1
                        VerticalStageValue1 = result[2];
                        VerticalSliderTop1 = CanvasHeight - (result[2] - VerticalSliderMinimum);
                        result[3] = double.Parse(StagePositions[3]); //StageV2
                        VerticalStageValue2 = result[3];
                        VerticalSliderTop2 = CanvasHeight - (result[3] - VerticalSliderMinimum);
                        Debug.WriteLine($"SliderValue: {SliderValue}, FollowerSliderValue: {FollowerSliderValue}");
                        Debug.WriteLine($"Vertical1: {VerticalSliderLeft1}, Vertical2: {VerticalSliderLeft2}");
                    }
                });

            }





            double vmidpoint = (VerticalSliderMinimum + VerticalSliderMaximum) / 2.0;
            double sliderHeight = 80; // as defined in XAML
            double midY = sliderHeight * (vmidpoint - VerticalSliderMinimum) / (VerticalSliderMaximum - VerticalSliderMinimum);

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
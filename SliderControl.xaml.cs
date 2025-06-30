using System.Diagnostics;
using System.Windows.Controls;

namespace Sliders
{
    public partial class SliderControl : UserControl
    {
        public SliderControl()
        {
            InitializeComponent();
            //var vm = new SliderControlViewModel();
            //this.Loaded += (s, e) =>
            //{
            //    this.DataContext = vm; // ✅ Set after Loaded
            //};
            //Debug.WriteLine($"[SliderControl] DataContext set to {vm.GetType().Name}");
            //Debug.WriteLine($"[SliderControl] SliderValue = {vm.SliderValue}");
            this.DataContextChanged += (s, e) =>
            {
                if (this.DataContext != null)
                {
                    Debug.WriteLine($"[SliderControl] DataContext set to {this.DataContext.GetType().Name}");

                    if (this.DataContext is SliderControlViewModel vm)
                    {
                        Debug.WriteLine($"[SliderControl] SliderValue = {vm.SliderValue}");
                    }
                }
                else
                {
                    Debug.WriteLine("[SliderControl] DataContext is NULL");
                }
            };
        }
    }
}
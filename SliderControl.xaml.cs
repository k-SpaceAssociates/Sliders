using log4net;
using System.Diagnostics;
using System.Windows.Controls;

namespace Sliders
{
    public partial class SliderControl : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SliderControl));
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
                    log.Debug($"[SliderControl] DataContext set to {this.DataContext.GetType().Name}");

                    if (this.DataContext is SliderControlViewModel vm)
                    {
                        log.Debug($"[SliderControl] SliderValue = {vm.SliderValue}");
                    }
                }
                else
                {
                    log.Debug("[SliderControl] DataContext is NULL");
                }
            };
        }
    }
}
using System.Windows.Controls;

namespace Sliders
{
    public partial class SliderControl : UserControl
    {
        public SliderControl()
        {
            InitializeComponent();
            DataContext = new SliderControlViewModel();
        }
    }
}
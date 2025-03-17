using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calculator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;
        if (scrollViewer != null)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Scroll pe orizontală când utilizatorul ține apăsat Shift
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }

    private void ScrollViewer_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;
        if (scrollViewer != null)
        {
            // Ajustează offset-ul pe orizontală în funcție de mișcare
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.DeltaManipulation.Translation.X);
        }
    }


}
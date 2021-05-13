using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CircuitPro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Zoom canvas
        private readonly ScaleTransform st;
        private readonly double zoomMax = 5;
        private readonly double zoomMin = 0.5;
        private double zoom = 1;

        public MainWindow()
        {
            InitializeComponent();

            // setare scale transform (zoom canvas)
            circuitCanvas.LayoutTransform = st = new ScaleTransform();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // reseteaza element selectat atunci cand click in afara elementelor
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

            if (r.VisualHit.GetType() != typeof(ListBoxItem))
            {
                componentList.UnselectAll();
            }
        }

        private void TreeView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // reseteaza element selectat atunci cand click in afara elementelor
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

            if (r.VisualHit.GetType() != typeof(TreeViewItem))
            {
                TreeViewItem item = (TreeViewItem)circuitTree.SelectedItem;
                if(item != null)
                {
                    item.IsSelected = false;
                }
            }
        }

        private void circuitCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // actualizare zoom
            zoom += e.Delta * 0.001;
            zoom = Math.Max(zoom, zoomMin);
            zoom = Math.Min(zoom, zoomMax);

            // zoom continut
            st.ScaleX = zoom;
            st.ScaleY = zoom;

            // actualizare pozitie continut (scroll in functie de pozitia mouse-ului)
            Point canvasPointPos = e.GetPosition(circuitCanvas);
            Point mousePos = e.GetPosition(canvasScroll);

            // se calculeaza diferenta dintre punctul din canvas (care se misca cand se face zoom) si pozitia mouse-ului
            canvasScroll.ScrollToVerticalOffset(canvasPointPos.Y * zoom - mousePos.Y);
            canvasScroll.ScrollToHorizontalOffset(canvasPointPos.X * zoom - mousePos.X);

            // declarare eveniment ca gestionat
            e.Handled = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using CircuitPro.CircuitModel;

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

            // desenare initiala
            UpdateComponentListView();
            UpdateCircuitView();
        }

        private Circuit GetCircuit()
        {
            return ((App)Application.Current).circuit;
        }

        private void UpdateComponentListView()
        {
            componentList.ItemsSource = GetCircuit().componente.ToList<KeyValuePair<string, Component>>();
        }

        private void UpdateCircuitView()
        {
            // resetare interfata
            circuitCanvas.Children.Clear();
            circuitTree.Items.Clear();

            // redesenare componente
            GetCircuit().Desenare(circuitCanvas);
            circuitTree.Items.Add(GetCircuit().GetComponentTree());
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // activeaza butoanele din toolbar
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

        private void CircuitCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
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

        private void AddComponentBtn_Click(object sender, RoutedEventArgs e)
        {
            // creare dialog
            AddComponentDialog dialog = new AddComponentDialog();
            if (dialog.ShowDialog() == true)
            {
                // preluare date si creare component
                Component c = null;
                Circuit circ = ((App)Application.Current).circuit;
                switch (dialog.Tip)
                {
                    case TipComponent.REZISTENTA:
                        c = new Rezistenta(dialog.Nume, dialog.Valoare);
                        break;

                    case TipComponent.CONDENSATOR:
                        c = new Condensator(dialog.Nume, dialog.Valoare, circ.Frecventa);
                        break;

                    case TipComponent.BOBINA:
                        c = new Bobina(dialog.Nume, dialog.Valoare, circ.Frecventa);
                        break;
                }

                // adaugare component in lista
                circ.AddComponent(c);
                UpdateComponentListView();
            };
        }

        private void RemoveComponentBtn_Click(object sender, RoutedEventArgs e)
        {
            // sterge element selectat (daca exista)
            if (componentList.SelectedItem == null)
                return;

            KeyValuePair<string, Component> sel = (KeyValuePair<string, Component>)componentList.SelectedItem;
            GetCircuit().componente.Remove(sel.Key);
            UpdateComponentListView();
        }

        private void UpdateCircuitBtn_Click(object sender, RoutedEventArgs e)
        {
            // actualizare circuit in functie de input utilizator
            if (DescriereCircutText.Text == "")
                return;

            GetCircuit().SetCircuit(DescriereCircutText.Text);
            UpdateCircuitView();
        }

        private void ResetCircuitBtn_Click(object sender, RoutedEventArgs e)
        {
            // reseteaza circuit
            GetCircuit().SetCircuit("");
            DescriereCircutText.Text = "";
            UpdateCircuitView();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        // Selected item
        Component selected = null;
        string selectedId = "";

        public MainWindow()
        {
            InitializeComponent();

            // setare scale transform (zoom canvas)
            circuitCanvas.LayoutTransform = st = new ScaleTransform();

            // initializare document
            ((App)Application.Current).DocumentNou();
        }


        // ------------ Data operations --------------
        private Circuit GetCircuit()
        {
            return ((App)Application.Current).circuit;
        }

        public void SetSelected(Component c, string id = "null")
        {
            // actualizare selectie
            selected = c;
            selectedId = id;

            // actualizare selectie
            if(c == null)
            {
                ResetSelectionView();
            }
            else
            {
                UpdateSelectionView();
            }
        }

        public void UpdateCircuit()
        {
            ResetSelectionView();
            UpdateComponentListView();
            UpdateCircuitView(true);
        }


        // ------------ Update views ------------------
        private void UpdateComponentListView()
        {
            componentList.ItemsSource = GetCircuit().componente.ToList<KeyValuePair<string, Component>>();
        }

        private void UpdateCircuitView(bool reset = false)
        {
            try
            {
                // setare structura circuit
                if (reset)
                {
                    DescriereCircutText.Text = GetCircuit().Descriere;
                }
                else
                {
                    GetCircuit().Descriere = DescriereCircutText.Text;
                }

                // activare interfata
                circuitTree.IsEnabled = true;
                canvasScroll.IsEnabled = true;
                circuitCanvas.IsEnabled = true;
                circuitCanvas.Opacity = 1;

                // resetare interfata
                circuitCanvas.Children.Clear();
                circuitTree.Items.Clear();

                // actualizare interfata
                GetCircuit().Desenare(circuitCanvas);
                circuitTree.Items.Add(GetCircuit().GetComponentTree());
            }

            // capturare erori
            catch (Exception e)
            {
                // dezactivare interfata
                circuitTree.IsEnabled = false;
                canvasScroll.IsEnabled = false;
                circuitCanvas.IsEnabled = false;
                circuitCanvas.Opacity = 0.5;

                // afisare mesaj eroare
                MessageBox.Show(this, e.Message, "Descriere circuit incorectă", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePropertiesView()
        {
            // reset everything
            PropertiesDenumire.Text = "";
            PropertiesValoare.Text = "";
            PropertiesDefazaj.Text = "";
            PropertiesTensiune.Text = "";
            PropertiesIntensitate.Text = "";

            if (selected == null)
                return;

            // generator
            if (selected.Tip == TipComponent.GENERATOR)
            {
                PropertiesDenumire.Text = selected.Nume;
                PropertiesValoare.Text = $"Frecvență: {((Generator)selected).Frecventa} Hz";
                PropertiesDefazaj.Text = $"Tensiune: {selected.Tensiune} V";
                return;
            }

            // generare informatii
            string denumire = "";
            double valoare = selected.Reactanta;
            string numeValoare = "Impedanță";
            string unitateMasura = "Ω";
            double defazaj = selected.Defazaj * 180.0 / Math.PI;
            switch (selected.Tip)
            {
                case TipComponent.REZISTENTA:
                    denumire = $"Rezistență: ";
                    numeValoare = "Rezistență";
                    break;

                case TipComponent.CONDENSATOR:
                    denumire = $"Condensator:";
                    numeValoare = "Capacitate";
                    valoare = ((Condensator)selected).Capacitate;
                    unitateMasura = "1/π F";
                    break;

                case TipComponent.BOBINA:
                    denumire = $"Bobină: ";
                    numeValoare = "Inductanță";
                    valoare = ((Bobina)selected).Inductanta;
                    unitateMasura = "1/π H";
                    break;
            }
            denumire += selected.Nume;

            // actualizare proprietati
            PropertiesDenumire.Text = denumire;
            PropertiesValoare.Text = $"{numeValoare}: {Math.Round(valoare, 4)} {unitateMasura}";
            PropertiesDefazaj.Text = $"Defazaj: {Math.Round(defazaj, 4)} grade";

            // informatii element circuit (tensiune, intensitate)
            if(selectedId != "null")
            {
                PropertiesTensiune.Text = $"Tensiune: {Math.Round(selected.Tensiune, 4)} V";
                PropertiesIntensitate.Text = $"Intensitate: {Math.Round(selected.Intensitate, 4)} A";
            }
        }

        private void UpdateSelectionView()
        {
            // actualizare proprietati
            UpdatePropertiesView();

            // actualizare selectie lista componente
            if (selectedId != "null")
                componentList.UnselectAll();

            // actualizare selectie tree
            if(circuitTree.Items.Count != 0 && circuitTree.Items[0] != null)
            {
                ((ComponentTreeItem)circuitTree.Items[0]).SetSelected(selectedId);
            }

            // actualizare selectie canvas
            foreach (UIElement child in circuitCanvas.Children)
            {
                if (child is CircuitShapes.ComponentDraw elem)
                {
                    elem.SetSelected(elem.Id.StartsWith(selectedId));
                }
            }
        }

        private void ResetSelectionView()
        {
            // disable selection on component list
            componentList.UnselectAll();

            // disable tree selection
            ComponentTreeItem item = (ComponentTreeItem)circuitTree.SelectedItem;
            if (item != null)
            {
                item.IsSelected = false;
            }

            // disable canvas selection
            foreach (CircuitShapes.ComponentDraw elem in circuitCanvas.Children)
            {
                elem.SetSelected(false);
            }

            // ascundere proprietati
            PropertiesDenumire.Text =
                PropertiesValoare.Text =
                PropertiesDefazaj.Text =
                PropertiesTensiune.Text =
                PropertiesIntensitate.Text = "";
        }


        // ---------------- Toolbar handlers -------------------
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // activeaza butoanele din toolbar
            e.CanExecute = true;
        }

        private void NewToolBtn_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).DocumentNou();
        }

        private void OpenToolBtn_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).Deschidere();
        }

        private void SaveToolBtn_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).Salvare();
        }

        private void ComponentNouToolBtn_Click(object sender, RoutedEventArgs e)
        {
            AddComponentBtn_Click(sender, e);
        }

        private void ModificareGeneratorBtn_Click(object sender, RoutedEventArgs e)
        {
            ModifyGeneratorDialog dialog = new ModifyGeneratorDialog(GetCircuit().Frecventa, GetCircuit().Tensiune);
            if(dialog.ShowDialog() == true)
            {
                GetCircuit().Frecventa = dialog.Frecventa;
                GetCircuit().Tensiune = dialog.Tensiune;
            }
            UpdatePropertiesView();
        }

        private void HelpToolBtn_Click(object sender, RoutedEventArgs e)
        {
            HelpDialog dlg = new HelpDialog();
            dlg.Show();
        }

        private void AboutToolBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this,
@"Circuit Pro
Proiectarea și vizualizarea circuitelor electrice de curent alternativ

Proiect pentru susținerea examenului de atestat profesional la informatică
(c) 2021 Butufei Tudor-David"
                , "Despre aplicație", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        // ---------------- Component list handlers ------------------
        private void ComponentList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // reseteaza element selectat atunci cand click in afara elementelor
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

            if (r.VisualHit.GetType() != typeof(ListBoxItem))
            {
                SetSelected(null);
            }
        }

        private void ComponentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0)
            {
                return;
            }

            Component c = ((KeyValuePair<string,Component>) e.AddedItems[0]).Value;
            if(c != null)
            {
                SetSelected(c);
            }
        }

        private void AddComponentBtn_Click(object sender, RoutedEventArgs e)
        {
            // creare dialog
            AddComponentDialog dialog = new AddComponentDialog();
            if (dialog.ShowDialog() == true)
            {
                if(dialog.Valoare == 0)
                {
                    throw new Exception("Valoarea componentei nu poate fi nulă.");
                }
                // preluare date si creare component
                Component c = null;
                Circuit circ = ((App)Application.Current).circuit;
                switch (dialog.Tip)
                {
                    case TipComponent.REZISTENTA:
                        c = new Rezistenta(dialog.Nume, dialog.Valoare);
                        break;

                    case TipComponent.CONDENSATOR:
                        c = new Condensator(dialog.Nume, dialog.Valoare);
                        break;

                    case TipComponent.BOBINA:
                        c = new Bobina(dialog.Nume, dialog.Valoare);
                        break;
                }

                // adaugare component in lista
                try
                {
                    circ.AddComponent(c);
                    UpdateComponentListView();
                }
                catch (Exception ex)
                {
                    // afisare mesaj eroare
                    MessageBox.Show(this, ex.Message, "Date incorecte", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private void RemoveComponentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (componentList.SelectedItem == null)
                return;

            // sterge element selectat (daca exista)
            KeyValuePair<string, Component> sel = (KeyValuePair<string, Component>)componentList.SelectedItem;
            GetCircuit().componente.Remove(sel.Key);

            // actualizare interfata
            UpdateComponentListView();
            UpdateCircuitView();
            SetSelected(null);
        }



        // ----------------- Circuit input handlers ---------------------
        private void UpdateCircuitBtn_Click(object sender, RoutedEventArgs e)
        {
            // actualizare circuit in functie de input utilizator
            UpdateCircuitView();
            SetSelected(null);
        }

        private void ResetCircuitBtn_Click(object sender, RoutedEventArgs e)
        {
            // reseteaza circuit
            DescriereCircutText.Text = "";
            UpdateCircuitView();
            SetSelected(null);
        }

        private void DescriereCircutText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                return;
            }

            // actualizare ciruit pe enter
            if (e.Key == Key.Return)
            {
                UpdateCircuitBtn_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                ResetCircuitBtn_Click(null, null);
            }
        }


        // ----------------- Circuit tree handlers ----------------------
        private void CircuitTree_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // reseteaza element selectat atunci cand click in afara elementelor
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

            if (r.VisualHit.GetType() != typeof(ComponentTreeItem))
            {
                SetSelected(null);
            }
        }

        private void CircuitTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ComponentTreeItem selected = (ComponentTreeItem)e.NewValue;
            if (selected == null)
            {
                return;
            }

            // selectare element
            SetSelected(selected.component, selected.Id);
        }


        // ----------------- Circuit canvas handlers -----------------------
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

        private void CircuitCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetSelected(null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !((App)Application.Current).InchidereFisier();
        }
    }
}

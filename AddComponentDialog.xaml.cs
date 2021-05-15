using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using CircuitPro.CircuitModel;

namespace CircuitPro
{
    /// <summary>
    /// Interaction logic for AddComponentDialog.xaml
    /// </summary>
    public partial class AddComponentDialog : Window
    {
        public AddComponentDialog()
        {
            InitializeComponent();
        }

        private void TipComponenCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LabelValoare == null)
                return;

            switch(TipComponenCombo.SelectedIndex)
            {
                case 0:
                    LabelValoare.Text = "Rezistență";
                    break;

                case 1:
                    LabelValoare.Text = "Capacitate";
                    break;

                case 2:
                    LabelValoare.Text = "Inductanță";
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public TipComponent Tip => TipComponenCombo.SelectedIndex switch
        {
            0 => TipComponent.REZISTENTA,
            1 => TipComponent.CONDENSATOR,
            2 => TipComponent.REZISTENTA,
            _ => TipComponent.REZISTENTA,
        };

        public string Nume => NumeComponentText.Text;

        public int Valoare => int.Parse(ValoareComponentText.Text);

        private void ValoareComponentText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumeComponentText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("\\W+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}

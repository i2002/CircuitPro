using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
                    UnitateMasura.Text = "Ω";
                    break;

                case 1:
                    LabelValoare.Text = "Capacitate";
                    UnitateMasura.Text = "1/π F";
                    break;

                case 2:
                    LabelValoare.Text = "Inductanță";
                    UnitateMasura.Text = "1/π H";
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public TipComponent Tip => TipComponenCombo.SelectedIndex switch
        {
            0 => TipComponent.REZISTENTA,
            1 => TipComponent.CONDENSATOR,
            2 => TipComponent.BOBINA,
            _ => TipComponent.REZISTENTA,
        };

        public string Nume => NumeComponentText.Text.Trim();

        public double Valoare => ValoareComponentText.Text != "" ? double.Parse(ValoareComponentText.Text) : 0;

        private void ValoareComponentText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumeComponentText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[^\w]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }
    }
}

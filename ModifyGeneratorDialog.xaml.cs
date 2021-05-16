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

namespace CircuitPro
{
    /// <summary>
    /// Interaction logic for ModifyGeneratorDialog.xaml
    /// </summary>
    public partial class ModifyGeneratorDialog : Window
    {
        public double Frecventa => FrecventaText.Text != "" ? double.Parse(FrecventaText.Text) : 0;

        public double Tensiune => TensiuneText.Text != "" ? double.Parse(TensiuneText.Text) : 0;

        public ModifyGeneratorDialog(double frecventa, double tensiune)
        {
            InitializeComponent();

            // valori inițiale
            FrecventaText.Text = frecventa.ToString();
            TensiuneText.Text = tensiune.ToString();
        }

        private void VerificareInputNumar(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

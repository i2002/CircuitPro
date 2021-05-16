using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;

using CircuitPro.CircuitModel;

namespace CircuitPro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public CircuitModel.Circuit circuit;
        private bool modificat = false;
        private string numeFisier = "";

        private static readonly string filters = "Diagramă circuit|*.cpro|Toate fișierele|*.*";


        // ------------- Operatii ---------------
        public void DocumentNou()
        {
            if(InchidereFisier())
            {
                SetareCircuit();
            }
        }

        public void Salvare()
        {
            if(numeFisier == "")
            {
                // setari dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Salvează circuit",
                    FileName = "Circuit nou",
                    DefaultExt = ".cpro",
                    Filter = filters
                };

                // afisare dialog
                if (saveFileDialog.ShowDialog() != true)
                {
                    return;
                }

                // selectare nume fisier
                numeFisier = saveFileDialog.FileName;
            }

            // salvare circuit
            string json = circuit.Serialize();
            File.WriteAllText(numeFisier, json);
            SetModificat(false);
        }

        public void Deschidere()
        {
            if(!InchidereFisier())
            {
                return;
            }

            // setari dialog
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "Deschide circuit", Filter = filters };

            // afisare dialog
            if(openFileDialog.ShowDialog() == true)
            {
                // deschidere circuit
                try
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    Circuit c = Circuit.Deserialize(json);
                    SetareCircuit(c, openFileDialog.FileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Eroare deschidere fișier", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            }
        }

        public bool InchidereFisier()
        {
            if (modificat)
            {
                MessageBoxResult res = MessageBox.Show("Doriți să salvați modificările?", "Închidere fișier", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                if(res == MessageBoxResult.Yes)
                {
                    Salvare();
                    return true;
                }

                return res == MessageBoxResult.No;
            }

            return true;
        }

        public void SetModificat(bool mod = true)
        {
            modificat = mod;
            ActualizareTitlu();
        }


        // ----------- Gestiune date ------------
        private void SetareCircuit(Circuit c = null, string fileName = "")
        {
            // actualizare model
            circuit = c ?? new Circuit();
            numeFisier = fileName;
            modificat = false;

            // actualizare view
            ((MainWindow)MainWindow).UpdateCircuit();
            ActualizareTitlu();
        }

        private void ActualizareTitlu()
        {
            string aplicatie = "Circuit Pro";
            string fisier = numeFisier == "" ? "Circuit nou" : Path.GetFileName(numeFisier);
            string decorator = modificat ? "* " : "";

            MainWindow.Title = $"{decorator}{fisier} - {aplicatie}";
        }
    }
}

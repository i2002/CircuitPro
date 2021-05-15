using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CircuitPro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public CircuitModel.Circuit circuit;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            circuit = new CircuitModel.Circuit();
        }
    }
}

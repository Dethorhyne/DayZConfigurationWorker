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
using ShardTools.DayZConfigurationWorker;

namespace DayZ_Name_Changer
{
    public class ConfigurationWorker
    {
        public TextSetting PlayerName { get; set; }

        public ConfigurationWorker()
        {

        }
    }

    public class TextSetting
    {
        public DayZConfig.ConfigEntry Setting { get; set; }

        public TextSetting(DayZConfig.ConfigEntry x)
        {
            Setting = x;
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DayZConfig DC = new DayZConfig(@"C:\Users\Detho\Documents\DayZ\Detho.DayZProfile");
        public ConfigurationWorker DW = new ConfigurationWorker();
        public MainWindow()
        {
            InitializeComponent();
            DW.PlayerName = new TextSetting(DC["playername"]);

            DataContext = DW;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DC.UpdateProfile(false);
        }
    }
}

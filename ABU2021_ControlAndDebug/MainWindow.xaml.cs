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

namespace ABU2021_ControlAndDebug
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //コントロールの拡大
            MainGrid.Height = (SystemParameters.WorkArea.Height + 30) / 1.3;//何故かGDPのディスプレイだとずれるのでプラス
            MainGrid.Width = SystemParameters.WorkArea.Width / 1.3;
        }
    }
}

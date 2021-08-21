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
using System.Windows.Shapes;

namespace ABU2021_ControlAndDebug.SubWindows
{
    /// <summary>
    /// SetArrowNumWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SetArrowNumWindow : Window
    {
        public int ArrowNum { get; private set; } = -1;
        public bool IsMachine { get; set; }

        public SetArrowNumWindow()
        {
            InitializeComponent();
            GetWindow(this).Title = IsMachine ? "機体上" : "ラック上" + " 矢数入力";
        }

        private void NumButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            ArrowNum = int.Parse((string)button.Content);

            Window.GetWindow(this).Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}

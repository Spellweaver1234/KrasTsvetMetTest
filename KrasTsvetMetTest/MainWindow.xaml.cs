using System.Windows;

namespace KrasTsvetMetTest
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ApplicationViewModel(new DefaultDialogService(), new ExcelFileService());
        }
    }
}

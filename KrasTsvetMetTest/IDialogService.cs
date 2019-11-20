using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrasTsvetMetTest
{
    public interface IDialogService
    {
        // показ сообщения
        void ShowMessage(string message);   
        
        // путь к выбранному файлу
        string FilePath { get; set; }   
        
        // открытие файла
        bool OpenFolderDialog();
        
        // сохранение файла
        bool SaveFileDialog();  
    }
}

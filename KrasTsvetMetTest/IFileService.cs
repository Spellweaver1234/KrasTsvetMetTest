using System.Collections.ObjectModel;

namespace KrasTsvetMetTest
{
    public interface IFileService
    {
        // получает путь к файлу и возвращает список объектов
        string[,] OpenExcel(string path);

        // сохраняет объекты по пути
        void SaveExcel(string filename, ObservableCollection<Raspisanie> list);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Excel = Microsoft.Office.Interop.Excel;
using ClosedXML;
using ClosedXML.Excel;
using Syncfusion.XlsIO;
using WinForms = System.Windows.Forms;

namespace KrasTsvetMetTest
{
    public partial class MainWindow : Window
    {
        List<Times> times;
        List<Machine_tools> machine_Tools;
        List<Nomenclatures> nomenclatures;
        List<Parties> parties;
        List<Raspisanie> raspisanie;
        string folderPath;

        public MainWindow()
        {
            InitializeComponent();

            MessageBox.Show("Пожалуйста, выберите папку в которой находятся следующие файлы: " +
                "\n\n times.xlsx" +
                "\n machine_tools.xlsx" +
                "\n nomenclatures.xlsx" +
                "\n parties.xlsx",
                "Начало работы", 
                MessageBoxButton.OK, MessageBoxImage.Information);

            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            if (fbd.ShowDialog() == WinForms.DialogResult.OK)
            {
                folderPath = fbd.SelectedPath.ToString();

                MessageBox.Show("Ожидайте завершения анализа файлов");
                MainProcess();
            }
            else
            {
                Application.Current.Shutdown();
            }


        }

        private void MainProcess()
        {
            string[,] tData = null;
            string[,] mData = null;
            string[,] nData = null;
            string[,] pData = null;

            try
            {
                tData = ExcelParse(folderPath + "\\" + "times.xlsx");
                mData = ExcelParse(folderPath + "\\" + "machine_tools.xlsx");
                nData = ExcelParse(folderPath + "\\" + "nomenclatures.xlsx");
                pData = ExcelParse(folderPath + "\\" + "parties.xlsx");

                times = Times.TParse(tData);
                machine_Tools = Machine_tools.MParse(mData);
                nomenclatures = Nomenclatures.NParse(nData);
                parties = Parties.PParse(pData);
                raspisanie = new List<Raspisanie>();

                // партия -> номенклатура -> машина -> вычисление -> расписание
                while (parties.Count > 0)
                {
                    // берём следующую партию и возвращаем оставшиеся
                    Parties current_party = GetParties(parties, out var newParties);

                    // определяем номенклатуру
                    Nomenclatures current_nomenclature = GetNomenclature(current_party, nomenclatures);

                    // выбираем для номенклатуры машину
                    Machine_tools current_machine = GetMachine(current_nomenclature, machine_Tools, times);

                    string partyName = current_nomenclature.nomenclature;
                    string equipmentName = current_machine.name;
                    string tStart = current_machine.time.ToString();
                    string tStop = СalculationTime(current_machine, current_nomenclature, times);

                    raspisanie.Add(new Raspisanie(partyName, equipmentName, tStart, tStop));
                }

                dataGrid.ItemsSource = raspisanie;
                ShowStats(machine_Tools);
            }
            catch
            {
                MessageBox.Show("В папке не найдены нужные файлы", "Ошибка чтения файлов");
                Application.Current.Shutdown();
            }
        }

        // все данные из Эксель
        private string[,] ExcelParse(string path)
        {
            Excel.Application application = new Excel.Application();
            Excel.Workbook workbook = application.Workbooks.Open(path, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            Excel.Worksheet worksheet = (Excel.Worksheet)application.Sheets[1];
            var lastCell = worksheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);
            int lastColumn = lastCell.Column;
            int lastRow = lastCell.Row;
            string[,] buff = new string[lastRow, lastColumn];

            for (int i = 0; i < lastCell.Row; i++)                             // по всем колонкам
            {
                for (int j = 0; j < lastCell.Column; j++)                            // по всем строкам
                {
                    buff[i, j] = application.Cells[i + 1, j + 1].Text.ToString();    // считываем текст в строку
                }
            }
            workbook.Close(false, Type.Missing, Type.Missing);
            application.Quit();
            return buff;
        }

        // получаем партию и отделяем её от списка всех партий
        private Parties GetParties(List<Parties> parties, out List<Parties> newparties)
        {
            newparties = parties;
            foreach (var item in newparties)                        // перебор всех партий
            {
                string nomenclature = "";
                foreach (var name in nomenclatures)             // перебор номенклатур
                {
                    if (item.nomenclature_id == name.id)        // если совпадает
                    {
                        nomenclature = name.id;               // ид
                        newparties.Remove(item);           // удаяем из общего списка партий
                        return item;
                    }
                }
            }
            return null;
        }

        // получаем номенклатуру по партии
        private Nomenclatures GetNomenclature(Parties parties, List<Nomenclatures> nomenclatures)
        {
            foreach (var item in nomenclatures)
            {
                if (item.id == parties.nomenclature_id)
                {
                    return item;
                }
            }
            return null;
        }

        // по номенклатуре, списку машин и времени получаем нужную машину (ближайшую свободную)
        private Machine_tools GetMachine(Nomenclatures nomenclatures, List<Machine_tools> machine_Tools, List<Times> times)
        {
            // поиск ид возможных машин
            List<string> posMachines = new List<string>();
            foreach (var item in times)
            {
                if (nomenclatures.id == item.nomenclature_id)
                {
                    posMachines.Add(item.machine_tool_id);
                }
            }

            Machine_tools result = null;
            int min = int.MaxValue;
            foreach (var item in posMachines)
            {
                Machine_tools buff = null;
                foreach(var o in machine_Tools)
                {
                    if (o.id == item)
                    {
                        buff = o;
                        break;
                    }
                }

                if (min > buff.time)
                {
                    min = buff.time;
                    result = buff;
                }
            }

            return result;
        }

        // расчёт времени, вычисление времени завершения процеса
        private string СalculationTime(Machine_tools machine, Nomenclatures item, List<Times> times)
        {
            // перебор времен
            for (int i = 0; i < times.Count; i++)
            {
                // совпадение по ид машины и номенклатуры
                if (times[i].machine_tool_id == machine.id && times[i].nomenclature_id == item.id)
                {
                    // перебор машин
                    for (int k = 0; k < machine_Tools.Count; k++)
                    {
                        // совпадение по ид
                        if (machine_Tools[k].id == machine.id)
                        {
                            machine.time += int.Parse(times[i].operation_time);
                            return machine.time.ToString();
                        }
                    }
                    break;
                }
            }
            return null;
        }

        private void ShowStats(List<Machine_tools> machine_Tools)
        {
            lab_m1.Content = machine_Tools[0].time;
            lab_m2.Content = machine_Tools[1].time;
            lab_m3.Content = machine_Tools[2].time;
            lab_total.Content = machine_Tools[0].time + machine_Tools[1].time + machine_Tools[2].time;
        }

        private void ExportToExcel()
        {
            try
            {
            var workbook = new XLWorkbook();
            workbook.AddWorksheet("Расписание");
            var ws = workbook.Worksheet("Расписание");
            int row = 1;
            foreach (var c in raspisanie)
            {
                ws.Cell("A" + row.ToString()).Value = c.party;
                ws.Cell("B" + row.ToString()).Value = c.equipment;
                ws.Cell("C" + row.ToString()).Value = c.tStart;
                ws.Cell("D" + row.ToString()).Value = c.tStop;
                row++;

            }

            string fileName = folderPath + "\\" + "Raspisanie.xlsx";
            workbook.SaveAs(fileName);
                MessageBox.Show("Файл успешно сохранён в папке с исходными файлами" +
                    "\n " + fileName
                    , "Результат");
            }
            catch
            {
                MessageBox.Show("Не удалось сохранить файл");
            }
        }

        private void but_Save_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel();
        }
    }

    class Raspisanie
    {
        public Raspisanie(string party, string equipment, string tStart, string tStop)
        {
            this.party = party;
            this.equipment = equipment;
            this.tStart = tStart;
            this.tStop = tStop;
        }
        public string party { get; set; }
        public string equipment { get; set; }
        public string tStart { get; set; }
        public string tStop { get; set; }
    }
}

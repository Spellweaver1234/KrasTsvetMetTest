using System.Collections.ObjectModel;

namespace KrasTsvetMetTest
{
    class ExcelFileService : IFileService
    {
        public string[,] OpenExcel(string filename)
        {
            using (ClosedXML.Excel.XLWorkbook workbook = new ClosedXML.Excel.XLWorkbook(filename))
            {
                ClosedXML.Excel.IXLWorksheet worksheet = workbook.Worksheet(1);
                int rc = worksheet.RangeUsed().RowCount();
                int cc = worksheet.RangeUsed().Row(1).CellCount();
                string[,] buff = new string[rc, cc];
                for (int i = 0; i < rc; i++)
                {
                    ClosedXML.Excel.IXLRow row = worksheet.Row(i + 1);
                    for (int j = 0; j < cc; j++)
                    {
                        ClosedXML.Excel.IXLCell cell = row.Cell(j + 1);
                        string value = cell.GetValue<string>();
                        buff[i, j] = value;
                    }
                }

                return buff;
            }
        }

        // сохранение. путь к папке, лист с данными
        public void SaveExcel(string fileName, ObservableCollection<Raspisanie> raspisanie)
        {
            using (ClosedXML.Excel.XLWorkbook workbook = new ClosedXML.Excel.XLWorkbook())
            {
                workbook.AddWorksheet("Расписание");
                var ws = workbook.Worksheet("Расписание");
                int row = 1;
                foreach (var c in raspisanie)
                {
                    ws.Cell("A" + row.ToString()).Value = c.Party;
                    ws.Cell("B" + row.ToString()).Value = c.Equipment;
                    ws.Cell("C" + row.ToString()).Value = c.TStart;
                    ws.Cell("D" + row.ToString()).Value = c.TStop;
                    row++;
                }

                workbook.SaveAs(fileName + "\\" + "Raspisanie.xlsx");
            }
        }
    }
}

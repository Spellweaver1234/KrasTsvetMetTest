using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KrasTsvetMetTest
{
    // модель-представление
    class ApplicationViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Parties> party { get; set; }
        public ObservableCollection<Nomenclatures> nomenclatures { get; set; }
        public ObservableCollection<Times> times { get; set; }
        public ObservableCollection<Machine_tools> machine_Tools { get; set; }
        public ObservableCollection<Raspisanie> Raspisanies { get; set; }

        ObservableCollection<KeyValuePair<double, double>> Power;

        IFileService fileService;
        IDialogService dialogService;

        public ApplicationViewModel(IDialogService dialogService, IFileService fileService)
        {
            this.dialogService = dialogService;
            this.fileService = fileService;

            party = new ObservableCollection<Parties>();
            nomenclatures = new ObservableCollection<Nomenclatures>();
            times = new ObservableCollection<Times>();
            machine_Tools = new ObservableCollection<Machine_tools>();
            Raspisanies = new ObservableCollection<Raspisanie>();
        }

        // команда открытия файлов и первичного считывания
        private RelayCommand openCommand;
        public RelayCommand OpenCommand
        {
            get
            {
                return openCommand ??
                  (openCommand = new RelayCommand(obj =>
                  {
                      try
                      {
                          if (dialogService.OpenFolderDialog() == true)
                          {
                              string[,] tData = fileService.OpenExcel(dialogService.FilePath + "\\" + "times.xlsx");
                              string[,] mData = fileService.OpenExcel(dialogService.FilePath + "\\" + "machine_tools.xlsx");
                              string[,] nData = fileService.OpenExcel(dialogService.FilePath + "\\" + "nomenclatures.xlsx");
                              string[,] pData = fileService.OpenExcel(dialogService.FilePath + "\\" + "parties.xlsx");

                              dialogService.ShowMessage("Данные добавлены");

                              party.Clear();
                              nomenclatures.Clear();
                              machine_Tools.Clear();
                              times.Clear();
                              Raspisanies.Clear();

                              foreach (var item in Parties.PParse(pData))
                                  party.Add(item);

                              foreach (var item in Nomenclatures.NParse(nData))
                                  nomenclatures.Add(item);

                              foreach (var item in  Machine_tools.MParse(mData))
                                  machine_Tools.Add(item);

                              foreach(var item in  Times.TParse(tData))
                                  times.Add(item);
                          }
                      }
                      catch (Exception ex)
                      {
                          dialogService.ShowMessage(ex.Message);
                      }
                  }));
            }
        }

        // команда распределения данных если данные есть в количестве > 0 
        private RelayCommand distributionCommand;
        public RelayCommand DistributionCommand
        {
            get
            {
                return distributionCommand ??
                  (distributionCommand = new RelayCommand(obj =>
                  {
                      try
                      {
                          Distribution();
                          dialogService.ShowMessage("Распределение выполнено!");
                      }
                      catch (Exception ex)
                      {
                          dialogService.ShowMessage(ex.Message);
                      }
                  },
                  (obj) =>
                  (times.Count>0) &&
                  (machine_Tools.Count > 0) &&
                  (nomenclatures.Count > 0) &&
                  (party.Count > 0)));
            }
        }

        // команда сохранения файла если расписание составлено
        private RelayCommand saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ??
                  (saveCommand = new RelayCommand(obj =>
                  {
                      try
                      {
                          if (dialogService.SaveFileDialog() == true)
                          {
                              fileService.SaveExcel(dialogService.FilePath, Raspisanies);
                              dialogService.ShowMessage("Файл сохранен");
                          }
                      }
                      catch (Exception ex)
                      {
                          dialogService.ShowMessage(ex.Message);
                      }
                  },
                  (obj) => Raspisanies.Count > 0 ));
            }
        }

        // распределение
        private void Distribution()
        {
            // партия -> номенклатура -> машина -> вычисление -> расписание
            while (party.Count > 0)
            {
                // берём следующую партию и возвращаем оставшиеся
                Parties current_party = GetParties(party, out var newParties);

                // определяем номенклатуру
                Nomenclatures current_nomenclature = GetNomenclature(current_party, nomenclatures);

                // выбираем для номенклатуры машину
                Machine_tools current_machine = GetMachine(current_nomenclature, machine_Tools, times);

                string partyName = current_nomenclature.Nomenclature;
                string equipmentName = current_machine.name;
                string tStart = current_machine.time.ToString();
                string tStop = СalculationTime(current_machine, current_nomenclature, times);

                Raspisanies.Add(new Raspisanie { Party = partyName, Equipment = equipmentName, TStart = tStart, TStop = tStop });
            }
        }

        // получаем партию и отделяем её от списка всех партий
        private Parties GetParties(ObservableCollection<Parties> parties, out ObservableCollection<Parties> newparties)
        {
            newparties = parties;
            foreach (var item in newparties)                        // перебор всех партий
            {
                string nomenclature = "";
                foreach (var name in nomenclatures)             // перебор номенклатур
                {
                    if (item.Nomenclature_id == name.Id)        // если совпадает
                    {
                        nomenclature = name.Id;               // ид
                        newparties.Remove(item);           // удаяем из общего списка партий
                        return item;
                    }
                }
            }
            return null;
        }

        // получаем номенклатуру по партии
        private Nomenclatures GetNomenclature(Parties parties, ObservableCollection<Nomenclatures> nomenclatures)
        {
            foreach (var item in nomenclatures)
            {
                if (item.Id == parties.Nomenclature_id)
                {
                    return item;
                }
            }
            return null;
        }

        // по номенклатуре, списку машин и времени получаем нужную машину (ближайшую свободную)
        private Machine_tools GetMachine(Nomenclatures nomenclatures, ObservableCollection<Machine_tools> machine_Tools, ObservableCollection<Times> times)
        {
            // поиск ид возможных машин
            ObservableCollection<string> posMachines = new ObservableCollection<string>();
            foreach (var item in times)
            {
                if (nomenclatures.Id == item.nomenclature_id)
                {
                    posMachines.Add(item.machine_tool_id);
                }
            }

            Machine_tools result = null;
            int min = int.MaxValue;
            foreach (var item in posMachines)
            {
                Machine_tools buff = null;
                foreach (var o in machine_Tools)
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
        private string СalculationTime(Machine_tools machine, Nomenclatures item, ObservableCollection<Times> times)
        {
            // перебор времен
            for (int i = 0; i < times.Count; i++)
            {
                // совпадение по ид машины и номенклатуры
                if (times[i].machine_tool_id == machine.id && times[i].nomenclature_id == item.Id)
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}

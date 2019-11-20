using System.Collections.ObjectModel;

namespace KrasTsvetMetTest
{
    public class Parties
    {
        public string Id { get; set; }
        public string Nomenclature_id { get; set; }

        public Parties(string id, string nomenclature_id)
        {
            this.Id = id;
            this.Nomenclature_id = nomenclature_id;
        }

        public static ObservableCollection<Parties> PParse(string[,] parties)
        {
            ObservableCollection<Parties> buff = new ObservableCollection<Parties>();
            for (int i = 1; i < parties.GetLength(0); i++)      // со 2ой строчки так как первая - шапка таблицы
            {
                string id, nomenclature_id;
                for (int j = 0; j < parties.GetLength(1); j += 2)
                {
                    id = parties[i, j];
                    nomenclature_id = parties[i, j + 1];
                    buff.Add(new Parties(id, nomenclature_id));
                }
            }
            return buff;
        }
    }
}
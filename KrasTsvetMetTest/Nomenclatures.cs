using System.Collections.ObjectModel;

namespace KrasTsvetMetTest
{
    class Nomenclatures
    {
        public string Id { get; set; }
        public string Nomenclature { get; set; }

        public Nomenclatures(string id, string nomenclature)
        {
            this.Id = id;
            this.Nomenclature = nomenclature;
        }

        public static ObservableCollection<Nomenclatures> NParse(string[,] nomenclatures)
        {
            var buff = new ObservableCollection<Nomenclatures>();
            for (int i = 1; i < nomenclatures.GetLength(0); i++)
            {
                string id, nomenclature;
                for (int j = 0; j < nomenclatures.GetLength(1); j += 2)
                {
                    id = nomenclatures[i, j];
                    nomenclature = nomenclatures[i, j + 1];
                    buff.Add(new Nomenclatures (id, nomenclature ));
                }
            }
            return buff;
        }
    }
}

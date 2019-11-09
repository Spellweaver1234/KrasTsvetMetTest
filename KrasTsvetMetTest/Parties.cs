using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrasTsvetMetTest
{
    class Parties
    {
        public string id { get; set; }
        public string nomenclature_id { get; set; }

        public Parties(string id, string nomenclature_id)
        {
            this.id = id;
            this.nomenclature_id = nomenclature_id;
        }

        public static List<Parties> PParse(string[,] parties)
        {
            var buff = new List<Parties>();
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
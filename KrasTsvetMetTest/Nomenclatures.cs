using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrasTsvetMetTest
{
    class Nomenclatures
    {
        public string id { get; set; }
        public string nomenclature { get; set; }

        public Nomenclatures(string id, string nomenclature)
        {
            this.id = id;
            this.nomenclature = nomenclature;
        }

        public static List<Nomenclatures> NParse(string[,] nomenclatures)
        {
            var buff = new List<Nomenclatures>();
            for (int i = 1; i < nomenclatures.GetLength(0); i++)
            {
                string id, nomenclature;
                for (int j = 0; j < nomenclatures.GetLength(1); j += 2)
                {
                    id = nomenclatures[i, j];
                    nomenclature = nomenclatures[i, j + 1];
                    buff.Add(new Nomenclatures(id, nomenclature));
                }
            }
            return buff;
        }
    }
}

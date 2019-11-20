using System.Collections.ObjectModel;

namespace KrasTsvetMetTest
{
    class Times
    {
        public string machine_tool_id { get; set; }
        public string nomenclature_id { get; set; }
        public string operation_time { get; set; }

        public Times(string machine_tool_id,string nomenclature_id,string operation_time)
        {
            this.machine_tool_id = machine_tool_id;
            this.nomenclature_id = nomenclature_id;
            this.operation_time = operation_time;
        }

        public static ObservableCollection<Times> TParse(string[,] times)
        {
            var buff = new ObservableCollection<Times>();
            for (int i = 1; i < times.GetLength(0); i++)
            {
                string mti, ni, ot;
                for (int j = 0; j < times.GetLength(1); j += 3)
                {
                    mti = times[i, j];
                    ni = times[i, j + 1];
                    ot = times[i, j + 2];
                    buff.Add(new Times(mti, ni, ot));
                }
            }
            return buff;
        }
    }
}

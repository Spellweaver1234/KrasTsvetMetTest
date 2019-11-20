using System.Collections.ObjectModel;

namespace KrasTsvetMetTest
{
    class Machine_tools
    {
        public string id { get; set; }
        public string name { get; set; }
        public int time { get; set; }
        public int timeStop { get; set; }

        public Machine_tools(string id, string name)
        {
            this.id = id;
            this.name = name;
            this.time = 0;
        }

        public static ObservableCollection<Machine_tools> MParse(string[,] machine_Tools)
        {
            var buff = new ObservableCollection<Machine_tools>();
            for (int i = 1; i < machine_Tools.GetLength(0); i++)
            {
                string id, name;
                for (int j = 0; j < machine_Tools.GetLength(1); j += 2)
                {
                    id = machine_Tools[i, j];
                    name = machine_Tools[i, j + 1];
                    buff.Add(new Machine_tools(id, name));
                }
            }
            return buff;
        }
    }
}

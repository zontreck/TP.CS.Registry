using TP.CS.Registry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP.CS.Registry.RegRead
{
    public class Program
    {
        public static int Main(string[] args)
        {
            string file = args[0];
            string key = args[1];
            bool json = false;
            if (args.Length > 2)
            {
                json = true;
            }
            EventsBus.EventBus.debug = false;

            Key hive = RegistryIO.loadHive(file);

            Entry en = hive.getAtPath(key);
            if(json)
                Console.WriteLine(JsonConvert.SerializeObject(en, Formatting.Indented));
            else
            {
                switch (en.Type)
                {
                    case EntryType.Word:
                        {
                            Console.Write(en.Word().Value);
                            break;
                        }
                    case EntryType.Int16:
                        {
                            Console.Write(en.Int16().Value); 
                            break;
                        }
                    case EntryType.Int32:
                        {
                            Console.Write(en.Int32().Value);
                            break;
                        }
                    case EntryType.Int64:
                        {
                            Console.Write(en.Int64().Value);
                            break;
                        }
                    case EntryType.Bool:
                        {
                            Console.Write(en.Bool().Value);
                            break;
                        }
                    case EntryType.Byte:
                        {
                            Console.Write(en.Byte().Value);
                            break;
                        }
                }
            }

            return (int) en.Type;
        }
    }
}

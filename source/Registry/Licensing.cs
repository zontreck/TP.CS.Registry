using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TP.CS.Registry
{
    public class Licensing
    {
        public enum KeyType : byte
        {
            Customer,
            Support,
            Patron
        }
        private Key element = new Key("key")
        {
            Type = EntryType.Root
        };

        public int Expiry = -1;
        public KeyType TypeOfKey = KeyType.Customer;
        public string Name;

        public Licensing()
        {
            Expiry = 365;
            
        }

        public Licensing(string Keyx)
        {
            byte[] arr = Convert.FromHexString(Keyx);

            element = RegistryIO.loadWithoutHeader(arr);
            Expiry = element.getNamed("X").Int32().Value;
            TypeOfKey = (KeyType)element.getNamed("T").Byte().Value;
            Name = element.getNamed("N").Word().Value;
        }

        public string MakeKey()
        {
            element = new Key("key")
            {
                Type = EntryType.Root
            };

            element.Add(new VInt32("X", Expiry));
            element.Add(new Word("N", Name));
            element.Add(new VByte("T", (byte)TypeOfKey));
            

            byte[] arr = RegistryIO.saveWithoutHeader(element);

            return Convert.ToHexString(arr);
        }

        public override string ToString()
        {
            return MakeKey();
        }
    }
}

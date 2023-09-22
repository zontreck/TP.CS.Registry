using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        public string Name = "No Name";
        public bool B64 = false;

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

        public byte[] MakeKey()
        {
            byte[] res = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {

                    bw.Write((byte)TypeOfKey);
                    bw.Write(Name);
                    bw.Write(Expiry);
                }

                res=ms.ToArray();
            }


            //byte[] arr = RegistryIO.saveWithoutHeader(element);
            return res;
        }

        public byte[] CompressKey(byte[] rawKey)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                using(GZipStream gz = new GZipStream(ms, CompressionMode.Compress, false))
                {
                    byte[] current = ms.ToArray();
                    gz.Write(rawKey, 0, rawKey.Length);

                }

                return ms.ToArray();
            }
        }

        public string ToStringKey(byte[] kx)
        {
            if (B64) return Convert.ToBase64String(kx);
            else return Convert.ToHexString(kx);
        }

        public string FormatKey(string input)
        {
            return $"" +
                $"{input.Substring(0, 4)}" +
                $"-" +
                $"{input.Substring(5, 4)}" +
                $"-" +
                $"{input.Substring(9, 6)}" +
                $"-" +
                $"{input.Substring(15)}";
        }

        public override string ToString()
        {
            return (ToStringKey(MakeKey()));
        }
    }
}

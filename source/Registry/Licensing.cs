using Microsoft.VisualBasic;
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
            Unknown,
            Customer,
            Support,
            Patron
        }

        public string Date = "09/21/2023";
        public short Expiry = -1;
        public KeyType TypeOfKey = KeyType.Customer;
        public string Name = "No Name";
        public bool B64 = false;

        public Licensing()
        {
            Expiry = 365;
            Date = DateTime.Now.ToString("MM/dd/YYYY");
        }

        public bool Expired()
        {
            if (Expiry == -1) return false;
            return DateTime.Parse(Date).AddDays(Expiry) >= DateTime.Now;
        }

        public Licensing(string Keyx, bool b64)
        {
            byte[] arr = null;
            
            if(b64) arr = Convert.FromBase64String(Keyx);
            else arr = Convert.FromHexString(Keyx);


            using(MemoryStream ms = new MemoryStream(arr))
            {
                using(BinaryReader br = new BinaryReader(ms))
                {
                    TypeOfKey = (KeyType)br.ReadByte();
                    Name = br.ReadString();
                    Expiry = br.ReadInt16();
                    Date = br.ReadString();
                }
            }
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
                    bw.Write(Date);
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
                $"{input.Substring(0, 6)}" +
                $"-" +
                $"{input.Substring(7, 3)}" +
                $"-" +
                $"{input.Substring(10)}";
        }

        public override string ToString()
        {
            return FormatKey(ToStringKey(MakeKey()));
        }
    }
}

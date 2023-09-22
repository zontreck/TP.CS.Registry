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

        public short Year;
        public byte Month;
        public byte Day;

        public short Expiry = -1;
        public KeyType TypeOfKey = KeyType.Customer;
        public string Name = "No Name";
        public bool B64 = false;

        public Licensing()
        {
            Expiry = 365;
            DateTime dt = DateTime.Now;

            Year = (short)dt.Year;
            Month = (byte)dt.Month;
            Day = (byte)dt.Day;
        }

        public bool Expired()
        {
            if (Expiry == -1) return false;
            DateTime dt = new DateTime(Year, Month, Day);
            return dt.AddDays(Expiry) >= DateTime.Now;
        }

        public int RemainingDays()
        {
            DateTime dt = new DateTime(Year, Month, Day);

            dt.AddDays(Expiry);
            return dt.Subtract(DateTime.Now).Days;
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
                    Day = br.ReadByte();
                    Name = br.ReadString();
                    Month = br.ReadByte();
                    Expiry = br.ReadInt16();

                    Year = br.ReadInt16();
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
                    bw.Write(Day);
                    bw.Write(Name);
                    bw.Write(Month);
                    bw.Write(Expiry);

                    bw.Write(Year);
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

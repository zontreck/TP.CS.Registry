using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Security.Cryptography;
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
            Patron,
            Developer
        }

        /// <summary>
        /// ByteWave - Aria's Creations 
        /// 
        /// Data Offset
        /// </summary>
        public const int BWACOffset = 0x0115044;
        public const int BWACOffset2 = 0x61726109;
        public const int BWACOffset3 = 0xFFFF;
        public const int BWACOffset4 = 0xE707;

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
            char[] chars = Keyx.ToCharArray();
            
            Keyx = $"" +
                $"{string.Join("",chars.Take(6))}" +
                $"{string.Join("", chars.Skip(6+1).Take(4))}" +
                $"{string.Join("", chars.Skip(6+1).Skip(4+1).Take(6))}" +
                $"{string.Join("", chars.Skip(6+1).Skip(4+1).Skip(6+1))}";

            byte[] arr = null;
            
            if(b64) arr = Convert.FromBase64String(Keyx);
            else arr = Convert.FromHexString(Keyx);

            BigInteger bi = new BigInteger(arr);

            bi += BWACOffset;
            bi -= BWACOffset2;
            bi += BWACOffset3;
            bi += BWACOffset4;

            arr = bi.ToByteArray();

            using (MemoryStream ms = new MemoryStream(arr))
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
            BigInteger bi = new BigInteger(res);
            bi -= BWACOffset;
            bi += BWACOffset2;
            bi -= BWACOffset3;
            bi -= BWACOffset4;

            //byte[] arr = RegistryIO.saveWithoutHeader(element);
            return bi.ToByteArray();
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
            char[] chars = input.ToCharArray();
            return $"" +
                $"{string.Join("", chars.Take(6))}" +
                $"-" +
                $"{string.Join("", chars.Skip(6).Take(4))}" +
                $"-" +
                $"{string.Join("", chars.Skip(6).Skip(4).Take(6))}" +
                $"-" +
                $"{string.Join("", chars.Skip(6).Skip(4).Skip(6))}";
        }

        public override string ToString()
        {
            return FormatKey(ToStringKey(MakeKey()));
        }
    }
}

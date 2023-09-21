using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP.CS.Registry
{
    public class WordList : Entry, IList<string>
    {
        #region Entry Implementation
        public WordList(string Name)
        {
            Type = EntryType.WordArray;
            this.Name = Name;
            Description = "";
        }


        public List<string> Value = new();

        public override void readValue(BinaryReader stream)
        {
            int count = stream.ReadInt32();

            for(int i =0; i< count; i++)
            {
                Value.Add(stream.ReadString());
            }
        }

        public override void Write(BinaryWriter stream)
        {
            base.Write(stream);

            stream.Write(Value.Count);
            foreach(var item in Value)
            {
                stream.Write(item);
            }
        }

        public override string PrettyPrint(int indent = 0)
        {
            string str = base.PrettyPrint(indent);

            // Print the entire list
            str += $" [{Value.Count}] \n";
            string ind = "".PadLeft(indent+4, ' ');

            str += ind + "{\n";
            foreach(var item in Value)
            {
                str += ind + $"\"{item}\",\n";
            }
            str = str.TrimEnd().TrimEnd(',');
            str += "\n" + ind + "}";

            return str;

        }

        public override string ToString()
        {
            return PrettyPrint(0);
        }

        #endregion

        #region IList

        public int IndexOf(string item)
        {
            return ((IList<string>)Value).IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            ((IList<string>)Value).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<string>)Value).RemoveAt(index);
        }

        public void Add(string item)
        {
            ((ICollection<string>)Value).Add(item);
        }

        public void Clear()
        {
            ((ICollection<string>)Value).Clear();
        }

        public bool Contains(string item)
        {
            return ((ICollection<string>)Value).Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            ((ICollection<string>)Value).CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return ((ICollection<string>)Value).Remove(item);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Value).GetEnumerator();
        }

        public int Count => ((ICollection<string>)Value).Count;

        public bool IsReadOnly => ((ICollection<string>)Value).IsReadOnly;

        public string this[int index] { get => ((IList<string>)Value)[index]; set => ((IList<string>)Value)[index] = value; }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP.CS.Registry
{
    /// <summary>
    /// This class differs from a Key in that it is a strict array. 
    /// 
    /// An array has no named entries. They are also all of just one type.
    /// 
    /// 
    /// TODO: This is messy, and very difficult to actually use... 
    /// I am going to refactor this at some point to have a dedicated Array type for each available data type.
    /// It'll be harder to maintain to do it that way, but it would mean being able to iterate in the native datatype.
    /// 
    /// Essentially, it would not store an entry, it would store the data type just as Word stores a String. A WordList would store strings, not Words.
    /// </summary>
    public class EntryList<T> : Entry, IList<T> where T : Entry
    {
        public EntryList(string Name)
        {
            Type = EntryType.Array;
            this.Name = Name;
            Description = "Array";
        }

        #region Entry List Elements
        public EntryType type;

        public List<T> value { get; set; } = new();


        public override void readValue(BinaryReader stream)
        {
            type = (EntryType)stream.ReadByte();

            if(type == EntryType.Empty)
            {
                return;
            }
            int count = stream.ReadInt32();
            for(int i = 0; i < count; i++)
            {
                Entry subEntry = Read(stream, true, type);

                value.Add((T)subEntry);
                subEntry.Parent = this;
            }
        }

        public override void Write(BinaryWriter stream)
        {
            base.Write(stream);

            if(value.Count > 0)
            {
                type = value[0].Type;
            }

            stream.Write((byte)type);

            if(type == EntryType.Empty)
            {
                return;
            }

            stream.Write(value.Count);
            foreach(T entry in value)
            {
                entry.Write(stream);
            }
        }

        public void updateParents()
        {
            foreach(Entry entry in value)
            {
                entry.Parent = this;
            }
        }

        public override string PrettyPrint(int indent = 0)
        {
            string str = base.PrettyPrint(indent) + " [ {Count} ]\n";
            foreach(Entry entry in value)
            {
                str += entry.PrettyPrint(indent+4) + "\n";
            }

            return str;
        }

        #endregion


        #region IList

        public T this[int index] { get => ((IList<T>)value)[index]; set => ((IList<T>)this.value)[index] = value; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)value).GetEnumerator();
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)value).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)value).RemoveAt(index);
        }

        public int Count => ((ICollection<T>)value).Count;

        public bool IsReadOnly => ((ICollection<T>)value).IsReadOnly;

        public void Add(T item)
        {
            ((ICollection<T>)value).Add(item);
        }

        public void Clear()
        {
            ((ICollection<T>)value).Clear();
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)value).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)value).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)value).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)value).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)value).Insert(index, item);
        }
        #endregion
    }
}

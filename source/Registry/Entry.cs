using TP.CS.EventsBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace TP.CS.Registry
{
    public abstract class Entry
    {
        /// <summary>
        /// The type of the entry
        /// </summary>
        public EntryType Type { get; set; } = EntryType.Root;

        /// <summary>
        /// Entry Name
        /// </summary>
        public string Name { get; set; } = "(noname)";

        /// <summary>
        /// This flag controls whether the description value gets encoded for all child items. It can only be set on the root.
        /// </summary>
        public bool EncodeDescription { get; set; } = true;

        /// <summary>
        /// Entry Common Name
        /// 
        /// This may be used for friendlier display of a Entry Name
        /// </summary>
        public string Description { get; set; } = "";


        public Stream ValueStream { get; set; }


        /// <summary>
        /// Entry Path
        /// 
        /// Should only show names, not descriptions
        /// </summary>
        public string EntryPath
        {
            get
            {
                Stack<Entry> stack = new Stack<Entry>();
                Entry? current = this;
                while (current != null)
                {
                    stack.Push(current);
                    current = current.Parent;
                }

                string path = "";
                while(stack.Count > 0)
                {
                    string nxt = stack.Pop()?.Name ?? "";

                    path = path + "/" + nxt;
                    if (path.StartsWith("/")) path = path.Substring(1);
                }

                return path;
            }
        }


        internal void setRoot(Key root)
        {
            MyRoot = root;

            if (this is Key k)
            {
                foreach (Entry entry in k)
                {
                    entry.setRoot(root);
                }
            }
        }

        [JsonIgnore()]
        private Entry? _parent;
        /// <summary>
        /// Parent Entry Key
        /// </summary>
        [JsonIgnore()]
        public Entry? Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                var oldParent = _parent;
                var oldPath = EntryPath;
                _parent = value;
                if(value!=null)
                {
                    if(EventBus.PRIMARY.post(new RegistryEntryAddedEvent(this, EntryPath)))
                    {
                        _parent = oldParent;
                        if(_parent is Key key)
                        {
                            key.Remove(this);
                        }
                    }
                } else
                {
                    if(oldParent!=null)
                    {
                        // It may be possible that this gets fired if the old parent is null, so only dispatch in this case

                        if(EventBus.PRIMARY.post(new RegistryEntryRemovedEvent(this, oldPath, oldParent as Key)))
                        {
                            _parent = oldParent;
                            if(_parent is Key key)
                            {
                                key.Add(this);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Static root Key
        /// </summary>
        [JsonIgnore()]
        public static readonly Key ROOT = new Key("root")
        {
            Type = EntryType.Root
        };

        internal Entry()
        {
            Type = EntryType.Root;
            Name = "root";
            Description = "Registry Root";
            Parent = null;
        }

        /// <summary>
        /// Public Contstructor
        /// 
        /// For consistency, a type should never be root.
        /// 
        /// This constructor is internal and you should only use the subtype constructors.
        /// </summary>
        /// <param name="type">Entry Type</param>
        /// <param name="name">Entry Name</param>
        internal Entry(EntryType type, string name)
        {
            Type = type;
            Name = name;
        }

        public static Entry getByPath(string path)
        {
            try
            {

                // Remove the root/ text
                path = path.Substring(path.IndexOf('/') + 1);

                Entry retentry = null;
                Entry entry = ROOT;
                while (retentry == null)
                {
                    int slash = path.IndexOf("/");
                    string nextEntry = "";
                    if (slash != -1)
                        nextEntry = path.Substring(0, slash);
                    else
                    {
                        nextEntry = path;
                        path = "";
                    }



                    if (entry is Key key && nextEntry != "")
                    {
                        path = path.Substring(slash + 1);

                        entry = key.getNamed(nextEntry);
                        
                    }
                    else retentry = entry;
                }
                return entry;
            }
            catch (Exception e)
            {
                return null;
            }

        }
        public Entry getAtPath(string path)
        {
            try
            {

                // Remove the root/ text
                path = path.Substring(path.IndexOf('/') + 1);

                Entry retentry = null;
                Entry entry = MyRoot; // Get root
                while (retentry == null)
                {
                    int slash = path.IndexOf("/");
                    string nextEntry = "";
                    if (slash != -1)
                        nextEntry = path.Substring(0, slash);
                    else
                    {
                        nextEntry = path;
                        path = "";
                    }



                    if (entry is Key key && nextEntry != "")
                    {
                        path = path.Substring(slash + 1);
                        entry = key.getNamed(nextEntry);
                    }
                    else retentry = entry;
                }
                return entry;
            }
            catch (Exception e)
            {
                // The entry wasn't found, generate a new key, place at path, and return
                return null;
            }

        }

        public virtual void Write(BinaryWriter stream)
        {
            if (Parent is EntryList<Entry>) return;
            
            stream.Write((byte)Type);
            stream.Write(Name);

            if (Type == EntryType.Root)
            {
                stream.Write(EncodeDescription);
            }
            if(encodesDescription())
                stream.Write(Description);

        }

        public bool encodesDescription()
        {
            return MyRoot.EncodeDescription;
        }

        public static Entry? Read(BinaryReader stream, Key root, bool isList=false, EntryType listType = EntryType.Empty, bool skipEncDesc = false)
        {
            Entry x = null;
            EntryType Type = listType;
            string Name = "";
            string Description = "";
            bool EncodeDesc=true;
            if (!isList)
            {
                Type = (EntryType)stream.ReadByte();
                Name = stream.ReadString();
                if(Type == EntryType.Root && !skipEncDesc)
                {
                    EncodeDesc = stream.ReadBoolean();
                }
                if (skipEncDesc)
                {
                    EncodeDesc = true;
                }
                if(root == null)
                {
                    if(EncodeDesc) Description = stream.ReadString();
                }
                if(root != null && root.EncodeDescription)
                    Description = stream.ReadString();


            }

            switch(Type)
            {
                case EntryType.Key:
                    {
                        x = new Key(Name);
                        break;
                    }
                case EntryType.Word:
                    {
                        x = new Word(Name, "");
                        break;
                    }
                case EntryType.Int16:
                    {
                        x = new VInt16(Name, 0);
                        break;
                    }
                case EntryType.Int32:
                    {
                        x = new VInt32(Name, 0);
                        break;
                    }
                case EntryType.Int64:
                    {
                        x = new VInt64(Name, 0);
                        break;
                    }
                case EntryType.Bool:
                    {
                        x = new VBool(Name, false);
                        break;
                    }
                case EntryType.Byte:
                    {
                        x = new VByte(Name, 0);
                        break;
                    }
                case EntryType.Root:
                    {
                        x = new Key("root");
                        break;
                    }
                case EntryType.Array:
                    {
                        x = new EntryList<Entry>(Name);
                        break;
                    }
                case EntryType.WordArray:
                    {
                        x = new WordList(Name);
                        break;
                    }
                default:
                    {
                        x = new BlankEntry(Name);
                        break;
                    }
            }
            x.Type = Type;
            x.Description = Description;
            x.EncodeDescription = EncodeDesc;
            x.Parent = null;

            if(x.Type == EntryType.Root)
            {
                x.MyRoot = x.Key();
            }

            x.readValue(stream);
            

            return x;
        }

        public abstract void readValue(BinaryReader stream);

        private static Key getRoot(Entry entry)
        {
            var ent = entry;
            while(ent.Parent != null)
            {
                ent = ent.Parent;
            }

            return ent as Key;
        }

        public Word Word()
        {
            return (Word)this;
        }
        public VInt16 Int16()
        {
            return (VInt16)this;
        }
        public VInt32 Int32()
        {
            return (VInt32)this;
        }
        public VInt64 Int64()
        {
            return (VInt64)this;
        }
        public VBool Bool()
        {
            return (VBool)this;
        }
        public Key Key()
        {
            return (Key)this;
        }
        public VByte Byte()
        {
            return (VByte)this;
        }

        public EntryList<T> Array<T>() where T : Entry
        {
            return (EntryList<T>)this;
        }

        public WordList WordList()
        {
            return (WordList)this;
        }

        /// <summary>
        /// The root key for the current tree
        /// </summary>
        [JsonIgnore()]
        public Key MyRoot;

        public virtual string PrettyPrint(int indent=0)
        {
            return $"{"".PadLeft(indent, ' ')}[{Type}] {Name}";
        }

        public override string ToString()
        {
            return PrettyPrint();
        }

        /// <summary>
        /// Deletes the registry entry at a specified path.
        /// </summary>
        /// <param name="path">Absolute path to delete</param>
        /// <returns>The parent key</returns>
        public Entry deleteByPath(string path)
        {
            if (this is Key key)
            {

                Entry pth = getAtPath(path);
                Key parent = pth.Parent.Key();
                parent.Remove(pth);

                return parent;
            }
            else return null;
        }

        public virtual void setValue(object value)
        {

        }

        /// <summary>
        /// Sets the entry to be completely empty.
        /// </summary>
        public void clear()
        {
            Type = EntryType.Empty;
        }
    }

    public class BlankEntry : Entry
    {
        public BlankEntry(string name) : base(EntryType.Empty, name)
        {

        }
        public override void readValue(BinaryReader stream)
        {
            // done
        }


    }
}

﻿using TP.CS.EventsBus;
using TP.CS.EventsBus.Events;
using System;
using System.IO;
using System.Reflection;
using System.IO.Enumeration;

namespace TP.CS.Registry
{
    public class RegistryIO
    {
        public const byte Version = 2;
        public const byte Version2 = 4;

        public static string GetRegistryGlobalPath(string name, string database)
        {
            string filename = name;

            // Create a registry folder in the user profile
            string globalAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            globalAppData = Path.Combine(globalAppData, Embed.HSRDGlobalStorage);
            if (!Directory.Exists(globalAppData)) Directory.CreateDirectory(globalAppData);
            globalAppData = Path.Combine(globalAppData, database);
            if (!Directory.Exists(globalAppData)) Directory.CreateDirectory(globalAppData);

            filename = Path.Combine(globalAppData, filename);
            filename = Path.ChangeExtension(filename, HSRDExtension);
            return filename;
        }

        /// <summary>
        /// Saves the entire Registry to disk
        /// 
        /// This uses the Entry.ROOT object. To use another registry, see the saveHive function.
        /// </summary>
        public static void save(string database = "%nset")
        {
            if(database == "%nset")
            {
                database = Assembly.GetExecutingAssembly().GetName().Name;
                if (database == null) database = "Global";
                
            }
            string filename = GetRegistryGlobalPath(RootHSRD, database);
            

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Saving Registry : \n\nByte Count: {getBytes(Entry.ROOT).Length}\n{Entry.ROOT.PrettyPrint()}");

            Console.ForegroundColor = ConsoleColor.DarkGreen;


            // Reset the file to zero bytes if it exists
            resetFile(filename);
            using(FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                using(BinaryWriter bw = new BinaryWriter(fs))
                {
                    writeHeaderV2(bw);

                    Entry.ROOT.setRoot(Entry.ROOT);
                    Entry.ROOT.Write(bw);
                }
            }

            Console.WriteLine("Registry Saved");
        }

        /// <summary>
        /// Saves the specified Root Hive to disk
        /// 
        /// This requires a file name!
        /// </summary>
        /// <param name="root"></param>
        public static void saveHive(Key root, string name, string database = "%nset")
        {

            if (database == "%nset")
            {
                database = Assembly.GetExecutingAssembly().GetName().Name;
                if (database == null) database = "Global";

            }
            string filename = GetRegistryGlobalPath(name, database);


            Console.ForegroundColor = ConsoleColor.DarkRed;

            if(!EventBus.debug)
                Console.WriteLine($"Saving Registry {name} : \n\nByte Count: {getBytes(root).Length}\n{root.PrettyPrint()}");

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            // Reset the file to zero bytes if it exists
            resetFile(filename);

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                using(BinaryWriter bw = new BinaryWriter(fs))
                {
                    writeHeaderV2(bw);

                    root.setRoot(root);
                    root.Write(bw);
                }
            }

            if(!EventBus.debug)
                Console.WriteLine("Registry Saved");
        }

        public static void saveHive(Key root, Stream stream)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;

            if (!EventBus.debug)
                Console.WriteLine($"Saving Registry : \n\nByte Count: {getBytes(root).Length}\n{root.PrettyPrint()}");

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            using (BinaryWriter bw = new BinaryWriter(stream))
            {
                writeHeaderV2(bw);

                root.setRoot(root);
                root.Write(bw);
            }
            if (!EventBus.debug)
                Console.WriteLine("Registry Saved");
        }

        public static byte[] saveWithoutHeader (Key root)
        {
            MemoryStream ms = new MemoryStream();

            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                root.Write(bw);
            }

            return ms.ToArray();
        }

        public static Key loadWithoutHeader(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);

            using (BinaryReader br = new BinaryReader(ms))
            {
                return Entry.Read(br, null).Key();
            }

        }

        public static byte[] getBytes(Key root)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    writeHeaderV2(bw);

                    root.Write(bw);

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Write the header version 1 to the file
        /// 
        /// Header v1 is 31 bytes total, but can vary depending on the byte count of Creator
        /// </summary>
        /// <param name="bw"></param>
        private static void writeHeaderV1(BinaryWriter bw)
        {
            // Write out header!
            bw.Write((byte)1);
            bw.Write(Version2); // 2
            bw.Write(Embed.Creator); // 1 + 12
            bw.Write(new byte[16]); // 16

            // 16 bytes of padding for minor version changes. Potential bitmasks may be added.
            // Minor version upgrades should make padding changes when necessary during an upgrade to maintain compatibility when a major update arrives.
        }

        /// <summary>
        /// Writes header version 2 to the file
        /// 
        /// Header v2 is 128 bytes total
        /// Unlike v1, this is fixed in length.
        /// </summary>
        /// <param name="bw"></param>
        private static void writeHeaderV2(BinaryWriter bw)
        {
            MemoryStream MS = new MemoryStream(new byte[128], 0, 128, true);

            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                writer.Write((byte)2);
                writer.Write(Version2);
                writer.Write(Embed.Extension);
                writer.Write(Embed.Creator);

                // Date & Time saved at
                writer.Write(DateTime.UnixEpoch.Second);
                

            }

            bw.Write(MS.ToArray());
        }

        /// <summary>
        /// Loads the Root Hive into memory
        /// </summary>
        public static void load(string database = "%nset")
        {

            string filename = Path.Combine(DataFolder, RootHSRD);
            filename = Path.ChangeExtension(filename, HSRDExtension);

            if(File.Exists(filename))
            {
                // Migrate to new location
                File.Move(filename, GetRegistryGlobalPath(RootHSRD, database));
            }

            filename = GetRegistryGlobalPath(RootHSRD, database);


            Entry.ROOT.Type = EntryType.Root;
            if (File.Exists(filename))
            {

                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        byte ver1 = br.ReadByte();

                        switch (ver1)
                        {
                            case 1:
                                {
                                    readHeaderV1(br);
                                    break;
                                }
                            case 2:
                                {
                                    readHeaderV2(br);
                                    break;
                                }
                        }

                        Entry.ROOT.replaceEntries(Entry.Read(br, null));
                    }
                }
            }
            Entry.ROOT.setRoot(Entry.ROOT);


            Console.WriteLine("Registry Loaded.");
        }

        private static void readHeaderV1(BinaryReader br)
        {

            if (br.ReadByte() != Version2)
            {
                // We should be okay, but print a warning to the console. The format will be migrated during saving if there are any differences

                Console.WriteLine("WARNING: Registry minor version is different, there may be some missing header data. If your registry file fails to load report it to the developer");
                Console.WriteLine("The registry will be migrated to the new minor version upon being flushed to disk");
            }
            _ = br.ReadString(); // 13 bytes. Creator's signature.


            br.ReadBytes(16);
        }

        private static bool skipEncodeDescription = false;
        private static void readHeaderV2(BinaryReader br)
        {
            byte[] header = br.ReadBytes(127); // First byte was read already.
            using (MemoryStream ms = new MemoryStream(header))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    byte v2 = header[0];
                    switch (v2)
                    {
                        case 0:
                        case 1:
                        case 2:
                            {
                                // V2.3 wont skip the encoded description once the version is appropriately bumped.
                                skipEncodeDescription = true;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the specified custom Hive into memory
        /// </summary>
        public static Key loadHive(string name, string database = "%nset")
        {

            string filename = Path.Combine(DataFolder, name);
            filename = Path.ChangeExtension(filename, HSRDExtension);

            if (File.Exists(filename))
            {
                File.Move(filename, GetRegistryGlobalPath(name, database));
            }
            filename = GetRegistryGlobalPath(name, database);


            Key x = new Key("root");
            x.Type = EntryType.Root;
            if (File.Exists(filename))
            {

                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        byte ver1 = br.ReadByte();

                        switch (ver1)
                        {
                            case 1:
                                {
                                    readHeaderV1(br);
                                    break;
                                }
                            case 2:
                                {
                                    readHeaderV2(br);
                                    break;
                                }
                        }

                        x.replaceEntries(Entry.Read(br, null, skipEncDesc : (skipEncodeDescription ? true : false)));
                    }
                }
            }

            x.setRoot(x);
            skipEncodeDescription = false;

            return x;
        }


        /// <summary>
        /// Loads the specified custom Hive into memory
        /// </summary>
        public static Key loadHive(Stream hsrd)
        {

            Key x = new Key("root");
            x.Type = EntryType.Root;

            using (BinaryReader br = new BinaryReader(hsrd))
            {
                byte ver1 = br.ReadByte();

                switch (ver1)
                {
                    case 1:
                        {
                            readHeaderV1(br);
                            break;
                        }
                    case 2:
                        {
                            readHeaderV2(br);
                            break;
                        }
                }

                x.replaceEntries(Entry.Read(br, null, skipEncDesc: (skipEncodeDescription ? true : false)));
            }

            x.setRoot(x);
            skipEncodeDescription = false;

            return x;
        }

        private static void ensureFolder()
        {
            if(!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }
        }

        private static void resetFile(string filename)
        {
            File.WriteAllBytes(filename, new byte[0] { });
        }

        // HSRD
        // Harbinger Serialized Registry Data

        public static string RootHSRD = Embed.DefaultFileName;
        public static string DataFolder = Embed.RegistryFolder;

        public static string HSRDExtension = Embed.Extension;


        [Subscribe(Priority.Severe)]
        public static void onStartup(StartupEvent ev)
        {
            Console.WriteLine("Loading Registry...");
            load();


            EventBus.Broadcast(new RegistryLoadedEvent(Entry.ROOT));
        }



        [Subscribe(Priority.Low)]
        public static void onShutdown(ShutdownEvent ev)
        {
            Console.WriteLine("Flushing Registry...");
            if(!EventBus.Broadcast(new RegistrySavedEvent(Entry.ROOT, RootHSRD)))
                save();
        }

    }
}

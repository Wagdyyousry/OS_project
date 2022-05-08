using System;

using System.Reflection;

using System.IO;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;
namespace osProj
{
    public class Fat_Tabel
    {
        public static int[] fatTabel = new int[1024];
        public static void intialize()
        {
            fatTabel[0] = -1;
            fatTabel[1] = 2;
            fatTabel[2] = 3;
            fatTabel[3] = 4;
            fatTabel[4] = -1;


        }
        public static void testing()
        {
            int[] F = Read_fat();

            Console.WriteLine("index" + "            " + "Next");
            for (int i = 0; i < 1024; i++)
            {
                Console.WriteLine(i + "            " + F[i]);
            }
        }
        public static void write_fat()
        {
            string path = Directory.GetCurrentDirectory() + @"\\Virtual_disk.txt";
            FileStream wt = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            wt.Seek(1024, SeekOrigin.Begin);
            Byte[] bt = new Byte[1024 * 4];
            Buffer.BlockCopy(fatTabel, 0, bt, 0, bt.Length);
            wt.Write(bt, 0, bt.Length);
            wt.Close();


        }
        public static int[] Read_fat()
        {
            string path = Directory.GetCurrentDirectory() + @"\\Virtual_disk.txt";
            FileStream rd = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            rd.Seek(1024, SeekOrigin.Begin);
            Byte[] bt = new Byte[1024 * 4];
            rd.Read(bt, 0, bt.Length);
            rd.Close();
            Buffer.BlockCopy(bt, 0, fatTabel, 0, fatTabel.Length);


            return fatTabel;
        }

        public static int getNext(int index)
        {
            return fatTabel[index];
        }
        public static void setNext(int index, int value)
        {
            fatTabel[index] = value;

        }
        public static int getAvaliablIndex()
        {
            int[] S = Read_fat();
            for (int i = 0; i < 1024; i++)
            {
                if (S[i] == 0)
                {
                    return i;
                }

            }
            return -1;


        }
        public static int getAvaliableBlock()
        {
            int[] S = Read_fat();
            int no = 0;
            for (int i = 0; i < 1024; i++)
            {
                if (S[i] == 0)
                {
                    no++;
                }

            }
            return (no == 0 ? -1 : no);
        }

        public static int get_free_space()
        {

            return getAvaliableBlock() * 1024;
        }
    }
    class Virtual_disk
    {

        public void intialize()
        {
            string path = Directory.GetCurrentDirectory() + @"\\Virtual_disk.txt";
            FileInfo Virtual_disk_txt = new FileInfo(path);
            directory root = new directory("H", 1, 5, 0, null);
            Fat_Tabel.setNext(5, -1);
            Program.c_dic = root;
            if (File.Exists(path))
            {

                Fat_Tabel.fatTabel = Fat_Tabel.Read_fat();

                root.Read_directory();


            }
            else
            {

                FileStream wt = Virtual_disk_txt.Open(FileMode.Create, FileAccess.ReadWrite);
                for (int i = 0; i < 1024; i++)
                {
                    wt.WriteByte(0);
                }
                for (int i = 0; i < 4 * 1024; i++)
                {
                    wt.WriteByte((byte)'*');
                }
                for (int i = 0; i < 1019 * 1024; i++)
                {
                    wt.WriteByte((byte)'#');
                }
                wt.Close();
                Fat_Tabel.intialize();
                root.write_directory();
                Fat_Tabel.write_fat();
            }
        }
        public static void write_block(byte[] data, int index)
        {

            string path = Directory.GetCurrentDirectory() + @"\\Virtual_disk.txt";

            FileStream Virtual_disk_text = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            Virtual_disk_text.Seek(1024 * index, SeekOrigin.Begin);
            Virtual_disk_text.Write(data, 0, data.Length);
            Virtual_disk_text.Close();

        }
        public static byte[] read_block(int index)
        {
            string path = Directory.GetCurrentDirectory() + @"\\Virtual_disk.txt";
            FileStream Virtual_disk_text = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            Virtual_disk_text.Seek(1024 * index, SeekOrigin.Begin);
            Byte[] bt = new Byte[1024];
            Virtual_disk_text.Read(bt, 0, bt.Length);
            Virtual_disk_text.Close();
            return bt;
        }


    }

    public class directory : DirectoryEntry
    {
        public List<DirectoryEntry> Directory_Table = new List<DirectoryEntry>();
        public directory perant;

        public directory()
        {

        }
        public directory(string n, byte attr, int fc, int size, directory p) : base(n, attr, fc, size)
        {
            this.perant = p;


        }



        public void write_directory()
        {
            byte[] DTB = new byte[32 * Directory_Table.Count];
            byte[] DEB = new byte[32];
            for (int i = 0; i < Directory_Table.Count; i++)
            {
                DEB = Directory_Table[i].getBytes();
                for (int j = i * 32; j < 32 * (i + 1); j++)
                {
                    DTB[j] = DEB[j % 32];
                }

            }
            int num_of_req_block = (int)Math.Ceiling(DTB.Length / 1024.0);
            int num_no_full_size_block = (DTB.Length / 1024);
            int remainder = DTB.Length % 1024;
            List<byte[]> blocks = new List<byte[]>();
            byte[] temp = new byte[1024];
            for (int i = 0; i < num_no_full_size_block; i++)
            {


                for (int j = 0; j < 1024; j++)
                {
                    temp[j] = DTB[j + i * 1024];
                }
                blocks.Add(temp);
            }
            int indexR = (num_no_full_size_block * 1024);

            for (int i = 0; i < remainder; i++, indexR++)
            {

                temp[i] = DTB[indexR];


            }
            if (remainder > 0)
            {
                blocks.Add(temp);
            }


            int fc = 0, lc = -1;
            if (firstCluster != 0)
            {
                fc = firstCluster;
            }
            else
            {
                fc = Fat_Tabel.getAvaliablIndex();
                firstCluster = fc;
            }

            for (int i = 0; i < num_of_req_block; i++)
            {
                Virtual_disk.write_block(blocks[i], fc);
                Fat_Tabel.setNext(fc, -1);
                if (lc != -1)
                {
                    Fat_Tabel.setNext(lc, fc);

                    fc = Fat_Tabel.getAvaliablIndex();
                }
                lc = fc;

            }
            Fat_Tabel.write_fat();




        }
        public void Read_directory()
        {
            List<byte> ls = new List<byte>();
            byte[] d = new byte[32];
            int fc = 0, Nc;
            if (firstCluster != 0)
            {
                fc = firstCluster;
            }
            Nc = Fat_Tabel.getNext(fc);
            do
            {
                ls.AddRange(Virtual_disk.read_block(fc));

                if (fc != -1)
                {
                    Nc = Fat_Tabel.getNext(fc);
                }
                fc = Nc;
            }
            while (fc != -1);
            bool flage = false;
            for (int i = 0; i < ls.Count / 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (ls[j + (i * 32)] == (byte)'#')
                    {
                        flage = true;
                        break;
                    }
                    d[j] = ls[j + (i * 32)];
                }
                if (flage)
                    break;
                if (GetDirectory(d).firstCluster != 0)
                    Directory_Table.Add(GetDirectory(d));
            }


        }


        public int Search_directory(string name)
        {


            Read_directory();

            for (int i = 0; i < Directory_Table.Count; i++)
            {
                string s = "";
                for (int j = 0; j < Directory_Table[i].Fname.Length; j++)
                {
                    if (Directory_Table[i].Fname[j] == '\0')
                    {
                        break;
                    }

                    s += Directory_Table[i].Fname[j];
                }

                if (s == name)
                {
                    return i;
                }

            }
            return -1;
        }


        public void Update_content(DirectoryEntry d)
        {

            Read_directory();
            int index = Search_directory(d.Fname.ToString());
            if (index != -1)
            {
                Directory_Table.RemoveAt(index);
                Directory_Table.Insert(index, d);
            }

        }

        public void delete_directory()
        {
            int index, next;
            if (firstCluster != 0)
            {
                index = firstCluster;
                next = Fat_Tabel.getNext(index);


                do
                {
                    Fat_Tabel.setNext(index, 0);
                    index = next;
                    if (index != -1)
                    {
                        next = Fat_Tabel.getNext(index);
                    }

                } while (index != -1);
            }
            if (perant != null)
            {
                perant.Directory_Table.Clear();
                //perant.Read_directory();
                string s = "";

                for (int c = 0; c < Fname.Length && Fname[c] != '\0'; c++)
                {
                    s += Fname[c];

                }
                index = perant.Search_directory(s);
                if (index != -1)
                {
                    perant.Directory_Table.RemoveAt(index);
                    perant.write_directory();
                }

            }
            Fat_Tabel.write_fat();

        }
        public directory Getinf()
        {
            directory d = new directory();
            d.Fname = Fname;
            d.fileSize = fileSize;
            d.file_Empty = file_Empty;
            d.firstCluster = firstCluster;
            d.attribute = attribute;
            d.perant = perant;
            return d;
        }

    }
    internal class StreamReade
    {
        private FileStream virtual_disk_text;

        public StreamReade(FileStream virtual_disk_text)
        {
            this.virtual_disk_text = virtual_disk_text;
        }
    }

    class Program
    {
        public static directory c_dic = new directory();
        public static string c_pos = "";

        static void Main(string[] args)
        {
            string[] commands = new string[11];
            string[] details = new string[11];
            List<KeyValuePair<int, int>> number_of_arguments = new List<KeyValuePair<int, int>>();
            intialize(commands, details, number_of_arguments);
            Virtual_disk disk1 = new Virtual_disk();
            disk1.intialize();
            for (int i = 0; i < c_dic.Fname.Length && c_dic.Fname[i] != '\0'; i++)
            {
                c_pos += c_dic.Fname[i];
            }
            Console.WriteLine(Fat_Tabel.getAvaliablIndex());
            Console.WriteLine(Fat_Tabel.getAvaliableBlock());
            // Fat_Tabel.testing();



            while (true)
            {

                get_dir();
                check(commands, details, number_of_arguments);

            }

        }
        public static string return_path()
        {
            return Directory.GetCurrentDirectory();
        }

        static void get_dir()
        {


            Console.Write(@"{0}:\>", c_pos);

        }


        static void intialize(string[] commands, string[] details, List<KeyValuePair<int, int>> number_of_arguments)
        {
            commands[0] = "cd";
            details[0] = "Change the current default directory to . If the argument is not present, report the current directory. \n             If the directory does not exist an appropriate error should be reported.";
            number_of_arguments.Add(new KeyValuePair<int, int>(1, 2));

            commands[1] = "cls";//done 
            details[1] = "Clear the screen.";
            number_of_arguments.Add(new KeyValuePair<int, int>(0, 1000000));


            commands[2] = "dir";
            details[2] = "List the contents of directory.";
            number_of_arguments.Add(new KeyValuePair<int, int>(0, 2));


            commands[3] = "quit";//done 
            details[3] = "Quit the shell.";
            number_of_arguments.Add(new KeyValuePair<int, int>(0, 1000000));


            commands[4] = "copy";
            details[4] = "Copies one or more files to another location";
            number_of_arguments.Add(new KeyValuePair<int, int>(2, 2));

            commands[5] = "del";
            details[5] = "Deletes one or more files.";
            number_of_arguments.Add(new KeyValuePair<int, int>(1, 1));


            commands[6] = "help";//done
            details[6] = "Provides Help information for commands.";
            number_of_arguments.Add(new KeyValuePair<int, int>(0, 1));


            commands[7] = "md";
            details[7] = "Creates a directory.";
            number_of_arguments.Add(new KeyValuePair<int, int>(1, 1000000));


            commands[8] = "rd";
            details[8] = "Removes a directory.";
            number_of_arguments.Add(new KeyValuePair<int, int>(1, 1000000));


            commands[9] = "rename";
            details[9] = "Renames a file.";
            number_of_arguments.Add(new KeyValuePair<int, int>(2, 2));

            commands[10] = "type";
            details[10] = "Displays the contents of a text file.";
            number_of_arguments.Add(new KeyValuePair<int, int>(1, 1));


        }
        static void help(string[] commands, string[] details, string s = "")
        {
            if (s == "")
            {
                for (int i = 0; i < 11; i++)
                {
                    Console.Write(commands[i]);
                    for (int j = commands[i].Length; j <= 12; j++) Console.Write(" ");
                    Console.WriteLine(details[i] + "\n");
                }
            }
            else
            {
                bool che = false;
                for (int j = 0; j < 11; j++)
                {
                    if (s == commands[j])
                    {
                        che = true;
                        Console.WriteLine(details[j]);
                        break;
                    }
                }
                if (!che)
                {
                    Console.WriteLine("not vaild command");
                }
            }
        }
        static void check(string[] commands, string[] details, List<KeyValuePair<int, int>> number_of_arguments)
        {
            string order = Console.ReadLine();
            int i = 0;
            List<string> arguments = new List<string>();
            string temp;

            while (i < order.Length)
            {
                temp = "";
                while (i < order.Length && order[i] == ' ')
                {
                    i++;
                }
                while (i < order.Length && order[i] != ' ')
                {
                    temp += order[i];
                    i++;
                }
                arguments.Add(temp);


            }
            if (arguments.Count == 0)
            {

            }
            else
            {
                bool che = false;
                int index = 0;

                for (int j = 0; j < 11; j++)
                {
                    if (commands[j] == arguments[0] && (arguments.Count - 1 >= number_of_arguments[j].Key && arguments.Count - 1 <= number_of_arguments[j].Value))
                    {
                        che = true;
                        index = j;
                        break;
                    }
                }
                if (che)
                {

                    if (commands[index] == "quit")
                    {
                        Environment.Exit(0);
                    }
                    else if (commands[index] == "cls")
                    {
                        Console.Clear();
                    }

                    else if (commands[index] == "help")
                    {
                        if (arguments.Count == 1)
                            help(commands, details);
                        else
                            help(commands, details, arguments[1]);

                    }

                    else if (commands[index] == "md")
                    {

                        int pos = c_dic.Search_directory(arguments[1]);
                        c_dic.Directory_Table.Clear();
                        c_dic.Read_directory();

                        if (pos == -1)
                        {

                            DirectoryEntry d = new DirectoryEntry(arguments[1], 1, 0, 0);
                            c_dic.Directory_Table.Add(d);
                            c_dic.write_directory();
                            Fat_Tabel.setNext(d.firstCluster, -1);

                            Fat_Tabel.write_fat();

                        }
                        else
                        {
                            Console.WriteLine("Already Exist");
                        }
                        if (c_dic.perant != null)
                        {
                            c_dic.perant.Update_content(c_dic.Getinf());
                            c_dic.write_directory();
                        }
                        Console.WriteLine(Fat_Tabel.getAvaliablIndex());
                        Console.WriteLine(Fat_Tabel.getAvaliableBlock());
                        // Fat_Tabel.testing();
                    }

                    else if (commands[index] == "dir")
                    {
                        c_dic.Directory_Table.Clear();
                        c_dic.Read_directory();
                        int fl_no = 0, dir_no = 0, fl_size = 0;
                        for (int j = 0; j < c_dic.Directory_Table.Count; j++)
                        {
                            string s = "";

                            for (int c = 0; c < c_dic.Directory_Table[j].Fname.Length; c++)
                            {
                                s += c_dic.Directory_Table[j].Fname[c];

                            }
                            if (c_dic.Directory_Table[j].attribute == 1)
                            {
                                Console.WriteLine("     <DIR>    " + s + " " + c_dic.Directory_Table[j].firstCluster);
                                dir_no++;
                            }
                            else
                            {
                                fl_size += c_dic.Directory_Table[j].fileSize;
                                fl_no++;
                            }

                        }
                        Console.WriteLine("     " + fl_no + " File(s)" + "     " + fl_size + " bytes");
                        Console.WriteLine("     " + dir_no + " Dir(s)" + "     " + (Fat_Tabel.get_free_space() - fl_size) + " Free bytes");

                    }

                    else if (commands[index] == "cd")
                    {



                        int pos = c_dic.Search_directory(arguments[1]);
                        if (pos != -1)
                        {
                            if (c_dic.Directory_Table[pos].attribute == 1)
                            {
                                int fc = c_dic.Directory_Table[pos].firstCluster;
                                directory d = new directory(arguments[1], 1, fc, 0, c_dic);
                                d.write_directory();


                                c_dic = d;
                                string s = "";
                                for (int j = 0; j < c_dic.Fname.Length && c_dic.Fname[j] != '\0'; j++)
                                {
                                    s += c_dic.Fname[j];
                                }
                                c_pos += "\\" + s;
                            }
                            else
                            {
                                Console.WriteLine("Can't change current directory ");
                            }
                        }
                        else
                        {
                            string r = "";
                            if (c_dic.perant != null)
                            {

                                for (int j = 0; j < c_dic.perant.Fname.Length && c_dic.perant.Fname[j] != '\0'; j++)
                                {
                                    r += c_dic.perant.Fname[j];
                                }

                            }

                            if (arguments[1] == r)
                            {
                                directory d = c_dic.perant;
                                d.Read_directory();
                                c_dic = d;
                                while (c_pos[c_pos.Length - 1] != '\\')
                                {

                                    c_pos = c_pos.Remove(c_pos.Length - 1);
                                }
                                c_pos = c_pos.Remove(c_pos.Length - 1);

                            }
                            else
                            {
                                Console.WriteLine("Not Exist");
                            }
                        }


                    }
                    else if (commands[index] == "rd")
                    {
                        c_dic.Directory_Table.Clear();
                        int pos = c_dic.Search_directory(arguments[1]);
                        if (pos != -1)
                        {
                            if (c_dic.Directory_Table[pos].attribute == 1)
                            {
                                int fc = c_dic.Directory_Table[pos].firstCluster;
                                directory d = new directory(arguments[1], 1, fc, 0, c_dic);
                                d.write_directory();
                                d.delete_directory();




                            }
                        }
                        else
                        {
                            Console.WriteLine("Not Exist");
                        }
                    }


                    else
                        Console.WriteLine("not vaild command");
                }

            }
        }
    }
    class File_Entry : directory
    {
        directory parent;
        string content;
        public File_Entry(string n, byte attr, int fc, int size, directory p, string c) : base(n, attr, fc, size, p)
        {
            this.perant = p;
            this.content = c;

        }
        public void write_directory()
        {
            byte[] DTB = new byte[32 * Directory_Table.Count];
            byte[] DEB = new byte[32];
            for (int i = 0; i < Directory_Table.Count; i++)
            {
                DEB = Directory_Table[i].getBytes();
                for (int j = i * 32; j < 32 * (i + 1); j++)
                {
                    DTB[j] = DEB[j % 32];
                }

            }
            int num_of_req_block = (int)Math.Ceiling(DTB.Length / 1024.0);
            int num_no_full_size_block = (DTB.Length / 1024);
            int remainder = DTB.Length % 1024;
            List<byte[]> blocks = new List<byte[]>();
            byte[] temp = new byte[1024];
            for (int i = 0; i < num_no_full_size_block; i++)
            {


                for (int j = 0; j < 1024; j++)
                {
                    temp[j] = DTB[j + i * 1024];
                }
                blocks.Add(temp);
            }
            int indexR = (num_no_full_size_block * 1024);

            for (int i = 0; i < remainder; i++, indexR++)
            {

                temp[i] = DTB[indexR];


            }
            if (remainder > 0)
            {
                blocks.Add(temp);
            }


            int fc = 0, lc = -1;
            if (firstCluster != 0)
            {
                fc = firstCluster;
            }
            else
            {
                fc = Fat_Tabel.getAvaliablIndex();
                firstCluster = fc;
            }

            for (int i = 0; i < num_of_req_block; i++)
            {
                Virtual_disk.write_block(blocks[i], fc);
                Fat_Tabel.setNext(fc, -1);
                if (lc != -1)
                {
                    Fat_Tabel.setNext(lc, fc);

                    fc = Fat_Tabel.getAvaliablIndex();
                }
                lc = fc;

            }
            Fat_Tabel.write_fat();




        }







    }

    public class DirectoryEntry
    {

        public char[] Fname = new char[11];
        public byte attribute;     //0 file		//1 folder
        public byte[] file_Empty = new byte[12];
        public int firstCluster;
        public int fileSize;

        public DirectoryEntry()
        {

        }
        public DirectoryEntry(string n, byte attr, int fc, int size)
        {
            string name = n;
            attribute = attr;
            fileSize = size;
            if (fc == 0)
            {
                fc = Fat_Tabel.getAvaliablIndex();
                firstCluster = fc;
            }
            else
            {
                firstCluster = fc;
            }

            // check that the file name contains .
            if (attribute == 0)
            {
                if (n.Length > 11)
                {
                    name = n.Substring(0, 7) + n.Substring(n.Length - 4);
                }
                else
                {
                    name = n;
                }

            }
            else
            {

                name = n.Substring(0, Math.Min(11, n.Length));
            }
            for (int i = 0; i < name.Length; i++)
            {
                Fname[i] = name[i];


            }
        }

        public byte[] getBytes()
        {
            byte[] b = new byte[32];
            for (int i = 0; i < Fname.Length; i++)
            {

                b[i] = Convert.ToByte(Fname[i]);
            }
            b[11] = attribute;
            for (int i = 12; i < 24; i++)
            {
                b[i] = file_Empty[i - 12];
            }
            Byte[] bt = new Byte[4];
            bt = BitConverter.GetBytes(firstCluster);
            for (int i = 24; i < 28; i++)
            {
                b[i] = bt[i - 24];
            }
            bt = BitConverter.GetBytes(fileSize);
            for (int i = 28; i < 32; i++)
            {
                b[i] = bt[i - 28];
            }

            return b;
        }
        public DirectoryEntry GetDirectory(byte[] b)
        {
            List<byte[]> bt = new List<byte[]>();
            List<byte> bs = new List<byte>();
            for (int i = 0; i < 11; i++)
            {

                bs.Add(b[i]);
            }
            bt.Add(bs.ToArray());
            bs.Clear();
            bs.Add(b[11]);
            bt.Add(bs.ToArray());
            bs.Clear();
            for (int i = 12; i < 24; i++)
            {
                bs.Add(b[i]);
            }
            bt.Add(bs.ToArray());
            bs.Clear();
            for (int i = 24; i < 28; i++)
            {
                bs.Add(b[i]);
            }
            bt.Add(bs.ToArray());
            bs.Clear();
            for (int i = 28; i < 32; i++)
            {
                bs.Add(b[i]);
            }
            bt.Add(bs.ToArray());
            DirectoryEntry c = new DirectoryEntry();
            c.Fname = Encoding.ASCII.GetString(bt[0]).ToCharArray();
            c.attribute = bt[1][0];
            c.file_Empty = bt[2];
            byte[] ba = new byte[4];
            ba = bt[3];
            c.firstCluster = BitConverter.ToInt32(ba, 0);
            ba = bt[4];
            c.fileSize = BitConverter.ToInt32(ba, 0);


            return c;
        }

    }
}
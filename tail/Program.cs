using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Mono.Options;

namespace tail
{
    class Program
    {
        static void Main(string[] args)
        {
            bool tail_mode = false;
            String filename = null;
            int lines = 0;
            OptionSet p = new OptionSet()
            {
                {"f", "Follow the file", f => { tail_mode = true; } },
                {"n=","Number of lines to print", (int n) => lines=n  }
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            if(extra.Count <= 0)
            {
                Console.WriteLine("Filename expected");
                return;
            }
            else
            {
                filename = extra[0];
            }

            StreamReader sr = new StreamReader(filename);
            sr.Peek();
            encoding = sr.CurrentEncoding;
            sr.Close();

            if (tail_mode)
            {
                watcher = new FileSystemWatcher();
                watcher.Path = Path.GetDirectoryName(filename);
                watcher.Filter = Path.GetFileName(filename);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += new FileSystemEventHandler(OnChange);
                tail_file(filename);
            }
            if (lines > 0)
            {
                last_lines(filename, lines);
            }
            else
            {
                last_lines(filename, 10);
            }
            return;
        }
        static int count_lines(String s)
        {
            int count = 0;
            foreach (char c in s)
            {
                //Console.Write(s[i]);
                if (c == '\n')
                {
                    count++;
                }
            }
            return count;
        }
        static void last_lines(String filename, int n)
        {
            String s;
            FileStream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long buf_sz = 512;
            byte[] buf = new byte[buf_sz];
            
            file.Read(buf, 0, (int)buf_sz);
            s = encoding.GetString(buf);
            if (file.Position == file.Length)
            {
                print_last_lines(s, n);
                return;
            }

            file.Seek(buf_sz * -1, SeekOrigin.End);
            file.Read(buf, 0, (int)buf_sz);
            s = encoding.GetString(buf);
            int count = count_lines(s);
            while(count < n)
            {
                file.Seek(buf_sz * -1, SeekOrigin.Current);
                if(file.Position < buf_sz)
                {
                    long pos = file.Position;
                    file.Seek(0, SeekOrigin.Begin);
                    file.Read(buf, 0, (int)pos);
                    s = encoding.GetString(buf) + s;
                    print_last_lines(s, n);
                    file.Close();
                    return;
                }
                file.Seek(buf_sz * -1, SeekOrigin.Current);
                file.Read(buf, 0, (int)buf_sz);
                s = encoding.GetString(buf) + s;
                count = count_lines(s);
            }
            file.Close();
            print_last_lines(s, n);
            return;
        }

        static void print_last_lines(String s, int n)
        {
            String[] a = s.Split('\n');
            if(a.Length < n )
            {
                Console.Write(s);
                return;
            }
            int start = a.Length - n;
            for(int i = start; i<a.Length;i++)
            {
                Console.WriteLine(a[i]);
            }
            return;
        }

        static void tail_file(String filename)
        {
            fs = null;
            FileStream file;
            try
            {
                file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (file.Length > 1024)
                {
                    file.Seek(-1024, SeekOrigin.End);
                    Console.Write("...");
                }
                fs = new StreamReader(file, encoding);
                Console.Write(fs.ReadToEnd());
                last_position = file.Position-(encoding.GetByteCount("a"));
                watcher.EnableRaisingEvents = true;
                file.Close();
                fs.Close();
                while (true)
                {
                    Console.Read();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Cannot open file " + filename);
            }
            finally
            {
                if(fs != null)
                {
                    fs.Close();
                }
            }
        }

        static void OnChange(object source, FileSystemEventArgs e)
        {
            FileStream stream = File.Open(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if(last_position > stream.Length)
            {
                if(stream.Length > 1024)
                {
                    stream.Seek(-1024, SeekOrigin.End);
                }
            }
            else
            {
                stream.Seek(last_position, 0);
            }
            fs = new StreamReader(stream, encoding);
            Console.Write(fs.ReadToEnd());
            last_position = stream.Position;
            stream.Close();
            fs.Close();
        }

        static StreamReader fs;
        static FileSystemWatcher watcher;
        static long last_position;
        static Encoding encoding;
    }
}

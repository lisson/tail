using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace tail
{
    class Tail
    {
        public Tail(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            sr.Peek();
            encoding = sr.CurrentEncoding;
            sr.Close();
        }

        private void OnChange(object source, FileSystemEventArgs e)
        {
            FileStream stream = File.Open(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (last_position > stream.Length)
            {
                if (stream.Length > 1024)
                {
                    stream.Seek(-1024, SeekOrigin.End);
                }
            }
            else
            {
                stream.Seek(last_position, 0);
            }
            this.fs = new StreamReader(stream, encoding);
            Console.Write(this.fs.ReadToEnd());
            last_position = stream.Position;
            stream.Close();
            this.fs.Close();
        }

        private void print_last_lines(String s, int n)
        {
            String[] a = s.Split('\n');
            if (a.Length < n)
            {
                Console.Write(s);
                return;
            }
            int start = a.Length - n;
            for (int i = start; i < a.Length; i++)
            {
                Console.WriteLine(a[i]);
            }
            return;
        }

        public void tail_file(String filename)
        {
            if(watcher == null)
            {
                watcher = new FileSystemWatcher();
                watcher.Path = Path.GetDirectoryName(filename);
                watcher.Filter = Path.GetFileName(filename);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += new FileSystemEventHandler(this.OnChange);
            }

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
                last_position = file.Position - (encoding.GetByteCount("a"));
                watcher.EnableRaisingEvents = true;
                file.Close();
                fs.Close();
                while (true)
                {
                    Console.Read();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Cannot open file " + filename);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public int count_lines(String s)
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

        public void last_lines(String filename, int n)
        {
            if(n==0)
            {
                return;
            }
            if(n<0)
            {
                n = 10;
            }
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
            while (count < n)
            {
                file.Seek(buf_sz * -1, SeekOrigin.Current);
                if (file.Position < buf_sz)
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


        private StreamReader fs;
        private FileSystemWatcher watcher;
        private long last_position;
        private Encoding encoding;
    }
}

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
            OptionSet p = new OptionSet()
            {
                {"f", "Keep tailing the file", f => { tail_mode = true; } },
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
            if (tail_mode)
            {
                watcher = new FileSystemWatcher();
                watcher.Path = Path.GetDirectoryName(filename);
                watcher.Filter = Path.GetFileName(filename);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += new FileSystemEventHandler(OnChange);
                tail_file(filename);
            }
            return;
        }

        static async void tail_file(String filename)
        {
            fs = null;
            FileStream file;
            try
            {
                file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fs = new StreamReader(file);                
                Console.Write(fs.ReadToEnd());
                last_position = file.Position-Encoding.Unicode.GetByteCount("a");
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
            stream.Seek(last_position, 0);
            fs = new StreamReader(stream, Encoding.Unicode);
            Console.Write(fs.ReadToEnd());
            last_position = stream.Position;
            stream.Close();
            fs.Close();
        }

        static StreamReader fs;
        static FileSystemWatcher watcher;
        static long last_position;
    }
}

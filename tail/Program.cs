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

            Tail t = new Tail(filename);

            if (tail_mode)
            {
                t.tail_file(filename);
            }
            t.last_lines(filename, lines);

            return;
        }
    }
}

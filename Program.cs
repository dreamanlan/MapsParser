using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace MapsParser
{
    class MapsInfo
    {
        internal ulong vm_start = 0;
        internal ulong vm_end = 0;
        internal string flags = string.Empty;
        internal string offset = string.Empty;
        internal string file1 = string.Empty;
        internal string file2 = string.Empty;
        internal string module = string.Empty;
    }
    class GroupInfo
    {
        internal string module = string.Empty;
        internal ulong start = ulong.MaxValue;
        internal ulong end = ulong.MinValue;
        internal ulong size = 0;
    }
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("Usage:MapsParser maps.txt");
                return;
            }
            var file = args[0];
            var groupDict = new Dictionary<string, GroupInfo>();
            var lines = File.ReadAllLines(file);
            foreach(var line in lines) {
                var mapsInfo = new MapsInfo();
                var fields = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var se = fields[0].Split('-');
                ulong start = ulong.Parse(se[0], System.Globalization.NumberStyles.AllowHexSpecifier);
                ulong end = ulong.Parse(se[1], System.Globalization.NumberStyles.AllowHexSpecifier);
                mapsInfo.vm_start = start;
                mapsInfo.vm_end = end;
                for(int i = 0; i < fields.Length; ++i) {
                    switch (i) {
                        case 1:
                            mapsInfo.flags = fields[i];
                            break;
                        case 2:
                            mapsInfo.offset = fields[i];
                            break;
                        case 3:
                            mapsInfo.file1 = fields[i];
                            break;
                        case 4:
                            mapsInfo.file2 = fields[i];
                            break;
                        case 5:
                            mapsInfo.module = fields[i];
                            break;
                    }
                }
                GroupInfo info;
                if(!groupDict.TryGetValue(mapsInfo.module, out info)) {
                    info = new GroupInfo();
                    info.module = mapsInfo.module;
                    groupDict.Add(info.module, info);
                }
                if (mapsInfo.vm_start < info.start) {
                    info.start = mapsInfo.vm_start;
                }
                if(mapsInfo.vm_end > info.end) {
                    info.end = mapsInfo.vm_end;
                }
                info.size += mapsInfo.vm_end - mapsInfo.vm_start;
            }
            var list = new List<GroupInfo>();
            foreach(var pair in groupDict) {
                list.Add(pair.Value);
            }
            list.Sort((a, b) => {
                if (a.start < b.start)
                    return -1;
                else if (a.start == b.start)
                    return 0;
                else
                    return 1;
            });
            using(var sw = new StreamWriter("groups.csv")) {
                sw.WriteLine("module,start,end,size");
                foreach(var g in list) {
                    sw.WriteLine("\"{0}\",\"{1,8:X8}\",\"{2,8:X8}\",{3}", g.module, g.start, g.end, g.size);
                }
                sw.Close();
            }
        }
    }
}

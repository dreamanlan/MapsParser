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
    class MapsGroupInfo
    {
        internal string module = string.Empty;
        internal ulong start = ulong.MaxValue;
        internal ulong end = ulong.MinValue;
        internal ulong size = 0;
    }
    class SmapsInfo
    {
        internal ulong vm_start = 0;
        internal ulong vm_end = 0;
        internal string flags = string.Empty;
        internal string offset = string.Empty;
        internal string file1 = string.Empty;
        internal string file2 = string.Empty;
        internal string module = string.Empty;

        internal ulong sizeKB = 0;
        internal ulong rss = 0;
        internal ulong pss = 0;
        internal ulong shared_clean = 0;
        internal ulong shared_dirty = 0;
        internal ulong private_clean = 0;
        internal ulong private_dirty = 0;
        internal ulong referenced = 0;
        internal ulong anonymous = 0;
        internal ulong swap = 0;
        internal ulong swappss = 0;
    }
    class SmapsGroupInfo
    {
        internal string module = string.Empty;
        internal ulong start = ulong.MaxValue;
        internal ulong end = ulong.MinValue;
        internal ulong size = 0;

        internal ulong sizeKB = 0;
        internal ulong rss = 0;
        internal ulong pss = 0;
        internal ulong shared_clean = 0;
        internal ulong shared_dirty = 0;
        internal ulong private_clean = 0;
        internal ulong private_dirty = 0;
        internal ulong referenced = 0;
        internal ulong anonymous = 0;
        internal ulong swap = 0;
        internal ulong swappss = 0;
    }
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("Usage:MapsParser maps.txt or SmapsParser smaps.txt");
                return;
            }
            var exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var exeName = Path.GetFileNameWithoutExtension(exe);
            var file = args[0];
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (exeName == "MapsParser") {
                var groupDict = new Dictionary<string, MapsGroupInfo>();
                var lines = File.ReadAllLines(file);
                foreach (var line in lines) {
                    var mapsInfo = new MapsInfo();
                    var fields = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var se = fields[0].Split('-');
                    ulong start = ulong.Parse(se[0], System.Globalization.NumberStyles.AllowHexSpecifier);
                    ulong end = ulong.Parse(se[1], System.Globalization.NumberStyles.AllowHexSpecifier);
                    mapsInfo.vm_start = start;
                    mapsInfo.vm_end = end;
                    for (int i = 0; i < fields.Length; ++i) {
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
                    MapsGroupInfo info;
                    if (!groupDict.TryGetValue(mapsInfo.module, out info)) {
                        info = new MapsGroupInfo();
                        info.module = mapsInfo.module;
                        groupDict.Add(info.module, info);
                    }
                    if (mapsInfo.vm_start < info.start) {
                        info.start = mapsInfo.vm_start;
                    }
                    if (mapsInfo.vm_end > info.end) {
                        info.end = mapsInfo.vm_end;
                    }
                    info.size += mapsInfo.vm_end - mapsInfo.vm_start;
                }
                var list = new List<MapsGroupInfo>();
                foreach (var pair in groupDict) {
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
                using (var sw = new StreamWriter(fileName + "_groups.csv")) {
                    sw.WriteLine("module,start,end,size");
                    foreach (var g in list) {
                        sw.WriteLine("\"{0}\",\"{1,8:X8}\",\"{2,8:X8}\",{3}", g.module, g.start, g.end, g.size);
                    }
                    sw.Close();
                }
            }
            else if (exeName == "SmapsParser") {
                var groupDict = new Dictionary<string, SmapsGroupInfo>();
                SmapsInfo curInfo = null;
                SmapsGroupInfo curGroup = null;
                var lines = File.ReadAllLines(file);
                foreach (var line in lines) {
                    var mapsInfo = new SmapsInfo();
                    var fields = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (fields[0].IndexOf('-') > 0) {
                        var se = fields[0].Split('-');
                        ulong start = ulong.Parse(se[0], System.Globalization.NumberStyles.AllowHexSpecifier);
                        ulong end = ulong.Parse(se[1], System.Globalization.NumberStyles.AllowHexSpecifier);
                        mapsInfo.vm_start = start;
                        mapsInfo.vm_end = end;
                        for (int i = 0; i < fields.Length; ++i) {
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
                        SmapsGroupInfo groupInfo;
                        if (!groupDict.TryGetValue(mapsInfo.module, out groupInfo)) {
                            groupInfo = new SmapsGroupInfo();
                            groupInfo.module = mapsInfo.module;
                            groupDict.Add(groupInfo.module, groupInfo);
                        }
                        if (mapsInfo.vm_start < groupInfo.start) {
                            groupInfo.start = mapsInfo.vm_start;
                        }
                        if (mapsInfo.vm_end > groupInfo.end) {
                            groupInfo.end = mapsInfo.vm_end;
                        }
                        groupInfo.size += mapsInfo.vm_end - mapsInfo.vm_start;

                        curInfo = mapsInfo;
                        curGroup = groupInfo;
                    }
                    else {
                        var key = fields[0];
                        var val = fields[1];
                        if (key == "Size:") {
                            curInfo.sizeKB = ulong.Parse(val);
                            curGroup.sizeKB += curInfo.sizeKB;
                        }
                        else if (key == "Rss:") {
                            curInfo.rss = ulong.Parse(val);
                            curGroup.rss += curInfo.rss;
                        }
                        else if (key == "Pss:") {
                            curInfo.pss = ulong.Parse(val);
                            curGroup.pss += curInfo.pss;
                        }
                        else if (key == "Shared_Clean:") {
                            curInfo.shared_clean = ulong.Parse(val);
                            curGroup.shared_clean += curInfo.shared_clean;
                        }
                        else if (key == "Shared_Dirty:") {
                            curInfo.shared_dirty = ulong.Parse(val);
                            curGroup.shared_dirty += curInfo.shared_dirty;
                        }
                        else if (key == "Private_Clean:") {
                            curInfo.private_clean = ulong.Parse(val);
                            curGroup.private_clean += curInfo.private_clean;
                        }
                        else if (key == "Private_Dirty:") {
                            curInfo.private_dirty = ulong.Parse(val);
                            curGroup.private_dirty += curInfo.private_dirty;
                        }
                        else if (key == "Referenced:") {
                            curInfo.referenced = ulong.Parse(val);
                            curGroup.referenced += curInfo.referenced;
                        }
                        else if (key == "Anonymous:") {
                            curInfo.anonymous = ulong.Parse(val);
                            curGroup.anonymous += curInfo.anonymous;
                        }
                        else if (key == "Swap:") {
                            curInfo.swap = ulong.Parse(val);
                            curGroup.swap += curInfo.swap;
                        }
                        else if (key == "SwapPss:") {
                            curInfo.swappss = ulong.Parse(val);
                            curGroup.swappss += curInfo.swappss;
                        }
                    }
                }
                var list = new List<SmapsGroupInfo>();
                foreach (var pair in groupDict) {
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
                using (var sw = new StreamWriter(fileName + "_groups.csv")) {
                    sw.WriteLine("module,start,end,size,sizeKB,rss,pss,shared_clean,shared_dirty,private_clean,private_dirty,referenced,anonymous,swap,swappss");
                    foreach (var g in list) {
                        sw.WriteLine("\"{0}\",\"{1,8:X8}\",\"{2,8:X8}\",{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", g.module, g.start, g.end, g.size, g.sizeKB, g.rss, g.pss,
                            g.shared_clean, g.shared_dirty, g.private_clean, g.private_dirty, g.referenced, g.anonymous, g.swap, g.swappss);
                    }
                    sw.Close();
                }
            }
        }
    }
}

using System;
using System.IO;
using System.Net.Mime;
using System.Text;

namespace Monica.Common.Utils
{
    public class SystemHelper
    {
        public static void CreateDirectory(string name)
        {
            if (Directory.Exists(name) == false)
                Directory.CreateDirectory(name);
        }

        public static String ReadLastLine(string path)
        {
            int charsize = Encoding.Default.GetByteCount("\n");
            byte[] buffer = Encoding.Default.GetBytes("\n");
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                long endpos = stream.Length/charsize;
                for (long pos = charsize; pos < endpos; pos += charsize)
                {
                    stream.Seek(-pos, SeekOrigin.End);
                    stream.Read(buffer, 0, buffer.Length);
                    if (Encoding.Default.GetString(buffer) == "\n")
                    {
                        buffer = new byte[stream.Length - stream.Position];
                        stream.Read(buffer, 0, buffer.Length);
                        return Encoding.Default.GetString(buffer);
                    }
                }
            }
            return null;
        }
    }
}

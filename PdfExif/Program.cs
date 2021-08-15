using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfExif
{
    class Program
    {
        static void Main(string[] args)
        {

            //Console.WriteLine(Encoding.UTF8.GetBytes("\xFF")[0] == (byte)255);


            byte[] startPdf = Encoding.UTF8.GetBytes("%PDF");
            byte[] endPdf = Encoding.UTF8.GetBytes("%%EOF");
            byte[] startStream = Encoding.UTF8.GetBytes("stream");
            byte[] endStream = Encoding.UTF8.GetBytes("endstream");
            byte[] startJpg = new byte[] { 0xFF, 0xD8 };
            byte[] endJpg = new byte[] { 0xFF, 0xD9 };
            byte[] startExif = new byte[] { 0xFF, 0xE1 };
            byte[] endExif = new byte[] { 0xFF };
            byte[] orientation = new byte[] { 0x01, 0x12 };

            var markers = new List<(byte[] start, byte[] end)>
            {
                ( null, null ),
                ( startPdf, endPdf ),
                ( startStream, endStream ),
                ( startJpg, endJpg),
                ( startExif, endExif),
                ( orientation, null),
            };

            //byte[][] markers = new[] { startStream, endStream, startJpg, startExif, orientation };

            string filePath = @"C:\Users\jamie\Downloads\RotatedGuitar(1).pdf";

            log("start");
            var file = File.Open(filePath, FileMode.Open, FileAccess.Read);
            log("opened file");
            /*
            var memory = new MemoryStream();
            file.CopyTo(memory);
            file.Dispose();
            log("copied");
            memory.Position = 0;
            */
            Search(file, markers);

            log("finished");
        }

        private static bool Search(Stream stream, List<(byte[] start, byte[] end)> needle)
        {
            byte[] b = new byte[1];
            int i, p, c;
            i = p = c = 0;
            bool startExif = false;
            using (Stream output = File.Create(@"C:\Users\jamie\Downloads\Out.pdf"))
                while (stream.Read(b, 0, 1) > 0)
                {
                    if (startExif)
                    {
                        b[0] = 0x00;
                        startExif = false;
                    }
                    else
                    {
                        if (i >= needle.Count() - 1)
                        {
                            // Replace second orientation EXIF byte
                            b[0] = 0x00;
                            log("exif");
                            // end of orientation EXIF
                            i--;
                            //stream.Write(b, 0, 1);
                            //stream.Flush();
                        }
                        if (MatchByte(b[0], needle[i].end, ref p))
                        {
                            // Console.WriteLine("end " + i);
                            i--;
                        }
                        if (MatchByte(b[0], needle[i + 1].start, ref c))
                        {
                            i++;
                            // Console.WriteLine("start " + i);
                        }
                        // Replace first orientation EXIF byte
                        if (i >= needle.Count() - 1)
                        {
                            startExif = true;
                            //b[0] = 0x00;
                            //stream.Write(b, 0, 1);
                        }
                    }
                    output.Write(b, 0, 1);
                }
            return false;
        }

        private static bool MatchByte(byte b, byte[] needle, ref int index)
        {
            if (needle == null)
                return false;

            if (b == needle[index])
                index++;
            else if (b == needle[0])
                index = 1;
            else
                index = 0;

            if (index == needle.Length)
            {
                index = 0;
                return true;
            }
            else
                return false;
        }

        private static void log(string msg)
        {
            Console.WriteLine(DateTime.Now.ToString("o") + " " + msg);
        }

        private static bool CompareBytes(byte a, byte b)
        {
            return (a == b) || ((a == ' ') && (Char.IsWhiteSpace(Encoding.UTF8.GetString(new[] { b }), 0)));
        }
    }
}

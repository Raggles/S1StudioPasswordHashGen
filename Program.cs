using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Management;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace S1StudioPasswordReset
{
    public sealed class PasswordTools
    {
        public static string WindowsSerialNumber
        {
            get
            {
                var str = string.Empty;
                try
                {
                    foreach (var managementBaseObject in new ManagementObjectSearcher("Select * from Win32_OperatingSystem").Get())
                        str = managementBaseObject["SerialNumber"].ToString();
                }
                catch (Exception)
                {
                    // ignored
                }

                return str;
            }
        }

        public static string ComputeHash(string password)
        {
            return PasswordTools.ByteArrayToString(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(password)));
        }

        public static string ComputeHashWithWindowsId(string password)
        {
            return PasswordTools.ComputeHash(PasswordTools.ComputeHash(password) + PasswordTools.WindowsSerialNumber);
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
            foreach (byte num in ba)
                stringBuilder.AppendFormat("{0:x2}", num);
            return stringBuilder.ToString();
        }
    }

    class Program
    {
        static void Main()
        {
            const string root = @"C:\ProgramData\Schneider Electric\MiCOM S1 Studio";
            var matches = new List<string>();
            var targetDir = "";
            if (Directory.Exists(root))
            {
                //bit weird that we use GetFileName to get the name of the directory
                matches.AddRange(Directory.EnumerateDirectories(root).Where(dir => Regex.Match(Path.GetFileName(dir) ?? throw new InvalidOperationException("Directory can't be null"), @"^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$").Success));
            }

            if (matches.Count == 0)
            {
                Console.WriteLine("No configuration file found, copy this hash into Configuration.bin manually.  Note that this program must be run on the target system.");
            }
            else
            {
                Console.WriteLine("Select target version:");
                var count = 1;
                foreach (var item in matches)
                {
                    Console.WriteLine($"{count++}) {item}");
                }

                Console.WriteLine($"{count}) Print hash only, no automatic correction");

                var input = Console.ReadLine();
                if (int.TryParse(input, out var iInput) && iInput <= matches.Count && iInput > 0)
                {
                    targetDir = matches[iInput-1];
                }
            }

            var hash = PasswordTools.ComputeHashWithWindowsId(" ");
            Console.WriteLine($"New hash: {hash}");
            if (!string.IsNullOrWhiteSpace(targetDir))
            {
                try
                {
                    File.Copy(Path.Combine(targetDir, "Configuration.bin"), Path.Combine(targetDir, $"Configuration.bak.{DateTime.Now.Ticks}"));
                    File.WriteAllText(Path.Combine(targetDir, "Configuration.bin"), hash);
                    Console.WriteLine("Successfully updated Configuration.bin");
                }
                catch
                {
                    Console.WriteLine("Error updating Configuration.bin");
                }
            }

            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
        }
    }
}

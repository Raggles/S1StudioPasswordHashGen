using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace HC.Utils.S1Studio
{
    public sealed class PasswordTools
    {
        public static string WindowsSerialNumber
        {
            get
            {
                string str = string.Empty;
                try
                {
                    foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher("Select * from Win32_OperatingSystem").Get())
                        str = managementBaseObject["SerialNumber"].ToString();
                }
                catch (Exception ex)
                {
                }
                return str;
            }
        }
        
        public static bool IsNERCCompiled(string password)
        {
            if (password.Length < 6 || !Regex.Match(password, "[0-9]").Success || (!Regex.Match(password, "[A-Z]").Success || !Regex.Match(password, "[a-z]").Success))
                return false;
            List<char> list = new List<char>();
            for (int index = 33; index <= 47; ++index)
                list.Add((char)index);
            for (int index = 58; index <= 63; ++index)
                list.Add((char)index);
            for (int index = 91; index <= 96; ++index)
                list.Add((char)index);
            bool flag = false;
            foreach (char ch in password)
            {
                if (list.Contains(ch))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        public static bool IsAllowedCharacter(char character)
        {
            if ((int)character >= 33)
                return (int)character <= 122;
            else
                return false;
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
                stringBuilder.AppendFormat("{0:x2}", (object)num);
            return ((object)stringBuilder).ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter new S1 Studio password:");
            Console.WriteLine("New password hash is: " + PasswordTools.ComputeHashWithWindowsId(Console.ReadLine()));
            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
        }
    }
}

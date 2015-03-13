using System;

namespace NGopher.Gopher
{
    public class GopherItem
    {
        // Item types as described in RFC 1436
        public const char TYPE_FILE        = '0';
        public const char TYPE_DIRECTORY   = '1';
        public const char TYPE_PHONEBOOK   = '2';
        public const char TYPE_ERROR       = '3';
        public const char TYPE_BINHEX      = '4';
        public const char TYPE_PC_DOS_BIN  = '5';
        public const char TYPE_UUENCODE    = '6';
        public const char TYPE_INDEXSEARCH = '7';
        public const char TYPE_TELNET      = '8';
        public const char TYPE_BINARY      = '9';
        public const char TYPE_REDUNDANT   = '+';
        public const char TYPE_TN3270      = 'T';
        public const char TYPE_GIF         = 'g';
        public const char TYPE_IMAGE       = 'I';
        // Non-RFC types
        public const char TYPE_HTML        = 'h';
        public const char TYPE_INFO        = 'i';
        public const char TYPE_AUDIO       = 's';

        public char Type { get; set; }
        public string UserName { get; set; }
        public string Selector { get; set; }
        public string Host { get; set; }
        public ushort Port { get; set; }

        /// <summary>
        /// Builds a new GopherItem object out of a string.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static GopherItem BuildItem(string line)
        {
            string[] list = line.TrimEnd().Split('\t');
            if (list.Length != 4)
                return null;
            char type = list[0][0];
            string userName = list[0].Substring(1);
            ushort port = Convert.ToUInt16(list[3]);
            return new GopherItem
            {
                Type = type,
                UserName = userName,
                Selector = list[1],
                Host = list[2],
                Port = port
            };
        }

        public override string ToString()
        {
            return String.Format("Type: {0}, UserName: {1}, Selector: {2} (gopher://{3}:{4})", Type, UserName, Selector, Host, Port);
        }
    }
}
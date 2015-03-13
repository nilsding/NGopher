using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace NGopher.Gopher
{
    public class GopherClient
    {
        private StreamSocket _streamSocket;
        private HostName _hostName;
        private string _port = "70";  // who the fuck thought specifying the port as a string is useful?
        private bool _connected;

        public bool Connected
        {
            get { return _connected; }
        }

        public string Server
        {
            get { return _hostName.ToString(); }
            set { _hostName = new HostName(value); }
        }

        public ushort Port
        {
            get { return Convert.ToUInt16(_port); }
            set { _port = String.Format("{0}", value); }
        }

        public async Task<bool> Connect()
        {
            if (_connected)
                return true;
            _streamSocket = new StreamSocket();
            try
            {
                Debug.WriteLine("Attempting to connect to " + Server + ":" + _port);
                await _streamSocket.ConnectAsync(_hostName, _port);
                _connected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("connect failed: " + ex.Message);

                _connected = false;
                _streamSocket.Dispose();
                _streamSocket = null;

                // if the error is unknown, it means that the error is fatal and retries will likely fail
                if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
                    throw;
            }
            return _connected;
        }

        public async Task<List<GopherItem>> GetDirectoryContents(string selector = "")
        {
            var response = await MakeRequest(selector);
            if (response == null)
                return null;

            var lines = response.Split('\n');

            return lines.Select(GopherItem.BuildItem).Where(gi => gi != null).ToList();
        }

        private async Task<string> MakeRequest(string selector = "")
        {
            string retval;

            if (!await Connect())
                return null;

            try
            {
                // send the request
                Debug.WriteLine(">" + selector);
                var data = selector + "\r\n";
                var writer = new DataWriter(_streamSocket.OutputStream);
                writer.WriteString(data);
                await writer.StoreAsync();

                // read the reply
                var reader = new DataReader(_streamSocket.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;

                var sb = new StringBuilder();
                uint x;
                do
                {
                    x = await reader.LoadAsync(512);
                    sb.Append(reader.ReadString(x));
                } while (x > 0);

                retval = sb.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("send or recv failed: " + ex.Message);

                // if the error is unknown, it means that the error is fatal and retries will likely fail
                if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
                    throw;

                return null;
            }
            finally
            {
                // after any Gopher request, the connection is closed by the server.
                _connected = false;
                if (_streamSocket != null)
                    _streamSocket.Dispose();
                _streamSocket = null;
            }

            return retval;
        }
    }
}
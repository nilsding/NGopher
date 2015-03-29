using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;

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
            var response = await MakeTextRequest(selector);
            if (response == null)
                return null;

            var lines = response.Split('\n');

            return lines.Select(GopherItem.BuildItem).Where(gi => gi != null).ToList();
        }

        public async Task<byte[]> MakeBinaryRequest(string selector)
        {
            return (byte[]) (await MakeRequest(true, selector));
        }

        public async Task<string> MakeTextRequest(string selector)
        {
            return (string) (await MakeRequest(false, selector));
        }

        private async Task<object> MakeRequest(bool binary = false, string selector = "")
        {
            var bytes = new List<Byte>();

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

                uint x;
                while ((x = await reader.LoadAsync(512)) > 0)
                    bytes.AddRange(reader.ReadBuffer(x).ToArray());
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

            if (binary)
                return bytes.ToArray();
            return Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
        }
    }
}
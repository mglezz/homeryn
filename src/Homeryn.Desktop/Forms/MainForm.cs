using System.Net.Sockets;
using System.Text.Json;
using Homeryn.Desktop.Networking;

namespace Homeryn.Desktop
{
    public partial class MainForm : Form
    {
        private readonly TcpClientService client = new();
        private readonly CancellationTokenSource lifetime = new();
        private bool isConnecting;

        public MainForm()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object? sender, EventArgs e)
        {
            if (isConnecting)
            {
                return;
            }

            isConnecting = true;
            Button? button = sender as Button;
            if (button is not null)
            {
                button.Enabled = false;
            }

            try
            {
                await client.PingAsync(cancellationToken: lifetime.Token);
                if (!IsDisposed)
                {
                    MessageBox.Show(this, "The connection is working.", "Homeryn",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (OperationCanceledException)
            {
                if (!IsDisposed)
                {
                    MessageBox.Show(this, "The server took too long to respond. Please try again.", "Homeryn",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception exception) when (exception is SocketException or IOException or JsonException or InvalidDataException)
            {
                if (!IsDisposed)
                {
                    MessageBox.Show(this, "I couldn't connect. Check that the server is running and try again.",
                        "Homeryn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                isConnecting = false;
                if (button is { IsDisposed: false })
                {
                    button.Enabled = true;
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            lifetime.Cancel();
            lifetime.Dispose();
            base.OnFormClosed(e);
        }
    }
}

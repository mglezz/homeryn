using Homeryn.Desktop.Forms;
using Homeryn.Desktop.Networking;
using Homeryn.Desktop.Hosting;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text.Json;

namespace Homeryn.Desktop
{
    public partial class MainForm : Form
    {
        private readonly TcpClientService client = new TcpClientService();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
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
            if (button != null)
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
            catch (SocketException)
            {
                ShowConnectionError();
            }
            catch (IOException)
            {
                ShowConnectionError();
            }
            catch (JsonException)
            {
                ShowConnectionError();
            }
            catch (InvalidDataException)
            {
                ShowConnectionError();
            }
            finally
            {
                isConnecting = false;
                if (button != null && !button.IsDisposed)
                {
                    button.Enabled = true;
                }
            }
        }

        private void ShowConnectionError()
        {
            if (!IsDisposed)
            {
                MessageBox.Show(this, "Couldn't connect. Check that the server is running and try again.",
                    "Homeryn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            lifetime.Cancel();
            lifetime.Dispose();
            base.OnFormClosed(e);
        }

        private void ShoppingListButton_Click(object? sender, EventArgs e)
        {
            using ShoppingListForm shoppingListForm = new ShoppingListForm();
            shoppingListForm.ShowDialog(this);
        }

        private void ChoresButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(this, "Chores are not available yet. The shopping list is ready to use.",
                "Homeryn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void StartServerButton_Click(object? sender, EventArgs e)
        {
            if (isConnecting)
            {
                return;
            }
            isConnecting = true;
            startServerButton.Enabled = false;
            connectButton.Enabled = false;
            try
            {
                LocalServerLauncher launcher = new LocalServerLauncher();
                if (!await launcher.IsRunningAsync(lifetime.Token))
                {
                    string? executable = launcher.FindExecutable();
                    if (executable == null)
                    {
                        using OpenFileDialog dialog = new OpenFileDialog();
                        dialog.Title = "Select the built Homeryn server";
                        dialog.Filter = "Homeryn server|Homeryn.Server.exe";
                        if (dialog.ShowDialog(this) != DialogResult.OK)
                        {
                            return;
                        }
                        executable = dialog.FileName;
                    }
                    await launcher.StartAsync(executable, lifetime.Token);
                }
                if (!IsDisposed)
                {
                    MessageBox.Show(this, "The server is ready. You can open the shopping list.",
                        "Homeryn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (OperationCanceledException)
            {
                if (!IsDisposed)
                {
                    MessageBox.Show(this, "The server did not respond in time. Check its console and try Check connection.",
                        "Homeryn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception exception) when (exception is Win32Exception || exception is IOException
                || exception is InvalidOperationException || exception is JsonException
                || exception is InvalidDataException || exception is UnauthorizedAccessException)
            {
                if (!IsDisposed)
                {
                    MessageBox.Show(this, $"Couldn't start the server. {exception.Message}",
                        "Homeryn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                isConnecting = false;
                if (!IsDisposed)
                {
                    startServerButton.Enabled = true;
                    connectButton.Enabled = true;
                }
            }
        }
    }
}

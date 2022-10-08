using ImageGlass.Base.NamedPipes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace igcmd;

public partial class FrmSlideshow : Form
{
    private PipeClient _client;
    private string _serverName;

    public FrmSlideshow(string serverName)
    {
        InitializeComponent();

        _serverName = serverName;
    }

    private void FrmSlideshow_Load(object sender, EventArgs e)
    {
        Text = _serverName;

        _client = new PipeClient(_serverName, PipeDirection.InOut);
        _client.MessageReceived += Client_MessageReceived;
        _client.Disconnected += Client_Disconnected;

        _ = ConnectAsync();
    }

    private void Client_MessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Message)) return;

        if (e.Message == "TERMINATE")
        {
            Exit();
        }
    }

    private void Client_Disconnected(object? sender, DisconnectedEventArgs e)
    {
        Exit();
    }



    private void FrmSlideshow_FormClosing(object sender, FormClosingEventArgs e)
    {
        _client.Dispose();
    }


    private async Task ConnectAsync()
    {
        await _client.ConnectAsync();
    }

    private void Exit()
    {
        Application.Exit();
    }

    private async void button1_Click(object sender, EventArgs e)
    {
        await _client.SendAsync("msg from client");
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DaveBot
{
    public partial class Window : Form
    {
        Dictionary<string, ulong> channels = new Dictionary<string, ulong>();
        DaveBot dbot;


        public Window()
        {
            InitializeComponent();
        }

        public void giveBot(DaveBot bot)
        {
            this.dbot = bot;
            if (!InvokeRequired)
            {
                foreach (Control c in serverBox.Controls)
                {
                    c.Enabled = true;
                }
                foreach (Control c in musicBox.Controls)
                {
                    c.Enabled = true;
                }
                foreach (Control c in userBox.Controls)
                {
                    c.Enabled = true;
                }
                foreach (Control c in loginBox.Controls)
                {
                    c.Enabled = false;
                }
            } else
            {
                Invoke(new Action(() =>
                {
                    foreach (Control c in serverBox.Controls)
                    {
                        c.Enabled = true;
                    }
                    foreach (Control c in musicBox.Controls)
                    {
                        c.Enabled = true;
                    }
                    foreach (Control c in userBox.Controls)
                    {
                        c.Enabled = true;
                    }
                    foreach (Control c in loginBox.Controls)
                    {
                        c.Enabled = false;
                    }
                }));
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            new Task(async () =>
            {
                await Program.disconnect("");
            }).RunSynchronously();
            base.OnFormClosing(e); 
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            Program.startBot(this.tokenBox.Text, this);
        }

        public void updateStatus(string status, Color color)
        {
            Console.WriteLine("Status Updated.");
            if (!InvokeRequired)
            {
                this.statusLabel.Text = status;
                this.statusLabel.ForeColor = color;
            } else
            {
                Invoke(new Action(() =>
                {
                    this.statusLabel.Text = status;
                    this.statusLabel.ForeColor = color;
                }));
            }
        }

        private void groupBox1_Enter_1(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void serverIdBox_GotFocus(object sender, EventArgs e)
        {
            this.AcceptButton = addServerButton;
            if (serverIdBox.ForeColor == SystemColors.ScrollBar)
            {
                serverIdBox.Text = "";
                serverIdBox.ForeColor = SystemColors.ControlText;
            }
        }

        private void serverIdBox_LostFocus(object sender, EventArgs e)
        {
            if (serverIdBox.Text == null || serverIdBox.Text.Equals(""))
            {
                serverIdBox.Text = "Server Id";
                serverIdBox.ForeColor = SystemColors.ScrollBar;
            }
        }

        private void urlBox_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = enqueueButton;
            if (urlBox.ForeColor == SystemColors.ScrollBar)
            {
                urlBox.Text = "";
                urlBox.ForeColor = SystemColors.ControlText;
            }
        }

        private void urlBox_Leave(object sender, EventArgs e)
        {
            if (urlBox.Text == null || urlBox.Text.Equals(""))
            {
                urlBox.Text = "Youtube URL";
                urlBox.ForeColor = SystemColors.ScrollBar;
            }
        }

        private void volumeBar_Scroll(object sender, EventArgs e)
        {
            volumeText.Text = "Volume: " + volumeBar.Value + "%";
        }

        private void tokenBox_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = loginButton;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (serverIdList.SelectedItem != null)
            {
                ulong serverId;
                channels.TryGetValue((string)serverIdList.SelectedItem,out serverId);
                if (dbot != null)
                {
                    dbot.setFocusedChannel(serverId);
                }
            }
        }

        private void addServerButton_Click(object sender, EventArgs e)
        {
            ulong serverId;
            bool isULong = ulong.TryParse(serverIdBox.Text, out serverId);
            if (isULong && dbot != null)
            {
                string serverName = this.dbot.idToName(serverId);
                channels.Add(serverName, serverId);
                serverIdList.Items.Add(serverName);
                serverIdBox.Text = "Server Id";
                serverIdBox.ForeColor = SystemColors.ScrollBar;
            }
        }
    }
}

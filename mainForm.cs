using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.IO;    // for StreamReader
using System.Windows.Forms;

namespace Desksub
{
    public partial class mainForm : Form
    {
        public string url;
        public string homePath;
        public dynamic searched;
        public dynamic selected;
        public subsceneApi subapi;

        public mainForm(string[] args)
        {
            this.url = args[0];
            InitializeComponent();
            this.subapi = new subsceneApi(this);
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            this.setStatusText(Titles.STATUS_LOADING_TITLES);
            this.homePath = System.IO.Path.GetDirectoryName(this.url);
            var filenNme = Path.GetFileNameWithoutExtension(this.url);
            filenNme = Base64Encode(filenNme);
            filenNme = System.Uri.EscapeDataString(filenNme);
            //subapi.request("YXZh", "search");
            subapi.request(filenNme, "search");
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            pictureBox3.Visible = true;
            this.setStatusText(Titles.STATUS_LOADING_SUBTITLES);
            this.setTitlesCursor(Cursors.WaitCursor);
            ListBox lb = sender as ListBox;
            string select = lb.SelectedItem.ToString();
            clearListbox(listBox2);
            var filenNme = Base64Encode(this.subapi.titles[select]);
            filenNme = System.Uri.EscapeDataString(filenNme);
            subapi.request(filenNme, "select");
        }

        private void listBox2_Click(object sender, EventArgs e)
        {
            this.setStatusText(Titles.STATUS_DOWNLOAD);
            listBox2.Cursor = Cursors.WaitCursor;
            ListBox lb = sender as ListBox;
            string select = lb.SelectedItem.ToString();
            if (!workerDownload.IsBusy) workerDownload.RunWorkerAsync(this.subapi.subtitles[select]);
        }

        private void workerDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var filenNme = RandomString();
                    client.DownloadFile((string)e.Argument, this.homePath + "/" + filenNme + ".zip");

                    if (File.Exists(this.homePath + "/" + filenNme + ".zip"))
                    {
                        System.Diagnostics.Process.Start(this.homePath + "/" + filenNme + ".zip");
                    }
                }
            }
            catch
            {
                this.messageBox(Titles.DOWNLOAD_FAILD, true);
            }
        }

        private void workerDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.setSubtitlesCursor(Cursors.Hand);
            this.setStatusText();
        }

        private void setStatusText(string text = "")
        {
            this.renderEmoji("", true);
            if(text == "")
            {
                label2.Text = "";
            }
            else
            {
                label2.Text = "|  " + text;
            }
            
        }

        public void render(string action, dynamic obj)
        {
            if (obj.ContainsKey("error"))
            {
                if (obj["error"] == "0")
                {
                    if (this.subapi.titles == null)
                    {
                        this.setTitlesCursor(Cursors.Default);
                        this.setStatusText(Titles.STATUS_ZERO_SUBTITLES);
                        this.renderEmoji("sad");
                        this.messageBox(Titles.ZERO_SUBTITLES, true);
                    }
                    else
                    {
                        this.setTitlesCursor(Cursors.Hand);
                        this.setStatusText(Titles.STATUS_ZERO_SUBTITLES);
                        this.renderEmoji("sad");
                        this.messageBox(Titles.ZERO_SUBTITLES);
                    }


                }
                else if (obj["error"] == "1")
                {
                    if (this.subapi.titles == null)
                    {
                        this.messageBox(Titles.BUSSY_SERVER, true);
                    }
                    else
                    {
                        this.setTitlesCursor(Cursors.Hand);
                        this.setStatusText(Titles.STATUS_BUSSY_SERVER);
                        this.renderEmoji("sad");
                        this.messageBox(Titles.BUSSY_SERVER);
                    }
                }
                return;
            }

            switch (action)
            {
                case "search":
                    this.renderTitles(obj);
                    break;
                case "select":
                    this.renderSubtitles(obj);
                    break;
            }
        }

        private void renderTitles(dynamic obj)
        {
            this.setStatusText(Titles.STATUS_FIND_TITLES);
            this.renderEmoji("happy");
            this.setTitlesCursor(Cursors.Hand);
            this.addTitles(obj);
        }

        private void renderEmoji(string emoticon, bool hide=false)
        {
            if(hide == true)
            {
                pictureBox4.Visible = false;
            }
             else
            {
                if(emoticon == "sad") pictureBox4.Image = Desksub.Properties.Resources.emoji2;
                else pictureBox4.Image = Desksub.Properties.Resources.emoji1;
                pictureBox4.Location = new Point(label2.Width + label2.Location.X, this.pictureBox4.Location.Y);
                pictureBox4.Visible = true;
            }
        }

        private void renderSubtitles(dynamic obj)
        {
            pictureBox3.Visible = false;
            this.setStatusText(Titles.STATUS_FIND_SUBTITLES);
            this.renderEmoji("happy");
            this.addSubtitles(obj);
        }

        private void addTitles(dynamic obj)
        {
            foreach (var x in obj)
            {
                listBox1.Items.Add(x.Key);
            }
            listBox1.SetSelected(0, true);
        }

        private void addSubtitles(dynamic obj)
        {
            foreach (var x in obj)
            {
                listBox2.Items.Add(x.Key);
            }
            this.setTitlesCursor(Cursors.Hand);
            listBox2.Enabled = true;
            listBox2.SetSelected(0, true);
        }

        private void setTitlesCursor(Cursor cursor)
        {
            listBox1.Cursor = cursor;
        }

        private void setSubtitlesCursor(Cursor cursor)
        {
            listBox2.Cursor = cursor;
        }

        private void clearListbox(ListBox name)
        {
            name.Items.Clear();
        }

        public void messageBox(string text, bool exit = false)
        {
            MessageBox.Show(text);
            if(exit == true) System.Environment.Exit(1);
        }

        private static Random random = new Random();
        private static string RandomString(int length=7)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private void mainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(1);
        }
    }
}

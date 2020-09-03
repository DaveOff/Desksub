using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Desksub
{
    public class subsceneApi
    {
        public string uri;
        public dynamic titles, subtitles = null;
        public Dictionary<string, dynamic> formMapping;
        private mainForm mainForm;

        public subsceneApi(mainForm mainForm)
        {
            this.mainForm = mainForm;
            string uri = configReader("api", "uri", System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\conf.ini");
            if(uri == "")
            {
                this.mainForm.messageBox(Titles.CONFIG_FAILD, true);
            }
            this.uri = uri;
        }

        public async Task request(string arg, string action)
        {
            var request = (HttpWebRequest)WebRequest.Create(this.uri + "/" + action + "/" + arg);
            var responseString = "";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        responseString = await sr.ReadToEndAsync();
                }

                dynamic obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                if (!obj.ContainsKey("error"))
                {
                    switch (action)
                    {
                        case "search":
                            this.titles = obj;
                            break;
                        case "select":
                            this.subtitles = obj;
                            break;
                    }
                }

                this.mainForm.render(action, obj);
            }
            catch
            {
                this.mainForm.messageBox(Titles.CONNECTION_ERROR, true);
            }
        }

        public string configReader(string SECTION, string KEY, string PATH, string DEFAULT_VALUE = "")
        {
            try
            {
                string[] READER_LINES = File.ReadAllLines(PATH);

                string CURRENT_SECTION = "";

                foreach (string READER_LINE in READER_LINES)
                {
                    if (READER_LINE.StartsWith("[") && READER_LINE.EndsWith("]"))
                    {
                        CURRENT_SECTION = READER_LINE;
                    }
                    else if (CURRENT_SECTION.Equals($"[{SECTION}]"))
                    {

                        string[] lineParts = READER_LINE.Split(new[] { '=' }, 2);

                        if (lineParts.Length >= 1 && lineParts[0] == KEY)
                        {
                            return lineParts.Length >= 2
                                ? lineParts[1]
                                : DEFAULT_VALUE;
                        }
                    }
                }
            }
            catch {}

            return DEFAULT_VALUE;
        }
    }
}

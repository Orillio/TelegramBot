using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Дз10
{
    public class Message
    {
        public string FileName { get; set; }
        public int ChatID { get; set; }
        public string FileId { get; set; }
        public string Text { get; set; }
        public string Date { get; set; }
        public bool Downloadable { get; set; }
        public Telegram.Bot.Types.Enums.MessageType Type { get; set; }
        public Message(Telegram.Bot.Types.Enums.MessageType type, string fileid, int userid, string text, string date, bool download, string filename)
        {
            this.Type = type;
            this.FileName = filename;
            this.ChatID = userid;
            this.Text = text;
            this.FileId = fileid;
            this.Date = date;
            this.Downloadable = download;
        }
    }
}

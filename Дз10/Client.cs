using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Дз10
{
    public class Client
    {
        public string FirstName { get; set; }
        public int ChatId { get; set; }
        public ObservableCollection<Message> Messages { get; set; }
        public Client(string name, int id)
        {
            FirstName = name;
            ChatId = id;
            Messages = new ObservableCollection<Message>();
        }
    }
}

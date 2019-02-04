using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramDaletNotificationBot {
    public class InputMessage {
        public String user_id;
        public String action;
        public List<String> @params;
        public String messanger = "telegram";
    }
}

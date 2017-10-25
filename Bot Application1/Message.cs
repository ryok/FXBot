using System;
using Microsoft.Bot.Connector;

namespace Bot_Application1
{
    public class Message
    {
        public ConversationReference RelatesTo { get; set; }
        public String Text { get; set; }
    }
}
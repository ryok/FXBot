using Microsoft.WindowsAzure.Storage.Table;

namespace Bot_Application1
{
    public class ConversationRefEntity : TableEntity
    {
        public ConversationRefEntity(string userId, string ConversationId)
        {
            this.PartitionKey = userId;
            this.RowKey = ConversationId;
        }

        public ConversationRefEntity() { }

        public string RelatesTo { get; set; }
    }
}
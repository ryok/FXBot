using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Bot_Application1
{
    public enum BotState
    {
        Default,
        RequestCurrencyCode,
        RequestClientName,
        RequestContract,
        RequestPositionVisual,
        RequestContractStepByStep
    };

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /*internal static IDialog<ElectricityUsageQuery> MakeUsageDialog()
        {
            return Chain.From(() => FormDialog.FromForm(ElectricityUsageQuery.BuildForm));
        }*/

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                // Botの現在の状態を取得
                StateClient stateClient = activity.GetStateClient();
                BotData conversationData = stateClient.BotState.GetConversationData(activity.ChannelId, activity.Conversation.Id);
                var state = conversationData.GetProperty<BotState>("state");

                if (state == BotState.Default)
                {
                    if (activity.Text.Contains("日銀") && activity.Text.Contains("会合"))
                    {
                        string message;
                        message = "直近の金融政策決定会合の日程です。 \n\n 4月26日（水）・27日（木）\n\n 6月15日（木）・16日（金）\n\n http://www.boj.or.jp/mopo/mpmsche_minu/index.htm/";

                        // 結果をユーザに送信
                        ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        Activity reply = activity.CreateReply(message);
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    else
                    {
                        // Default状態の場合はLuisDialogを呼ぶ
                        await Conversation.SendAsync(activity, () => new FxDialog());
                    }
                }
                else if (state == BotState.RequestPositionVisual)
                {
                    // 棒グラフ
                    if (activity.Text.Contains("棒グラフ"))
                    {
                        string message;
                        string keyword = "bar";
                        var uri = PhotoSearchAPI.GetPhotoByKeyword(keyword);
                        if (uri == null)
                        {
                            message = "申し訳ありません。取得できませんでした。";

                            // 結果をユーザに送信
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            Activity reply = activity.CreateReply(message);
                            await connector.Conversations.ReplyToActivityAsync(reply);
                        }
                        else if (uri != null)
                        {
                            // 画像はIMessageActivity.Attachmentsに情報を追加
                            var photoMessage = activity.CreateReply();
                            photoMessage.Text = "どうぞ";
                            photoMessage.Attachments.Add(new Attachment()
                            {
                                ContentType = "image/png",
                                ContentUrl = uri,
                                Name = "photo.png"
                            });

                            // 結果をユーザに送信
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            //Activity reply = activity.CreateReply(message);
                            await connector.Conversations.ReplyToActivityAsync(photoMessage);
                        }

                    } else
                    {
                        // Default状態の場合はLuisDialogを呼ぶ
                        await Conversation.SendAsync(activity, () => new FxDialog());
                    }
                    // 処理が終わったら、BotをDefault状態に戻す
                    conversationData.SetProperty<BotState>("state", BotState.Default);
                    await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.Conversation.Id, conversationData);
                }
                else if (state == BotState.RequestContract)
                {
                    string message;
                    
                    if (activity.Text == "はい" || activity.Text == "ok" || activity.Text.StartsWith("y"))
                    {
                        message = "約定を受け付けました。";

                        // 処理が終わったら、BotをDefault状態に戻す
                        conversationData.SetProperty<BotState>("state", BotState.Default);
                        await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.Conversation.Id, conversationData);

                    } else if (activity.Text == "いいえ" || activity.Text.StartsWith("n"))
                    {
                        message = "処理を中断しました。";

                        // 処理が終わったら、BotをDefault状態に戻す
                        conversationData.SetProperty<BotState>("state", BotState.Default);
                        await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.Conversation.Id, conversationData);
                    } else
                    {
                        message = "申し訳ありません。うまく理解できませんでした。yesかnoでお答えいただけますか。";
                    }
                    
                    // 結果をユーザに送信
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply = activity.CreateReply(message);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    // await Conversation.SendAsync(activity, () => new ContractDialog());
                }
                else if(state == BotState.RequestCurrencyCode)
                {
                    // RequestCurrencyCodeの場合は通貨ペアが入力されているので、
                    // activity.Textの内容を元にレートを取得する
                    string message;
                    if (activity.Text != null)
                    {
                        var rate = FxAPI.GetFxRate(activity.Text.Replace("/",""));
                        message = (rate != null) ? string.Format("現在{0}です。", rate) : "取得できませんでした。" ;
                    }
                    else
                    {
                        message = "通貨ペアが見つかりませんでした。";
                    }
                    // 結果をユーザに送信
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply = activity.CreateReply(message);
                    await connector.Conversations.ReplyToActivityAsync(reply);

                    // 処理が終わったら、BotをDefault状態に戻す
                    conversationData.SetProperty<BotState>("state", BotState.Default);
                    await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.Conversation.Id, conversationData);
                }
                else if(state == BotState.RequestClientName)
                {
                    // RequestClientNameの場合は顧客名が入力されているので、
                    // activity.Textの内容を元にポジションを取得する
                    var client = activity.Text;
                    string message;
                    if (client != null)
                    {
                        var position = FxAPI.GetPosition(client);
                        message = (position != null) ? string.Format("{0}様に関わる取引は{1}です。", client, position) : "ポジション情報を取得できませんでした。";
                    }
                    else
                    {
                        message = "該当するお客様が見つかりませんでした。";
                    }
                    // 結果をユーザに送信
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply = activity.CreateReply(message);
                    await connector.Conversations.ReplyToActivityAsync(reply);

                    // 処理が終わったら、BotをDefault状態に戻す
                    conversationData.SetProperty<BotState>("state", BotState.Default);
                    await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.Conversation.Id, conversationData);
                }
                //else if (state == BotState.RequestContractStepByStep)
                //{
                //    await Conversation.SendAsync(activity, () => new ContractDialog());
                //}
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        
        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity update = message;

                if (update.MembersAdded.Any())
                {
                    var newMembers = update.MembersAdded?.Where(t => t.Id != message.Recipient.Id);

                    foreach (var newMember in newMembers)
                    {
                        // ConversationReferenceオブジェクトを生成
                        var user = new ChannelAccount
                        {
                            Id = newMember.Id,
                            Name = newMember.Name
                        };

                        var bot = new ChannelAccount
                        {
                            Id = message.Recipient.Id,
                            Name = message.Recipient.Name
                        };

                        var conversation = new ConversationAccount
                        {
                            Id = message.Conversation.Id
                        };

                        var relatesTo = new ConversationReference
                        {
                            ActivityId = null,
                            User = user,
                            Bot = bot,
                            Conversation = conversation,
                            ChannelId = message.ChannelId,
                            ServiceUrl = message.ServiceUrl
                        };

                        // ストレージアカウントの接続文字列をパースし、ストレージアカウントを取得
                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

                        // クライアントを作成
                        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                        // "ConversationRef"テーブルのCloudTableオブジェクトを作成
                        CloudTable table = tableClient.GetTableReference("ConversationRef");

                        // 新しい"ConversationRef"エンティティを作成
                        ConversationRefEntity conversationRef = new ConversationRefEntity(relatesTo.User.Id, relatesTo.Conversation.Id);

                        // オブジェクトをJSONに変換
                        conversationRef.RelatesTo = JsonConvert.SerializeObject(relatesTo);

                        // "ConversationRef"エンティティを挿入するためのTableOperationオブジェクトを作成
                        TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(conversationRef);

                        // "ConversationRef"テーブルへ挿入
                        table.ExecuteAsync(insertOrReplaceOperation);
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
            // Triggerタイプはサポートされていないが、FunctionAppからの受信時はTriggerタイプ
            // 送信元のFunctionAppが使用するBotFrameworkがTriggerタイプで送信するため
            // BotFrameworkのGAによる変更になる可能性あり
            else if (message.Type == ActivityTypes.Trigger)
            {
                IEventActivity triggerEvent = message;

                // トリガーイベント値の"Message"部分をJSONからオブジェクトへ変換
                var messages = JsonConvert.DeserializeObject<Message>(((JObject)triggerEvent.Value).GetValue("Message").ToString());

                // ConversationReferenceからActivityを作成 
                var messageactivity = (Activity)messages.RelatesTo.GetPostToBotMessage();

                // クライアントコネクションを作成
                var client = new ConnectorClient(new Uri(messageactivity.ServiceUrl));

                // 返信メッセージの作成
                var triggerReply = messageactivity.CreateReply();
                triggerReply.Text = messages.Text;

                // メッセージの返信
                client.Conversations.ReplyToActivityAsync(triggerReply);
            }

            return null;
        }
    }
}
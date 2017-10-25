using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bot_Application1
{
    [LuisModel("YourModelID", "YourSubscriptionKey")]
    [Serializable]
    public class FxDialog : LuisDialog<object>
    {
        // 意図がわからない場合は、雑談APIをコール
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string TmpContext;
            //前回の会話の雑談Contextがあれば取得
            if (!context.ConversationData.TryGetValue<String>("ZatsudanContext",out TmpContext))
            {
                TmpContext = "";
            }

            //雑談APIをコール
            Reply resp = ZatsudanAPI.GetZatsudanConversation(result.Query, TmpContext);
            
            //新しい雑談Contextを保存
            context.ConversationData.SetValue<String>("ZatsudanContext", resp.context);


            await context.PostAsync(resp.utt);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Ready")]
        public async Task Ready(IDialogContext context, LuisResult result)
        {
            // LuisからのIntentがReadyの場合、いつでも準備バッチリ
            string message = "バッチリです。いつでもOKです！";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("FxRate")]
        public async Task FxRate(IDialogContext context, LuisResult result)
        {
            string currencyPair = null;
            foreach (EntityRecommendation entity in result.Entities)
            {
                System.Console.WriteLine(entity.Type);

                if (entity.Type == "CurrencyPair")
                {
                    // APIより返却されるJSON上の表現に変換
                    currencyPair = FxAPI.GetCurrencyPairFromEntity(entity.Entity.Replace(" ", ""));
                }
            }

            string message;
            if (currencyPair != null)
            {
                var rate = FxAPI.GetFxRate(currencyPair);
                message = (rate != null) ? string.Format("現在{0}です。", rate) : "レート情報を取得できませんでした。";
            } else
            {
                // 通貨ペアがわからなかったら、ユーザーに聞いてBotの状態をRequestCurrencyCodeに遷移する
                message = "どの通貨ペアについて知りたいですか？";
                context.ConversationData.SetValue<BotState>("state", BotState.RequestCurrencyCode);
            }
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        // 挨拶（日付固定。。）
        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            string message = "おはようございます。今日は2017/3/28（火）。今日も一日がんばりましょう";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        // 感謝
        [LuisIntent("Appreciation")]
        public async Task Appreciation(IDialogContext context, LuisResult result)
        {
            string message = "どういたしまして。お役に立ててうれしいです";
            context.ConversationData.SetValue<BotState>("state", BotState.Default);
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        // デモ用顧客ポジション照会
        [LuisIntent("Postion")]
        public async Task Position(IDialogContext context, LuisResult result)
        {
            string client = null;
            foreach (EntityRecommendation entity in result.Entities)
            {
                if (entity.Type == "Client")
                {
                    // ENtityから顧客情報を取得
                    client = entity.Entity.Replace(" ", "");
                }
            }

            string message;
            if (client != null)
            {
                // ポジション情報取得
                var position = FxAPI.GetPosition(client);
                
                // 画像uri取得
                var keyword = "circle";
                var uri = PhotoSearchAPI.GetPhotoByKeyword(keyword);

                if (uri == null)
                {
                    message = (position != null) ? string.Format("{0}様に関わる取引は{1}です。\n\n (システム画面URL : http://example.com)", client, position) : "ポジション情報を取得できませんでした。";
                    await context.PostAsync(message);
                }
                else if (uri != null)
                {
                    if (position != null)
                    {
                        // 画像はIMessageActivity.Attachmentsに情報を追加
                        var photoMessage = context.MakeMessage();
                        photoMessage.Text = string.Format("{0}様に関わる取引は{1}です。\n\n (システム画面URL : http://example.com)", client, position);
                        photoMessage.Attachments.Add(new Attachment()
                        {
                            ContentType = "image/png",
                            ContentUrl = uri,
                            Name = "photo.png"
                        });
                        await context.PostAsync(photoMessage);
                    }
                    else
                    {
                        message = "ポジション情報を取得できませんでした。";
                        await context.PostAsync(message);
                    }
                }

                // 顧客がわからなかったら、ユーザーに聞いてBotの状態をRequestPositionVisualに遷移する
                context.ConversationData.SetValue<BotState>("state", BotState.RequestPositionVisual);
            }
            else
            {
                // 顧客がわからなかったら、ユーザーに聞いてBotの状態をRequestClientNameに遷移する
                message = "どのお客様について知りたいですか？";
                context.ConversationData.SetValue<BotState>("state", BotState.RequestClientName);
                await context.PostAsync(message);
            }
            
            context.Wait(MessageReceived);
        }

        public FxDialog()
        {
        }
        public FxDialog(ILuisService service)
            : base(service)
        {
        }
    }
}

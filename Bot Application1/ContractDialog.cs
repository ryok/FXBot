using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Bot_Application1
{
    [Serializable]
    public class ContractDialog : IDialog<object>
    {
        // 会話を始めた時刻
        protected DateTime PrevTime;
        protected bool ResetTime = false;
        public string BondType { get; set; }
        public int Age { get; set; }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(InputBondTypeAsync);
        }

        public virtual async Task InputBondTypeAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            if (ResetTime)
            {
                this.PrevTime = DateTime.Now;
                this.ResetTime = false;
            }
            var activity = await argument;
            this.BondType = activity.Text;
            await context.PostAsync($"こんにちは{this.BondType}さん。続けて年齢を入力してください");
            context.Wait(this.InputAgeAsync);
            /*var message = await argument;
            //var usage = ElectricityUsageAPI.GetElectricityUsage("tokyo");
            PromptDialog.Confirm(
                context,
                ContractConfirmAsync,
                "約定を確定します。よろしいですか？",
                "わかりませんでした。「はい」か「いいえ」でお答えください。",
                promptStyle: PromptStyle.None);*/
        }

        private async Task InputAgeAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;
            int age;
            if (int.TryParse(activity.Text ?? "", out age))
            {
                this.Age = age;
                await context.PostAsync($"こんにちは{this.Age}歳の{this.BondType}さん");
                context.Done((object)null);
            }
            else
            {
                await context.PostAsync($"年齢は数字で入力してください");
                context.Wait(this.InputAgeAsync);
            }
        }

        /*public async Task ContractConfirmAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                await context.PostAsync("OK");
            } else
            {
                await context.PostAsync(string.Format("ではやめておきますね。余談ですが、あなた{0}秒間迷っていましたよ。", (int)(DateTime.Now - this.PrevTime).TotalSeconds));
            }
            context.ConversationData.SetValue("state", BotState.Default);
            this.ResetTime = true;
            context.Wait(InputAgeAsync);
        }

        public enum BondTypeOption
        {
            売現, 買現, 現先, 利含み現先, 先決め, 後決め
        }

        [Serializable]
        public class ContractQuery
        {
            [Describe("現先種別")]
            [Prompt("どの現先種別ですか？{||}")]
            public BondTypeOption? BondType;

            public static IForm<ContractQuery> BuildForm()
            {
                return new FormBuilder<ContractQuery>()
                    .Field(nameof(BondType))
                    .Confirm("{BondType}ですね。よろしいですか？")
                    .OnCompletion(ReportUsage)
                    .Build();
            }

            private static async Task ReportUsage(IDialogContext context, ContractQuery query)
            {
                // var usage = ElectricityUsageAPI.GetElectricityUsage(query.Area.GetValueOrDefault().ToEnglishString());
                await context.PostAsync("約定しました。");
                context.ConversationData.SetValue("state", BotState.Default);
                //conversationData.SetProperty<BotState>("state", BotState.Default);
            }
        }*/
    }
}

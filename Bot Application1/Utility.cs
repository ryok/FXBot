using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Drawing;
using Newtonsoft.Json;
using System.Text;

namespace Bot_Application1
{
    public class FxAPI
    {
        // 外為オンラインのAPIからレート情報取得
        public static float? GetFxRate(string ccyPair)
        {
            float? result = null;
            try
            {
                var requestUrl = @"http://www.gaitameonline.com/rateaj/getrate";
                var request = WebRequest.Create(requestUrl);
                var response = request.GetResponse();
                var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var json = JObject.Parse(rawJson);
                var jsonQuotes = json["quotes"];
                foreach (var obj in jsonQuotes)
                {
                    var currencyPairCode = obj["currencyPairCode"];
                    if (currencyPairCode.ToString() == ccyPair)
                    {
                        var free = obj["open"];
                        result = (float)free;
                    }
                }
            }
            catch (System.Net.WebException ex)
            {
                System.Console.WriteLine(ex);
            }
            return result;
        }

        // 外為オンラインAPIにgetする際のURL指定形式に変換するメソッド
        public static string GetCurrencyPairFromEntity(string currencyCode)
        {
            string returnValue = null;
            string upperCode = currencyCode.ToUpper().Replace("/", "");

            if (upperCode.Contains("ドル円") || upperCode.Contains("USDJPY"))
            {
                returnValue = "USDJPY";
            }
            else if (upperCode.Contains("ユーロ円") || upperCode.Contains("EURJPY"))
            {
                returnValue = "EURJPY";
            }
            else if (upperCode.Contains("GBPNZD"))
            {
                returnValue = "GBPNZD";
            }
            else if (upperCode.Contains("CADJPY"))
            {
                returnValue = "CADJPY";
            }
            else if (upperCode.Contains("GBPAUD"))
            {
                returnValue = "GBPAUD";
            }
            else if (upperCode.Contains("AUDJPY"))
            {
                returnValue = "AUDJPY";
            }
            else if (upperCode.Contains("AUDNZD"))
            {
                returnValue = "AUDNZD";
            }
            else if (upperCode.Contains("EURCAD"))
            {
                returnValue = "EURCAD";
            }
            else if (upperCode.Contains("ユーロドル") || upperCode.Contains("EURUSD"))
            {
                returnValue = "EURUSD";
            }
            else if (upperCode.Contains("NZDJPY"))
            {
                returnValue = "NZDJPY";
            }
            else if (upperCode.Contains("GBPAUD"))
            {
                returnValue = "GBPAUD";
            }
            else if (upperCode.Contains("USDCAD"))
            {
                returnValue = "USDCAD";
            }
            else if (upperCode.Contains("EURGBP"))
            {
                returnValue = "EURGBP";
            }
            else if (upperCode.Contains("GBPUSD"))
            {
                returnValue = "GBPUSD";
            }
            else if (upperCode.Contains("ZARJPY"))
            {
                returnValue = "ZARJPY";
            }
            else if (upperCode.Contains("EURCHF"))
            {
                returnValue = "EURCHF";
            }
            else if (upperCode.Contains("CHFJPY"))
            {
                returnValue = "CHFJPY";
            }
            else if (upperCode.Contains("AUDUSD"))
            {
                returnValue = "AUDUSD";
            }
            else if (upperCode.Contains("USDCHF"))
            {
                returnValue = "USDCHF";
            }
            else if (upperCode.Contains("GBPCHF"))
            {
                returnValue = "GBPCHF";
            }
            else if (upperCode.Contains("EURNZD"))
            {
                returnValue = "EURNZD";
            }
            else if (upperCode.Contains("NZDUSD"))
            {
                returnValue = "NZDUSD";
            }
            else if (upperCode.Contains("EURAUD"))
            {
                returnValue = "EURAUD";
            }
            else if (upperCode.Contains("AUDCHF"))
            {
                returnValue = "AUDCHF";
            }
            else if (upperCode.Contains("GBPJPY"))
            {
                returnValue = "GBPJPY";
            }
            return returnValue;
        }

        // デモ用顧客毎ハリボテポジション返却メソッド
        public static string GetPosition(string client)
        {
            string result = null;
            if (client == "A証券")
            {
                result = "出来3件、仮押さえ1件";
            }
            else if (client == "B銀行")
            {
                result = "出来10件、仮押さえなし";
            }
            return result;
        }
    }

    public class PhotoSearchAPI
    {
        // デモ用グラフ画像（固定）返却メソッド
        public static string GetPhotoByKeyword(string searchWords)
        {
            string result = null;
            try
            {
                // yahoo api
                // var appId = "dj0zaiZpPU1ET0lMbHpib0p4eiZzPWNvbnN1bWVyc2VjcmV0Jng9YzQ-";
                // var requestUrl = @"http://shinsai.yahooapis.jp/v1/Archive/search?appid="+ appId + @"&area=" + keyword + @"&output=json&results=1";

                // google custom search api
                // var apiKey = "AIzaSyAcNOrGio720Zhv8wACsM5YLEy8PDUEjco";
                // var customSearchEngineId = "016626523806187782504:c6xswr647li";
                // var requestUrl = @"https://www.googleapis.com/customsearch/v1?key=" + apiKey + @"&cx=" + customSearchEngineId + @"&q=" + searchWords + @"&searchType=image";

                // var request = WebRequest.Create(requestUrl);
                // var response = request.GetResponse();
                // var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();
                // var json = JObject.Parse(rawJson);

                // yahoo api
                // result = (string)json["ArchiveData"]["Result"][0]["PhotoData"]["OriginalUrl"];

                // google custom search api
                // result = (string)json["items"][0]["link"];

                //固定（デモ用）
                if (searchWords == "bar")
                {
                    result = "http://wb-ryok.azurewebsites.net/wp-content/uploads/2017/03/barGraph.png";
                }
                else
                {
                    result = "http://wb-ryok.azurewebsites.net/wp-content/uploads/2017/03/circleGraph.png";
                }

                return result;
            }
            catch
            {
                // Do nothing.
            }
            return result;
        }
    }

    public class ZatsudanAPI
    {
        // デモ用　雑談の返答を返却するメソッド
        public static Reply GetZatsudanConversation(string speach, string context)
        {
            SpeakTo speak;

            speak = new SpeakTo { utt = speach, age = "33", birthdateD = "20", birthdateM = "1", birthdateY = "1984", bloodtype = "A", constellations = "水瓶座", context = "", mode = "dialog", nickname = "DXC", nickname_y = "ディーエックスシー", place = "東京", sex = "男" };

            string jsonString = JsonConvert.SerializeObject(speak);

            string url = "https://api.apigw.smt.docomo.ne.jp/dialogue/v1/dialogue?APIKEY=6278654b57316d69325a364a4d6a67614e7131326332704e6c70494e63774637426b505359486634716137";

            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(url);

            // Set the Method property of the request to POST.
            request.Method = "POST";

            // Create POST data and convert it to a byte array.
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);

            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/json";

            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            // Get the request stream.
            Stream dataStream = request.GetRequestStream();

            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);

            // Close the Stream object.
            dataStream.Close();

            // Get the response.
            WebResponse response = request.GetResponse();

            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();

            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);

            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            // Display the content.
            Console.WriteLine(responseFromServer);

            // Clean up the streams.
            reader.Close();

            dataStream.Close();

            response.Close();

            //テキストをJSONオブジェクトに変換
            Reply ReplyJson = JsonConvert.DeserializeObject<Reply>(responseFromServer);

            return ReplyJson;
        }
    }
}

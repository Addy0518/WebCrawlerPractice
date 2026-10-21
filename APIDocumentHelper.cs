using System;
using System.Collections.Generic;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;

namespace Console1
{
    public static class APIDocumentHelper
    {
        public static async Task<IDocument> GetDocument(HttpClient httpClient, IBrowsingContext context, string url)
        {
            // 在 Get 呼叫時可能會發生逾時或伺服器出問題 , 這時候就要重新呼叫  , 但不能呼叫太多次以免給網站壓力
            int maxRetry = 3;

            for (int attempt = 0; attempt <= maxRetry; attempt++)
            {
                try
                {
                    //發送 GET 請求並取得回應內容
                    var responseMessage = await httpClient.GetAsync(url);

                    //檢查回應的伺服器狀態StatusCode是否是200 OK
                    if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        //讀取Content內容
                        string responseResult = await responseMessage.Content.ReadAsStringAsync();

                        // 字串丟給 AngleSharp 解析
                        return await context.OpenAsync(res => res.Content(responseResult));
                    }

                    int code = (int)responseMessage.StatusCode;
                    bool canRetry = code >= 500 || code == 429;
                    if (!canRetry)
                    {
                        Console.WriteLine("請求失敗(不重試):" + responseMessage.StatusCode);
                        return null;
                    }
                    Console.WriteLine($"第 {attempt} 次失敗 : {responseMessage.StatusCode}");
                }
                // 網路層錯誤(斷線、DNS 失敗等)
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"第 {attempt} 次網路錯誤:{ex.Message}");
                }
                // HttpClient 逾時會丟這個例外
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"第 {attempt} 次逾時");
                }
            }
            Console.WriteLine("已達重試上限,放棄");
            return null;
        }

        public static async Task<IDocument> PostDocument(
            HttpClient httpClient,
            IBrowsingContext context,
            string url,
            FormUrlEncodedContent form
        )
        {
            //發送 POST 請求並取得回應內容
            var responseMessage = await httpClient.PostAsync(url, form);

            //檢查回應的伺服器狀態StatusCode是否是200 OK
            if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("請求失敗:" + responseMessage.StatusCode);
                return null;
            }

            //讀取Content內容
            string responseResult = await responseMessage.Content.ReadAsStringAsync();

            // 字串丟給 AngleSharp 解析
            return await context.OpenAsync(res => res.Content(responseResult));
        }
    }
}

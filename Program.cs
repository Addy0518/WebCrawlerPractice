using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Playwright;

namespace Console1
{
    class Program
    {
        static void Main(string[] args)
        {
            // // 讓主控台用 UTF-8 輸出 , 因為可能對方網頁文字是簡體 , 預防缺字
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 準備好發請求的 HttpClient
            HttpClient httpClient = new HttpClient();

            // 如果送出的請求超過 15 秒還沒完成 , 就丟逾時警告
            httpClient.Timeout = TimeSpan.FromSeconds(15);

            // Configuration.Default : AngleSharp 解析器設定 , 能夠解析 DOM 但不會把它渲染出來 , 也就是會把 < script > 這種 JS 標籤忽略
            var config = Configuration.Default;

            // BrowsingContext : AngleSharp 環境設定 , 用 config 設定建立一個瀏覽環境 , 負責管理文件的載入與解析,之後用它的 OpenAsync 把 HTML 變成可查詢的 DOM
            var context = BrowsingContext.New(config);

            Console.WriteLine("請輸入數獲取網站資訊 : ");
            string input = Console.ReadLine();

            if (input == "1")
            {
                GetDOM1(httpClient, context).Wait();
            }
            else if (input == "2")
            {
                GetDOM2(httpClient, context).Wait();
            }
            else if (input == "3")
            {
                GetDOM3(httpClient, context).Wait();
            }
            else if (input == "4")
            {
                GetDOM4(httpClient).Wait();
            }
            else if (input == "5")
            {
                GetDOM5(httpClient, context).Wait();
            }
            else if (input == "6")
            {
                GetDOM6(httpClient, context).Wait();
            }
            else if (input == "7")
            {
                GetDOM7().Wait();
            }
            else if (input == "8")
            {
                GetDOM8(httpClient).Wait();
            }
            else if (input == "9")
            {
                GetDOM9(httpClient, context).Wait();
            }
            else if (input == "10")
            {
                GetDOM10(httpClient, context).Wait();
            }
            else if (input == "111")
            {
                Test1(httpClient, context).Wait();
            }
            else
            {
                Console.WriteLine("輸入錯誤 !");
            }
        }

        /// <summary>
        /// 拿到法律網站的 DOM 資料
        /// </summary>
        /// <param name="httpClient">HTTP 請求物件</param>
        /// <param name="context">瀏覽環境</param>
        /// <returns>法律資料</returns>
        public static async Task GetDOM1(HttpClient httpClient, IBrowsingContext context)
        {
            // 目標網址
            string url = "https://law.moj.gov.tw/";

            var document = await APIDocumentHelper.GetDocument(httpClient, context, url);
            if (document == null)
            {
                Console.WriteLine("請求失敗");
                return;
            }

            // 只印所有連結的文字與網址
            foreach (var a in document.QuerySelectorAll("table.home-news tr"))
            {
                var cells = a.QuerySelectorAll("td");
                if (cells.Length < 3)
                    continue;
                string date = cells[0].TextContent.Trim();
                string type = cells[1].TextContent.Trim();
                string title = cells[2].TextContent.Trim();
                string href = cells[2].QuerySelector("a")?.GetAttribute("href");

                Console.WriteLine($"{date} | {type} | {title} | https://law.moj.gov.tw{href}\r\n");
            }
        }

        /// <summary>
        /// 拿到書籍網站的 DOM 資料
        /// </summary>
        /// <param name="httpClient">HTTP 請求物件</param>
        /// <param name="context">瀏覽環境</param>
        /// <returns>書籍資料</returns>
        public static async Task GetDOM2(HttpClient httpClient, IBrowsingContext context)
        {
            // 目標網址
            string url = "https://books.toscrape.com/";

            var document = await APIDocumentHelper.GetDocument(httpClient, context, url);
            if (document == null)
            {
                Console.WriteLine("請求失敗");
                return;
            }

            // 只印所有連結的文字與網址
            foreach (var a in document.QuerySelectorAll(".row article"))
            {
                var cells = a.QuerySelectorAll(".image_container");

                string img = cells[0].QuerySelector("img")?.GetAttribute("alt");
                string href = cells[0].QuerySelector("a")?.GetAttribute("href");

                Console.WriteLine($"{img} | https://books.toscrape.com{href}\r\n");
            }
        }

        /// <summary>
        /// 登入名言大全網站 + 拿到 DOM 資料
        /// </summary>
        /// <param name="httpClient">HTTP 請求物件</param>
        /// <param name="context">瀏覽環境</param>
        /// <returns>名言資料</returns>
        public static async Task GetDOM3(HttpClient httpClient, IBrowsingContext context)
        {
            // 登入網址
            string loginUrl = "https://quotes.toscrape.com/login";

            // 目標網址
            string targetUrl = "https://quotes.toscrape.com/";

            var loginDocument = await APIDocumentHelper.GetDocument(httpClient, context, loginUrl);
            if (loginDocument == null)
            {
                Console.WriteLine("請求失敗");
                return;
            }

            // 要先登入所以從剛剛拿到的網頁資料拿到 csrf_token ( 這個是 防跨站請求偽造 , 不是登入的 token )
            string token = loginDocument.QuerySelector("input[name='csrf_token']").GetAttribute("value");

            var form = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "csrf_token", token },
                    { "username", "test" },
                    { "password", "test" },
                }
            );

            // Post 登入表單
            var loginResponse = await APIDocumentHelper.PostDocument(httpClient, context, loginUrl, form);

            // 這裡才會看到 token 是否成功生成
            bool ok = loginResponse.QuerySelector("a[href='/logout']") != null;
            Console.WriteLine(ok ? "登入成功" : "登入失敗");

            // 計算總共有幾頁名言大全的資料
            var count = 0;

            // 循環把每一頁資料都抓下來
            while (targetUrl != null)
            {
                var document = await APIDocumentHelper.GetDocument(httpClient, context, targetUrl);
                if (document == null)
                {
                    Console.WriteLine("請求失敗");
                    return;
                }
                foreach (var a in document.QuerySelectorAll(".quote"))
                {
                    string content = a.TextContent.Trim();
                    string author = a.QuerySelector("small").TextContent.Trim();
                    Console.WriteLine($"作者 : {author} \r\n 內容 : {content} ");
                }

                // 找到下一頁的連結
                var nextPageLink = document.QuerySelector("li.next a")?.GetAttribute("href");
                targetUrl = nextPageLink == null ? null : "https://quotes.toscrape.com" + nextPageLink;

                count++;

                // 延遲一下怕對網站造成太大壓力
                await Task.Delay(1000);
            }

            Console.WriteLine($"總共 {count} 頁");
        }

        /// <summary>
        /// 拿到名言大全網站的 DOM 資料 ( 同時拿好幾頁 )
        /// </summary>
        /// <param name="httpClient">HTTP 請求物件</param>
        /// <returns>名言資料</returns>
        public static async Task GetDOM4(HttpClient httpClient)
        {
            // SemaphoreSlim : 建立三個名額跑執行緒
            var semaPhore = new SemaphoreSlim(3);

            // 查看抓取資料的時間
            var sw = Stopwatch.StartNew();

            // 因為目標網站總共有十頁 , 所以 Range 設定 1 ~ 10 並丟進 url
            var tasks = Enumerable
                .Range(1, 10)
                .Select(async page =>
                {
                    // 拿一個名額,沒有就非同步等待(不卡執行緒)
                    await semaPhore.WaitAsync();

                    try
                    {
                        // 這裡我又建立一次 AngleSharp 的環境設定的原因是因為之前都是使用一個 context
                        // 但現在是要同時用好幾個 , 怕會出問題所以每次執行就建立一個
                        var context = BrowsingContext.New(Configuration.Default);

                        // 目標網址
                        string url = $"https://quotes.toscrape.com/page/{page}/";

                        var document = await APIDocumentHelper.GetDocument(httpClient, context, url);

                        // 建立放資料的 list
                        var list = new List<string>();

                        if (document == null)
                        {
                            Console.WriteLine("請求失敗");
                            return list;
                        }

                        foreach (var a in document.QuerySelectorAll(".quote"))
                        {
                            string content = a.QuerySelector(".text")?.TextContent.Trim();
                            string author = a.QuerySelector("small.author")?.TextContent.Trim();
                            list.Add($"[第 {page} 頁] {author} : {content}");
                        }

                        // 稍微讓他延遲一下以免呼叫太快打爆對面的 API
                        await Task.Delay(500);
                        return list;
                    }
                    finally
                    {
                        // 不管怎樣 , 最後都要釋放名額等下一個進來
                        semaPhore.Release();
                    }
                });

            // 執行緒同時進行
            var results = await Task.WhenAll(tasks);

            foreach (var pageResult in results)
            foreach (var line in pageResult)
                Console.WriteLine(line);

            Console.WriteLine($"共 {results.Sum(r => r.Count)} 筆 , 耗時 {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// 拿到名言大全網站的 DOM 資料 ( JS 動態網頁 )
        /// </summary>
        /// <param name="httpClient">HTTP 請求物件</param>
        /// <returns>名言資料</returns>
        public static async Task GetDOM5(HttpClient httpClient, IBrowsingContext context)
        {
            // 建立 Playwright , 用來讀取有 JS 的動態網站
            // 概念就是開一個網頁 , 先拿對方網頁的空殼 => < scripts >
            // 等對方網頁的 js 執行完有資料後 , 這邊的網頁就會把它塞過來 , 然後一樣交給 AngleSharp 讀取
            using var playWright = await Playwright.CreateAsync();

            // 啟動瀏覽器 Chromium ( 就是 Chrome 的開源版本 ) , LaunchAsync 則是讓瀏覽器在背景執行 , 不會跳視窗
            // await using 是非同步釋放 , 這樣才不會在背景殘留 Chromium 程序
            await using var browser = await playWright.Chromium.LaunchAsync();

            // 並在瀏覽器開一個分頁 , 之後的操作都在這下
            var page = await browser.NewPageAsync();

            // 從這個分業去目標網址拿 HTML 空殼
            await page.GotoAsync("https://quotes.toscrape.com/js/");

            // 這裡在等待 JS 跑完拿裡面資料
            await page.WaitForSelectorAsync(".quote");

            // 並最後在這裡拿 HTML 資料出來
            string html = await page.ContentAsync();

            // 接下來就一樣用 AngleSharp 抓資料出來
            var document = await context.OpenAsync(res => res.Content(html));
            foreach (var a in document.QuerySelectorAll(".quote"))
            {
                string content = a.QuerySelector(".text")?.TextContent.Trim();
                string author = a.QuerySelector("small.author")?.TextContent.Trim();
                Console.WriteLine($"{author} : {content}");
            }
        }

        /// <summary>
        /// 下載書籍網站的圖片
        /// </summary>
        /// <param name="httpClient">HTTP 請求物件</param>
        /// <param name="context">瀏覽環境</param>
        /// <returns>書籍資料</returns>
        public static async Task GetDOM6(HttpClient httpClient, IBrowsingContext context)
        {
            // 目標網址
            string url = "https://books.toscrape.com/";

            // 指定要存放的資料夾
            string saveFolder = "C:\\Users\\andychen\\Desktop\\爬蟲練習\\images";

            // 沒有資料夾的話就創建一個
            Directory.CreateDirectory(saveFolder);

            var document = await APIDocumentHelper.GetDocument(httpClient, context, url);
            if (document == null)
            {
                Console.WriteLine("請求失敗");
                return;
            }

            // Uri : 網址的型別 , 這裡把目標網址包成 Uri,當作「基準網址」,用來把相對路徑補成完整網址 ( https://books.toscrape.com/ )
            var baseUri = new Uri(url);

            // 紀錄下載幾張圖片
            int count = 0;

            // 只印所有連結的文字與網址
            foreach (var img in document.QuerySelectorAll("article img"))
            {
                // 拿到圖片連結名稱
                string src = img.GetAttribute("src");
                if (string.IsNullOrEmpty(src))
                    continue;

                // 用 Uri 組合基底網址跟圖片名稱 , 合成完整的路徑 ( https://books.toscrape.com/media/cache/2c/da/xxx.jpg )
                var imgUri = new Uri(baseUri, src);

                // LocalPath 取出網址中的路徑 ( 例如 /media/cache/2c/da/xxx.jpg  (不含 ?id=1 這類參數) )
                string fileName = Path.GetFileName(imgUri.LocalPath);

                // 再結合存取位置與檔名合成完整路徑
                string filePath = Path.Combine(saveFolder, fileName);

                if (File.Exists(filePath))
                    continue;

                try
                {
                    // GetByteArrayAsync : 把拿到的內容讀成二進位資料
                    byte[] data = await httpClient.GetByteArrayAsync(imgUri);

                    // WriteAllBytesAsync: 把 byte[] 寫成硬碟上的實體檔案(非同步版本)
                    await File.WriteAllBytesAsync(filePath, data);

                    count++;
                    Console.WriteLine($"已下載:{fileName}");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"下載失敗 {imgUri}:{ex.Message}");
                }
                await Task.Delay(300);
            }
            Console.WriteLine($"共下載 {count} 張,位置:{saveFolder}");
        }

        /// <summary>
        /// 拿到名言大全網站的 DOM 資料 ( 開模擬瀏覽器呼叫對面的 API , 模擬使用者再用網站時的動作 )
        /// </summary>
        /// <returns>名言資料</returns>
        public static async Task GetDOM7()
        {
            using var playWright = await Playwright.CreateAsync();
            await using var browser = await playWright.Chromium.LaunchAsync();
            var page = await browser.NewPageAsync();

            // 因為 API 會傳 JSON 字串 , 所以要用字串集合去接
            var jsonList = new List<string>();

            // 1. Response : Playwright 的事件 , 只要瀏覽器收到 response ( HTML , 圖片 ... ) 就觸發
            // 2. += : 訂閱事件 , 每次觸發就執行程式碼
            // 3. (_, response):兩個參數,第一個是事件發送者 ( 用不到,用 _ 表示忽略 ) ,
            //                          第二個 response 是這次收到的回應物件,包含網址、狀態碼、內容
            page.Response += async (_, response) =>
            {
                // 只需要網址含 /api/quotes ( 這個網站呼叫的 API )
                if (response.Url.Contains("/api/quotes") && response.Status == 200)
                {
                    // TextAsync 再把 JSON 回傳的內容讀成字串 , 加到 List , 要記得 lock 不然會同時寫入
                    string json = await response.TextAsync();
                    lock (jsonList)
                    {
                        jsonList.Add(json);
                    }
                }
            };

            // 這時候才前往目標頁面 , 這樣才不會進入網站後還沒設定訂閱事件漏掉 API
            await page.GotoAsync("https://quotes.toscrape.com/scroll");

            // 在我這裡的頁面模擬使用者往下滑的動作 , 因為我抓的這個網站是網頁往下滑就會呼叫 API 抓新資料 ( 無限捲動頁 )
            // 所以這裡模擬我有去往下滑 , 對面網頁就會一直呼叫 API
            for (int i = 0; i < 3; i++)
            {
                // 滑鼠往下滑 ( y 移動 10000 像素 )
                await page.Mouse.WheelAsync(0, 10000);

                // 並固定等待 1.5 秒 , 確保 API　有時間完成並觸發訂閱事件
                await page.WaitForTimeoutAsync(1500);
            }

            // 再從 JSON 讀資料
            foreach (var json in jsonList)
            {
                // JsonDocument.Parse : 將 Json 轉成可讀取的結構
                using var doc = JsonDocument.Parse(json);
                foreach (var q in doc.RootElement.GetProperty("quotes").EnumerateArray())
                {
                    string text = q.GetProperty("text").GetString();
                    string author = q.GetProperty("author").GetProperty("name").GetString();
                    Console.WriteLine($"{author} : {text}");
                }
            }
            Console.WriteLine($"攔截到 {jsonList.Count} 個 API 回應");
        }

        /// <summary>
        /// 拿到名言大全網站的 DOM 資料 ( 不開模擬瀏覽器直接呼叫對面的 API , 比模擬瀏覽器快 , 也是比較常見的方法 , 就是要先確認對方 API 是傳什麼參數 )
        /// </summary>
        /// <returns>名言資料</returns>
        public static async Task GetDOM8(HttpClient httpClient)
        {
            int page = 1;
            int total = 0;

            while (true)
            {
                string json = await httpClient.GetStringAsync($"https://quotes.toscrape.com/api/quotes?page={page}");
                using var doc = JsonDocument.Parse(json);

                // 讀取 JSON
                // RootElement : JSON 最外層那個括號 =>  { ... }
                // GetProperty : 拿欄位叫做 qutoes 的參數
                // EnumerateArray : 再把這個陣列拆成集合來跑 foreach
                foreach (var q in doc.RootElement.GetProperty("quotes").EnumerateArray())
                {
                    string text = q.GetProperty("text").GetString();
                    string author = q.GetProperty("author").GetProperty("name").GetString();
                    Console.WriteLine($"{author} : {text}");
                    total++;
                }

                // 對面網站的 API 傳的其中一個參數是 has_next ( 是否有下一頁 ) , 用這個判斷有沒有下一頁 , 有的話就繼續跑 while
                if (!doc.RootElement.GetProperty("has_next").GetBoolean())
                    break;

                page++;
                // 延遲一下怕對網站造成太大壓力
                await Task.Delay(1000);
            }

            Console.WriteLine($"共 {total} 筆,{page} 頁");
        }

        /// <summary>
        /// 拿到名言大全網站的 DOM 資料 ( 舊版網站 , WebForms )
        /// </summary>
        /// <returns>名言資料</returns>
        public static async Task GetDOM9(HttpClient httpClient, IBrowsingContext context)
        {
            // 目標網址
            string targetUrl = "https://quotes.toscrape.com/search.aspx";

            var document = await APIDocumentHelper.GetDocument(httpClient, context, targetUrl);
            if (document == null)
            {
                Console.WriteLine("請求失敗");
                return;
            }

            // Dictionary < 欄位名稱 , 欄位值 > => 用來裝要 Post 給對面網站的資料
            var fields = new Dictionary<string, string>();

            // 找出所有表單的隱藏欄位 , 因為 WebForms 是舊系統 , 會把伺服器頁面狀態放在隱藏欄位 , post 時要帶這些給對面不然會被擋住
            foreach (var input in document.QuerySelectorAll("form input[type='hidden']"))
            {
                // hideName : 隱藏的欄位 , 把它塞到 fields 字典裡準備 Post 回去
                string hideName = input.GetAttribute("name");
                if (!string.IsNullOrEmpty(hideName))
                    fields[hideName] = input.GetAttribute("value") ?? "";
            }

            // 其他欄位則是我們要選的欄位 , 所以不用迴圈而是直接指定
            fields["author"] = "J.K. Rowling";
            fields["tag"] = "abilities";
            fields["submit_filter"] = "Search";

            var result = await APIDocumentHelper.PostDocument(
                httpClient,
                context,
                "https://quotes.toscrape.com/filter.aspx",
                new FormUrlEncodedContent(fields)
            );
            if (result == null)
                return;

            // 最後一樣依照格式讀取
            foreach (var q in result.QuerySelectorAll(".results .quote"))
            {
                string content = q.QuerySelector(".content")?.TextContent.Trim();
                string author = q.QuerySelector(".author")?.TextContent.Trim();
                string tag = q.QuerySelector(".tag")?.TextContent.Trim();
                Console.WriteLine($"{author} ({tag}) : {content}");
            }
        }

        /// <summary>
        /// 練習
        /// </summary>
        /// <returns>資料</returns>
        public static async Task GetDOM10(HttpClient httpClient, IBrowsingContext context)
        {
            // 目標網址
            string url = "https://zh.moegirl.org.cn/%E7%89%A7%E6%BF%91%E7%BA%A2%E8%8E%89%E6%A0%96";

            // 指定要存放的資料夾
            string saveFolder = "C:\\Users\\andychen\\Desktop\\爬蟲練習\\images";

            // 沒有資料夾的話就創建一個
            Directory.CreateDirectory(saveFolder);

            var document = await APIDocumentHelper.GetDocument(httpClient, context, url);
            if (document == null)
            {
                Console.WriteLine("請求失敗");
                return;
            }

            // 一般來說 , AngleSharp 的 QuerySelector 回傳的會是 IElement ( 通用所有屬性 , TextContent、GetAttribute ... 等等 )
            // 但 template 不一樣 , 他有 Content 這個屬性 , 所以這裡要把它轉型成 IHtmlTemplateElement
            var temp = document.QuerySelector("#MOE_SKIN_TEMPLATE_BODYCONTENT") as IHtmlTemplateElement;
            if (temp == null)
            {
                Console.WriteLine("找不到 template");
                return;
            }

            Console.WriteLine(document.Title.Trim());

            // Uri : 網址的型別 , 這裡把目標網址包成 Uri,當作「基準網址」,用來把相對路徑補成完整網址
            var baseUri = new Uri(url);

            // 紀錄下載幾張圖片
            int count = 0;

            // 只印所有連結的文字與網址
            foreach (var img in temp.Content.QuerySelectorAll("img"))
            {
                // 拿到圖片連結名稱
                string src = img.GetAttribute("src");
                if (string.IsNullOrEmpty(src))
                    continue;

                // 用 Uri 組合基底網址跟圖片名稱 , 合成完整的路徑
                var imgUri = new Uri(baseUri, src);

                string path = imgUri.LocalPath;
                int bang = path.IndexOf('!');
                if (bang >= 0)
                    path = path.Substring(0, bang);

                string fileName = Path.GetFileName(path);

                // 再結合存取位置與檔名合成完整路徑
                string filePath = Path.Combine(saveFolder, fileName);

                if (File.Exists(filePath))
                    continue;

                try
                {
                    // GetByteArrayAsync : 把拿到的內容讀成二進位資料
                    byte[] data = await httpClient.GetByteArrayAsync(imgUri);

                    // WriteAllBytesAsync: 把 byte[] 寫成硬碟上的實體檔案(非同步版本)
                    await File.WriteAllBytesAsync(filePath, data);

                    count++;
                    Console.WriteLine($"已下載:{fileName}");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"下載失敗 {imgUri}:{ex.Message}");
                }
                await Task.Delay(300);
            }
            Console.WriteLine($"共下載 {count} 張,位置:{saveFolder}");
        }

        /// <summary>
        /// 測試
        /// </summary>
        /// <param name="httpClient">HTTP 請求物件</param>
        /// <param name="context">瀏覽環境</param>
        /// <returns></returns>
        public static async Task Test1(HttpClient httpClient, IBrowsingContext context)
        {
            // 目標網址
            string url = "https://httpbin.org/status/404";

            var document = await APIDocumentHelper.GetDocument(httpClient, context, url);
            if (document == null)
            {
                Console.WriteLine("請求失敗");
                return;
            }

            Console.ReadKey();
        }
    }
}

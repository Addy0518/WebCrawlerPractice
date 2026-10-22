using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Console1
{
    public class QuoteDetailsRow
    {
        [DisplayName("書名")]
        public string BookName { get; set; }

        [DisplayName("評分星級")]
        public int Star { get; set; }

        [DisplayName("價格")]
        public string Price { get; set; }

        [DisplayName("是否有庫存")]
        public string InStock { get; set; }

        [DisplayName("圖片名稱")]
        public string Img { get; set; }

        [DisplayName("圖片網址")]
        public string Herf { get; set; }
    }
}

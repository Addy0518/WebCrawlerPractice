using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Console1
{
    public class QuoteRow
    {
        [DisplayName("作者")]
        public string Author { get; set; }

        [DisplayName("內容")]
        public string Content { get; set; }

        [DisplayName("標籤")]
        public string Tags { get; set; }
    }
}

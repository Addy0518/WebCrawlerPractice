using System.ComponentModel;
using System.Reflection;
using NPOI.XSSF.UserModel;
using static Console1.ExtensionMethods.EnumExtension;

namespace UBOT_API.Common.Helpers
{
    public static class ExcelHelper
    {
        public static byte[] CreateExcel<T>(IEnumerable<T> dataList)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("爬蟲練習 - 名人佳句表");
            var properties = typeof(T).GetProperties();

            // 建立標題樣式
            var font = workbook.CreateFont();
            font.IsBold = true;
            var style = workbook.CreateCellStyle();
            style.SetFont(font);

            // 標題列
            var rowHeader = sheet.CreateRow(0);
            var colIndex = 0;
            foreach (var property in properties)
            {
                var cell = rowHeader.CreateCell(colIndex);
                var displayName = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? property.Name;
                cell.SetCellValue(displayName);
                cell.CellStyle = style;
                colIndex++;
            }

            // 資料列
            var rowNum = 1;
            foreach (var data in dataList)
            {
                var rowContent = sheet.CreateRow(rowNum);
                var colContentIndex = 0;
                foreach (var property in properties)
                {
                    var cellContent = rowContent.CreateCell(colContentIndex);
                    var value = property.GetValue(data, null);

                    var enumAttr = property.GetCustomAttribute<ExcelEnumAttribute>();

                    if (value == null)
                        cellContent.SetCellValue("");
                    else if (enumAttr != null)
                    {
                        var enumValue = (Enum)Enum.ToObject(enumAttr.EnumType, Convert.ToInt32(value));
                        cellContent.SetCellValue(enumValue.GetEnumDescription());
                    }
                    else if (property.PropertyType == typeof(string))
                        cellContent.SetCellValue(value.ToString());
                    else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                        cellContent.SetCellValue(Convert.ToInt32(value));
                    else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                        cellContent.SetCellValue(Convert.ToDouble(value));
                    else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        cellContent.SetCellValue(((DateTime)value).ToString("yyyy-MM-dd"));
                    else
                        cellContent.SetCellValue(value.ToString());

                    colContentIndex++;
                }
                rowNum++;
            }

            using var stream = new MemoryStream();
            workbook.Write(stream);
            return stream.ToArray();
        }
    }
}

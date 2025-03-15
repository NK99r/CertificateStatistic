using CertificateStatisticWPF.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace CertificateStatisticWPF.Tools
{
    internal class ExcelTool
    {
        /// <summary>
        /// 读取Excel文件里的内容
        /// </summary>
        /// <param name="filePath">Excel表格文件路径</param>
        /// <returns></returns>
        public static DataTable ReadExcelFile(string filePath)
        {
            DataTable dataTable = new DataTable();
            try
            {
                IWorkbook workbook = null;
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,FileShare.ReadWrite))
                {
                    //不同的格式的excel文件，需要NPOI创建不同类型的对象
                    if (filePath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(fs);
                    else if (filePath.EndsWith(".xls"))
                        workbook = new HSSFWorkbook(fs);
                    else return null;
                    //获得文件中的第一张表
                    var sheet = workbook.GetSheetAt(0);
                    //获得表头
                    var headerRow = sheet.GetRow(1);
                    foreach (var cell in headerRow)
                    {
                        //表头单元格可能为空，需要允许为null时不抛出异常
                        dataTable.Columns.Add(cell?.ToString());
                    }

                    //遍历所有数据行
                    for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
                    {
                        //根据索引获得行
                        var row = sheet.GetRow(rowIndex);

                        //防报错
                        if (row == null || row.ZeroHeight || row.Height == 0) continue;

                        //创建行对象
                        var dataRow = dataTable.NewRow();

                        //遍历本行中的每一单元格
                        for (int cellIndex = 0; cellIndex < headerRow.LastCellNum; cellIndex++)
                        {
                            //获得单元格中数据
                            var cell = row.GetCell(cellIndex);
                            dataRow[cellIndex] = GetCellValue(cell);
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取 Excel 文件时出错: {ex.Message}");
                return null;
            }
            return dataTable;
        }

        /// <summary>
        /// 处理Excel单元格中格式不正确的数据
        /// </summary>
        /// <param name="rawTable">表格数据集</param>
        /// <returns></returns>
        private static ObservableCollection<Certificate> ProcessData(DataTable rawTable)
        {
            var result = new ObservableCollection<Certificate>();

            foreach (DataRow row in rawTable.Rows)
            {
                //获取字段（对应Excel表头）
                string studentId = row["学号"].ToString();
                string name = row["姓名"].ToString();
                string category = row["申报类别"].ToString();
                //分割
                var dates = SplitDate(row["获奖时间（年/月）"].ToString());
                var projects = SplitProjects(row["申报项目名称/项目积分"].ToString());
                var organizers = SplitOrganizers(row["主办单位/发表刊物（期刊号）"].ToString());
                var eventLevels = SplitEventLevels(row["赛事级别"].ToString());

                //处理好的字段放到返回集合中
                //调用Max是防止主办单位和获奖项目计数不等
                for (int i = 0; i < Math.Max(projects.Count, organizers.Count); i++)
                {
                    //获取获奖项目名称，如果当前索引 i 小于获奖项目列表的长度，则获取对应的项目名称，否则设置为 "N/A" 和索引为 -1
                    var project = (i < projects.Count) ? projects[i] : (Text: "N/A", Index: -1);

                    //获取主办单位名称，如果当前索引 i 小于主办单位列表的长度，则获取对应的组织者名称，否则设置为 "N/A"
                    var organizer = (i < organizers.Count) ? organizers[i] : "N/A";

                    //获取获奖日期信息，如果当前索引 i 小于获奖日期列表的长度，则获取对应的日期信息，否则设置为 "N/A"
                    var date = (i < dates.Count) ? dates[i] : "N/A";

                    //匹配赛事级别：按项目序号查找，不存在则默认省部级
                    string eventLevel = eventLevels.TryGetValue(project.Index, out string level) ? level : "省部级";

                    result.Add(new Certificate
                    {
                        StudentID = studentId,
                        Name = name,
                        CertificateProject = project.Text,
                        Organizer = organizer,
                        Date = date,
                        EventLevel = eventLevel,
                        Category = category
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// 调用读取和处理方法，返回被处理的数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static ObservableCollection<Certificate> ReadAndProcessExcel(string filePath)
        {
            // 调用原始读取方法
            return ProcessData(ReadExcelFile(filePath));
        }

        /// <summary>
        /// 导出数据到 Excel 文件
        /// </summary>
        /// <param name="data">要导出的数据集合</param>
        /// <param name="filePath">保存路径</param>
        public static void ExportToExcel(ObservableCollection<Certificate> data, string filePath)
        {
            // 创建工作簿
            IWorkbook workbook;

            if (filePath.EndsWith(".xlsx"))
            {
                workbook = new XSSFWorkbook(); // .xlsx 文件
            }
            else
            {
                workbook = new HSSFWorkbook(); // .xls 文件
            }

            // 创建工作表
            ISheet sheet = workbook.CreateSheet("Sheet1");

            //设置列宽
            sheet.SetColumnWidth(0, 15 * 256);
            sheet.SetColumnWidth(1, 10 * 256);
            sheet.SetColumnWidth(2, 50 * 256);
            sheet.SetColumnWidth(3, 10 * 256);
            sheet.SetColumnWidth(4, 10 * 256);
            sheet.SetColumnWidth(5, 30 * 256);
            sheet.SetColumnWidth(6, 10 * 256);

            // 添加表头
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("学号");
            headerRow.CreateCell(1).SetCellValue("姓名");
            headerRow.CreateCell(2).SetCellValue("获奖项目");
            headerRow.CreateCell(3).SetCellValue("类别");
            headerRow.CreateCell(4).SetCellValue("赛事级别");
            headerRow.CreateCell(5).SetCellValue("主办单位");
            headerRow.CreateCell(6).SetCellValue("获奖日期");

            // 添加数据行
            for (int i = 0; i < data.Count; i++)
            {
                IRow dataRow = sheet.CreateRow(i + 1);
                var item = data[i];

                dataRow.CreateCell(0).SetCellValue(item.StudentID);
                dataRow.CreateCell(1).SetCellValue(item.Name);
                dataRow.CreateCell(2).SetCellValue(item.CertificateProject);
                dataRow.CreateCell(3).SetCellValue(item.Category);
                dataRow.CreateCell(4).SetCellValue(item.EventLevel);
                dataRow.CreateCell(5).SetCellValue(item.Organizer);
                dataRow.CreateCell(6).SetCellValue(item.Date);
            }

            // 保存文件
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                workbook.Write(fs);
            }
        }

        /*
         * 正则表达式切割：
         * ^：匹配字符串的开始。
         * $：匹配字符串的结尾。
         * \d：匹配一个数字字符（等价于[0-9]）。
         * \D：匹配非数字字符。
         * \s：匹配任何空白字符（如空格、制表符等）。
         * \S：匹配任何非空白字符。
         * \w：匹配字母、数字和下划线（等价于[a-zA-Z0-9_]）。
         * \W：匹配非字母、非数字和非下划线字符。
         * +：匹配前面的元素一次或多次。
         * *：匹配前面的元素零次或多次。
         * ?：匹配前面的元素零次或一次，或者使量词变为非贪婪（匹配最少字符）。
         * {n}：匹配前面的元素恰好n次。
         * {n,}：匹配前面的元素至少n次。
         * {n,m}：匹配前面的元素至少n次，但最多m次。
         * []：字符集，匹配其中的任意一个字符。
         * |：表示“或者”操作，匹配左边或右边的模式。
         * ()：括号用于分组，捕获匹配的部分，方便后续使用。
         * ?：非贪婪匹配，尽可能少的匹配。
         */

        #region 正则表达式切割

        /// <summary>
        /// 使用正则表达式切割单独对日期做分割
        /// </summary>
        /// <param name="input">要被切割的字符串</param>
        /// <returns>填充好的列表</returns>
        private static List<string> SplitDate(string input)
        {
            //StringSplitOptions.RemoveEmptyEntries表示在分割时会忽略空行
            return input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(line =>
                       {
                           //移除开头的序号，保留斜杠后
                           return Regex.Replace(line.Trim(), @"^\d+[\.、]\s*", "");
                           /*
                                ^：表示匹配输入字符串的开始位置。
                                \d+：表示匹配一个或多个数字（\d是数字字符的简写）。
                                [\.、]：表示匹配一个“点”（.）或者“顿号”（、）。方括号表示字符集，匹配其中的任意一个字符。
                                \s*：表示匹配零个或多个空白字符（空格、制表符等）。\s表示任何空白字符，*表示零次或多次。
                           */
                       })
                       .Where(cleaned => !string.IsNullOrWhiteSpace(cleaned))
                       .ToList();
        }

        /// <summary>
        /// 使用正则表达式切割所有主办单位
        /// </summary>
        /// <param name="input">要被切割的字符串</param>
        /// <returns>填充好的列表</returns>
        private static List<string> SplitOrganizers(string input)
        {
            //按换行符（\n）进行分割
            //StringSplitOptions.RemoveEmptyEntries表示在分割时会忽略空行
            return input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(line =>
                       {
                           //去除空格后，行首序号替换为空格
                           return Regex.Replace(line.Trim(), @"^\d+[\.、]\s*|/\d+(\.\d+)?$", "");
                           /*
                                ^：匹配字符串的开始。
                                \d+：匹配一个或多个数字。
                                [\.、]：匹配一个“点”或“顿号”。
                                \s*：匹配零个或多个空白字符。
                                |：表示“或者”，匹配左边或右边的模式。
                                /\d+(\.\d+)?：匹配以“/”开头，后跟一个或多个数字，后面可能跟有小数点和更多数字。(\.\d+)?表示小数部分是可选的。
                                $：表示匹配输入字符串的结尾。
                           */
                       })
                       .Where(cleaned => !string.IsNullOrWhiteSpace(cleaned))   //过滤空行
                       .ToList();
        }

        /// <summary>
        /// 使用正则表达式切割所有赛事级别
        /// </summary>
        /// <param name="input">要被切割的字符串</param>
        /// <returns>填充好的字典</returns>
        private static Dictionary<int, string> SplitEventLevels(string input)
        {
            var levels = new Dictionary<int, string>();
            var entries = input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //不使用linq语句，因为linq语句不容易处理字典
            foreach (var entry in entries)
            {
                //提取条目中的数字和文本部分
                var match = Regex.Match(entry.Trim(), @"^(\d+)[\.、]\s*(.+)$");
                /*
                        ^：匹配字符串的开始。
                        (\d+)：匹配一个或多个数字，并将其捕获到一个分组中。
                        [\.、]：匹配一个“点”或“顿号”。
                        \s*：匹配零个或多个空白字符。
                        (.+)：匹配一个或多个任意字符，并将其捕获到第二个分组中。
                        $：匹配字符串的结尾。
                 */
                //如果匹配成功且数字部分可以成功转换为整数
                if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
                {
                    //将序号和对应的文本存入字典
                    levels[index] = match.Groups[2].Value.Trim(); 
                }
                else if (!string.IsNullOrWhiteSpace(entry))
                {
                    // 无序号条目视为默认值（例如 "省部级" 对应序号 -1）
                    levels[-1] = entry.Trim();
                }
            }

            return levels;
        }

        /// <summary>
        /// 依据换行符'\n' 将输入字符串分割成多个行
        /// </summary>
        /// <param name="input">要被切割的字符串</param>
        /// <returns>
        /// 如果包含序号，则返回序号和对应的文本；
        /// 如果没有序号，则返回文本和一个默认的 -1 标识。
        /// </returns>
        private static List<(string Text, int Index)> SplitProjects(string input)
        {
            return input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(line =>
                       {
                           var match = Regex.Match(line.Trim(), @"^(\d+)?[\.、]?\s*(.+?)(/\d+(\.\d+)?)$");
                           /*
                                ^：匹配字符串的开始。
                                (\d+)?：匹配一个或多个数字，并将其捕获到一个分组中。可选。
                                [\.、]?：匹配一个“点”或“顿号”。可选。
                                \s*：匹配零个或多个空白字符。
                                (.+?)：匹配任意字符，但采用非贪婪模式（+?），尽可能少地匹配字符。
                                (/\d+(\.\d+)?)：匹配斜杠（/）后跟一个或多个数字，后面可能跟小数部分。可选。
                                $：匹配字符串的结尾。
                           */
                           if (match.Success)
                           {
                               // 如果有编号，则提取编号
                               if (int.TryParse(match.Groups[1].Value, out int index))
                               {
                                   return (Text: match.Groups[2].Value.Trim(), Index: index);
                               }
                               // 如果没有编号，则返回默认编号-1
                               return (Text: match.Groups[2].Value.Trim(), Index: -1);
                           }
                           // 如果正则匹配失败，则返回整行内容和默认编号-1
                           return (Text: line.Trim(), Index: -1);
                       })
                       .ToList();
        }

        #endregion

        /// <summary>
        /// 使用ToString方法无法直接处理数字、日期等类型，需要做转换
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <returns>转换后的字符串</returns>
        private static string GetCellValue(ICell cell)
        {
            if (cell == null) return string.Empty;
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    return DateUtil.IsCellDateFormatted(cell) ?
                        cell.DateCellValue.ToString() :
                        cell.NumericCellValue.ToString();
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                default:
                    return cell.ToString();
            }
        }
    }
}

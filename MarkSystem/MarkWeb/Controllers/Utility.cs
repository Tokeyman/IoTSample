using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;

namespace MarkWeb.Controllers
{
    public class Utility
    {
        public static IEnumerable<SelectListItem> GetSelected(IEnumerable<SelectListItem> listItem, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return listItem;
            var res = listItem.Select(s => new SelectListItem()
            {
                Selected = s.Value == value,
                Text = s.Text,
                Value = s.Value
            });
            return res;
        }

        public static IEnumerable<SelectListItem> GetSelectList<T>(List<T> ListItems, string ValueField, string TextField)
        {
            IEnumerable<SelectListItem> result = ListItems.Select(x => new SelectListItem()
            {
                Selected = false,
                Text = x.GetType().GetProperty(TextField).GetValue(x, null).ToString(),
                Value = x.GetType().GetProperty(ValueField).GetValue(x, null).ToString()
            });
            return result;
        }

        /// <summary>
        /// 字符串分行去除重复项
        /// </summary>
        /// <param name="CodeString">字符串</param>
        /// <returns>结果数组</returns>
        public static string[] Split(string CodeString)
        {
            //一行一个
            string[] Codes = CodeString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Codes.Length; i++)
            {
                Codes[i] = Codes[i].Trim();
            }
            //去除重复项目
            Codes = Codes.GroupBy(p => p).Select(p => p.Key).ToArray();
            return Codes;
        }
    }
}
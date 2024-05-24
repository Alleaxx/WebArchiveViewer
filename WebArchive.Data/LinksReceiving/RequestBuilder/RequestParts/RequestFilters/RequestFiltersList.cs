using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchive.Data.RequestParts
{
    /// <summary>
    /// Конструктор для фильтрации по типам
    /// </summary>
    public class RequestFiltersList
    {
        public string[] Source { get; private set; }
        public IList<RequestFilterItem> List { get; private set; }
        public string Selected { get; private set; }

        public RequestFiltersList(IEnumerable<string> source)
        {
            Source = source.ToArray();
        }
        public void SetCollection(IList<RequestFilterItem> list)
        {
            List = list;
        }
        public void SetSelected(string value)
        {
            Selected = value;
        }

        public IEnumerable<string> GetSourceWithoutPicked()
        {
            return Source.Except(List.Select(l => l.Content)).ToArray();
        }
        public string GetFilterString()
        {
            if (!string.IsNullOrEmpty(Selected))
            {
                return Selected;
            }
            else
            {
                return List.Any() ? string.Join(",", List.Select(l => l.Enabled ? l.Content : $"!{l.Content}")) : null;
            }
        }
        public Dictionary<string, bool> GetFilters()
        {
            var dict = new Dictionary<string, bool>();
            if (!string.IsNullOrEmpty(Selected))
            {
                dict.Add(Selected, true);
            }
            else if(List != null)
            {
                foreach (var item in List)
                {
                    string text = item.Content;
                    bool disabled = !item.Enabled;

                    if (!dict.ContainsKey(text) && !string.IsNullOrEmpty(text))
                    {
                        dict.Add(text, !disabled);
                    }
                }
            }
            return dict;
        }
    }
}

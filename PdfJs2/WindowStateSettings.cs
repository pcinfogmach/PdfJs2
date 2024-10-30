using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace PdfJs2
{
    public class WindowStateSettings
    {
        public List<string> OpenFilesState { get; set; } = new List<string>();
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WindowState WindowState { get; set; }

        public int SelectedTabIndex { get; set; } // New property for selected tab index
    }
}

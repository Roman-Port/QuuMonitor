using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618

namespace QuuEmbeddedPreview.Communication
{
    class QuuEvent
    {
        public string Image { get; set; }
        public string Sold { get; set; }
        public string EventId { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Sales { get; set; }
        public string Playlist { get; set; }
        public long Station { get; set; }
        public string PlayDateTime { get; set; }
        public long RefId { get; set; }
        public int Category { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
    }
}

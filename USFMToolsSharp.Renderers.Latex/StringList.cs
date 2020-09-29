using System;
using System.Collections.Generic;
using System.Text;

namespace USFMToolsSharp.Renderers.Latex
{
    public class StringList
    {
        public List<string> Contents { get; set; }
        public StringList()
        {
            Contents = new List<string>();
        }

        public void Append(string content)
        {
            if (content == Environment.NewLine)
            {
                return;
            }

            if (content == string.Empty)
            {
                return;
            }

            Contents.Add(content);
        }

        public void AppendLine(string content)
        {
            if (content == Environment.NewLine)
            {
                return;
            }

            if (content == string.Empty)
            {
                return;
            }
            Contents.Add(content + Environment.NewLine);
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder(Contents.Count);
            foreach(var i in Contents)
            {
                output.Append(i);
            }
            return output.ToString();
        }

        public string this[int key]
        {
            get
            {
                return this.Contents[key];
            }
        }

        public int Count
        {
            get
            {
                return this.Contents.Count;
            }
        }
    }
}

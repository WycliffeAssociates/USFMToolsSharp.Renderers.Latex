using System;
using System.Collections.Generic;
using System.Text;

namespace USFMToolsSharp.Renderers.Latex
{
    public class LatexRendererConfig
    {
        public int Columns = 1;
        public double LineSpacing = 1.0;
        public bool SeparateChapters = false;
        public bool SeparateVerses = false;
        public string Font = "";
        public bool RightToLeft = false;
    }
}

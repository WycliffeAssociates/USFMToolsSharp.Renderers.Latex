using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using USFMToolsSharp.Models.Markers;

namespace USFMToolsSharp.Renderers.Latex
{
    public class LatexRenderer
    {
        public string currentChapterLabel;
        public List<string> UnrenderableMarkers = new List<string>();
        private USFMDocument inputDocument;
        private LatexRendererConfig config;
        private bool NotFirstChapter;
        private Stack<Marker> currentHierachy;
        public LatexRenderer()
        {
            config = new LatexRendererConfig();
        }

        public LatexRenderer(LatexRendererConfig latexConfig)
        {
            config = latexConfig;
        }
        public string Render(USFMDocument input)
        {
            inputDocument = input;
            StringBuilder output = new StringBuilder();
            currentChapterLabel = "";
            NotFirstChapter = false;
            currentHierachy = new Stack<Marker>();

            output.AppendLine("\\documentclass{article}");
            output.AppendLine("\\usepackage[margin=0.5in]{geometry}");
            output.AppendLine("\\usepackage{multicol}");
            output.AppendLine("\\begin{document}");
            output.AppendLine("\\pagenumbering{gobble}");
            if(config.LineSpacing != 1.0)
            {
                output.AppendLine($"\\renewcommand{{\\baselinestretch}}{{{config.LineSpacing}}}");
            }
            if(config.Columns != 1)
            {
                output.AppendLine("\\begin{multicols}{" + config.Columns + "}");
            }

            foreach (Marker marker in input.Contents)
            {
                output.Append(RenderMarker(marker, output));
            }

            if(config.Columns != 1)
            {
                output.AppendLine("\\end{multicols}");
            }

            output.AppendLine("\\end{document}");

            return string.Join("\n", output.ToString().Split('\n').Where(e => !string.IsNullOrWhiteSpace(e)));
        }

        public string RenderMarker(Marker input, StringBuilder output)
        {
            currentHierachy.Push(input);
            switch (input)
            {
                case PMarker _:
                    if (input.Contents.Count == 0)
                    {
                        break;
                    }
                    output.AppendLine("\\paragraph*{}");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    break;
                case CMarker cMarker:
                    if (config.SeparateChapters)
                    {
                        if (NotFirstChapter)
                        {
                            output.AppendLine("\\newpage");
                        }
                    }
                    else
                    {
                        var pathToChapter = inputDocument.GetHierarchyToMarker(cMarker);
                        var parent = pathToChapter[pathToChapter.Count - 2];
                        var olderSibling = parent.Contents[parent.Contents.IndexOf(cMarker) - 1];

                        if (olderSibling is CMarker)
                        {
                            var pathToLastChild = parent.Contents[parent.Contents.IndexOf(cMarker) - 1].GetTypesPathToLastMarker();
                            if (!pathToLastChild.Contains(typeof(QMarker)) && !pathToLastChild.Contains(typeof(MSMarker)) && !pathToLastChild.Contains(typeof(MMarker)))
                            {
                                output.AppendLine("\\newline");
                            }
                        }
                    }
                    string chapterMarker = cMarker.PublishedChapterMarker;
                    if (cMarker.GetChildMarkers<CLMarker>().Count > 0)
                    {
                        chapterMarker = cMarker.CustomChapterLabel;
                    }
                    else if (currentChapterLabel != null)
                    {
                        chapterMarker = $"{currentChapterLabel} {cMarker.PublishedChapterMarker}";
                    }
                    output.AppendLine($"\\Large{{{chapterMarker.Trim()}}}");

                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }

                    NotFirstChapter = true;

                    break;
                case CLMarker cLMarker:
                    currentChapterLabel = cLMarker.Label;
                    break;
                case VMarker vMarker:
                    output.AppendLine($"\\textsuperscript{{{vMarker.VerseCharacter}}}");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    break;
                case QMarker qMarker:
                    if (input.Contents.Count == 0)
                    {
                        break;
                    }
                    output.AppendLine("\\begin{center}");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.AppendLine("\\end{center}");
                    break;
                case MMarker mMarker:
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    break;
                case TextBlock textBlock:
                    output.AppendLine(textBlock.Text);
                    break;
                case BDMarker bdMarker:
                    output.AppendLine("\\begin{textbd}");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.AppendLine("\\end{textbd}");
                    break;
                case ITMarker iTMarker:
                    output.AppendLine("\\begin{textit}");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.AppendLine("\\end{textit}");
                    break;
                case BDITMarker bditMarker:
                    output.AppendLine("\\begin{textbdit}");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.AppendLine("\\end{textbdit}");
                    break;
                case EMMarker emMarker:
                    output.AppendLine("\\emph{");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker, output));
                    }
                    output.AppendLine("}");
                    break;
                case NOMarker noMarker:
                    output.AppendLine("\\textnormal{");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.AppendLine("}");
                    break;
                case NDMarker ndMarker:
                    output.AppendLine("\\textsc{");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.AppendLine("}");
                    break;
                case SUPMarker supMarker:
                    output.AppendLine("\\textsuperscript{");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.AppendLine("}");
                    break;
                case HMarker hMarker:
                    output.AppendLine("\\centerline{\\Large{" + hMarker.HeaderText + "}}");
                    break;
                case MTMarker mTMarker:
                    output.AppendLine("\\newpage");
                    output.AppendLine($"\\centerline{{{mTMarker.Title}}}");
                    currentChapterLabel = null;
                    break;
                case FMarker fMarker:
                    switch (fMarker.FootNoteCaller)
                    {
                        case "-":
                        case "+":
                            output.Append($"\\footnote{{");
                            break;
                        default:
                            output.Append($"\\footnote[{fMarker.FootNoteCaller}]{{");
                            break;
                    }

                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.Append("}");
                    break;
                case FPMarker fPMarker:
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    break;
                case FTMarker fTMarker:
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker, output));
                    }
                    break;
                case FRMarker fRMarker:
                    output.Append($"{fRMarker.VerseReference}");
                    break;
                case FKMarker fKMarker:
                    output.Append($" {fKMarker.FootNoteKeyword.ToUpper()}: ");
                    break;
                case FQAMarker _:
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    break;
                case BMarker bMarker:
                    output.Append("\\newline");
                    break;
                case SMarker sMarker:
                    output.AppendLine("\\section*{" + sMarker.Text + "}");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker, output));
                    }
                    break;
                case ADDMarker addMarker:
                    output.Append("\\textit{");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.Append("}");
                    break;
                case FQMarker fqMarker:
                    output.AppendLine("\\begin{textit}");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    output.AppendLine("\\end{textit}");
                    break;
                case QSMarker qSMarker:
                    output.Append("\\begin{textit}");
                    foreach (Marker marker in input.Contents)
                    {
                        output.Append(RenderMarker(marker, output));
                    }
                    output.AppendLine("\\end{textit}");
                    break;
                case IPMarker _:
                    output.AppendLine("\\paragraph*{}");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    break;
                case DMarker dMarker:
                    output.AppendLine("\\begin{center}");
                    output.AppendLine("\\begin{textit}");
                    output.AppendLine(dMarker.Description);
                    output.AppendLine("\\end{textit}");
                    output.AppendLine("\\end{center}");
                    break;
                case IDMarker idMarker:
                    currentChapterLabel = null;
                    break;
                case QAMarker qAMarker:
                    output.AppendLine("\\center{\\textit{" + qAMarker.Heading + "}}");
                    break;
                case PIMarker _:
                    output.AppendLine("\\paragraph*{}");
                    foreach (Marker marker in input.Contents)
                    {
                        RenderMarker(marker, output);
                    }
                    break;
                case MSMarker mSMarker:
                    output.AppendLine("\\begin{center}");
                    output.AppendLine(mSMarker.Heading);
                    output.AppendLine("\\end{center}");
                    break;
                case IOREndMarker _:
                case SUPEndMarker _:
                case NDEndMarker _:
                case NOEndMarker _:
                case BDITEndMarker _:
                case EMEndMarker _:
                case QACEndMarker _:
                case QSEndMarker _:
                case XEndMarker _:
                case WEndMarker _:
                case RQEndMarker _:
                case FVEndMarker _:
                case TLEndMarker _:
                case SCEndMarker _:
                case ADDEndMarker _:
                case BKEndMarker _:
                case FEndMarker _:
                case IDEMarker _:
                case VPMarker _:
                case VPEndMarker _:
                case USFMMarker _:
                case TOC1Marker _:
                case TOC2Marker _:
                case TOC3Marker _:
                case VAMarker _:
                case NBMarker _: // Might enable this later
                    break;
                default:
                    UnrenderableMarkers.Add(input.Identifier);
                    break;
            }
            currentHierachy.Pop();
            return "";
        }
    }
}

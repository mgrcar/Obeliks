/*==========================================================================;
 *
 *  This file is part of Obeliks. See http://obeliks.sourceforge.net/
 *
 *  File:    Default.aspx.cs
 *  Desc:    Obeliks Web page code
 *  Created: Apr-2012
 *
 *  Author:  Miha Grcar
 *
 *  License: GNU LGPL (http://www.gnu.org/licenses/lgpl.txt)
 *
 ***************************************************************************/

using System;
using System.Web.UI;
using System.Text;
using System.Web;
using System.IO;
using System.Collections.Generic;
using Latino;

/* .-----------------------------------------------------------------------
   |
   |  Class Default
   |
   '-----------------------------------------------------------------------
*/
public partial class Default : Page
{
    private ObeliksService mTaggerService 
        = new ObeliksService();

    /* .-----------------------------------------------------------------------
       |
       |  Class AttrInfo
       |
       '-----------------------------------------------------------------------
    */
    private class AttrInfo
    {
        public string mAttrName;
        public Dictionary<char, string> mAttrValInfo
            = new Dictionary<char, string>();

        public AttrInfo(string attrName)
        {
            mAttrName = attrName;
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class PosInfo
       |
       '-----------------------------------------------------------------------
    */
    private class PosInfo
    {
        public string mPosCat;
        public ArrayList<AttrInfo> mAttrInfo
            = new ArrayList<AttrInfo>();

        public PosInfo(string posCat)
        {
            mPosCat = posCat;
        }
    }

    private static Dictionary<char, PosInfo> mTagInfo
        = new Dictionary<char, PosInfo>();

    static Default()
    {
        LoadTagInfo();
    }

    static string CreateInfoText(string tag)
    {
        if (string.IsNullOrEmpty(tag) || !mTagInfo.ContainsKey(tag[0])) { return tag; }
        PosInfo posInfo = mTagInfo[tag[0]];
        string infoText = tag[0] + " = " + posInfo.mPosCat;
        for (int i = 1; i < tag.Length; i++)
        {
            AttrInfo attrInfo = posInfo.mAttrInfo[i - 1];
            if (attrInfo.mAttrValInfo.ContainsKey(tag[i]))
            {
                infoText += string.Format("\n{0} = {1} = {2}", tag[i], attrInfo.mAttrName, attrInfo.mAttrValInfo[tag[i]]);
            }
        }
        return infoText;
    }

    static void LoadTagInfo()
    {
        string[] lines = File.ReadAllLines(Global.mServer.MapPath("~\\App_Data\\tagExpl.txt"));
        foreach (string line in lines)
        {
            string[] data = line.Split('\t');
            if (!mTagInfo.ContainsKey(data[0][0])) { mTagInfo.Add(data[0][0], new PosInfo(data[1])); }
            PosInfo posInfo = mTagInfo[data[0][0]];
            if (data[3] != "")
            {
                int idx = Convert.ToInt32(data[3]) - 1;
                if (posInfo.mAttrInfo.Count - 1 < idx) { posInfo.mAttrInfo.Add(new AttrInfo(data[4])); }
                AttrInfo attrInfo = posInfo.mAttrInfo.Last;
                attrInfo.mAttrValInfo.Add(data[2][0], data[5]);                
            }
        }
    }

    protected void Submit_Click(object sender, EventArgs e)
    {    
        try
        {
            if (TextBox.Text.Trim() == "")
            {
                ErrorMessage.Text = "Niste vnesli besedila.";
                PageView.ActiveViewIndex = 1;
                return;
            }
            StringBuilder response = new StringBuilder("<h2>Označeno besedilo</h2>");
            if (OutputType.SelectedIndex == 0)
            {
                string[] triples = mTaggerService.Tag(TextBox.Text.Trim(), /*xmlOutput=*/false).Replace("\r", "").Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);                
                response.AppendLine("<div class='p'><table border='1' class='container'>");
                int i = 0;
                int k = 1;
                while (i < triples.Length)
                {
                    response.AppendLine(string.Format("<tr><th align='left' valign='middle'>{0}</th><th align='left' valign='middle'>" +
                        "<table border='0'><tr><th align='left'>beseda</th></tr><tr><th align='left'>lema</th></tr><tr><th align='left'>oznaka</th></tr></table>" +
                        "</th><td nowrap='nowrap' align='left' valign='baseline'><table border='0'>", k++));
                    string wordsHtml = "<tr>";
                    string lemmasHtml = "<tr>";
                    string tagsHtml = "<tr>";
                    const int maxCharCount = 80; // *** hardcoded maxCharCount
                    int charCount = 0;
                    int j = i;
                    for (; j < triples.Length; j++)
                    {
                        string[] cols = triples[j].Split('\t');
                        int tokenCharCount = Math.Max(Math.Max(cols[0].Length, cols[1].Length), cols[2].Length);
                        if (j == i || charCount + tokenCharCount <= maxCharCount)
                        {
                            charCount += tokenCharCount;
                            wordsHtml += string.Format("<td nowrap='nowrap'><span class='word'>{0}</td>", cols[0]);
                            lemmasHtml += string.Format("<td nowrap='nowrap'><span class='lemma'>{0}</td>", cols[1]);
                            tagsHtml += string.Format("<td nowrap='nowrap'><span class='tag' title='{1}'>{0}</td>", cols[2].Replace("<eos>", ""), CreateInfoText(cols[2].Replace("<eos>", "")));
                        }
                        else
                        {
                            break;
                        }
                    }
                    response.Append(wordsHtml);
                    response.AppendLine("</tr>");
                    response.Append(lemmasHtml);
                    response.AppendLine("</tr>");
                    response.Append(tagsHtml);
                    response.AppendLine("</tr>");
                    response.AppendLine("</table></tr>");
                    i = j;
                }
                response.AppendLine("</table></div>");
            }
            else
            {
                string xml = mTaggerService.Tag(TextBox.Text.Trim(), /*xmlOutput=*/true).Replace("\t", "  ");
                response.AppendLine(string.Format("<pre>{0}</pre>", HttpUtility.HtmlEncode(xml)));
            }
            TaggedText.Text = response.ToString();
            PageView.ActiveViewIndex = 0;
        }       
        catch (Exception exception)
        {
            ErrorMessage.Text = exception.Message;// +" " + exception.StackTrace.Replace("\r\n", "<br>");
            PageView.ActiveViewIndex = 1;
        }
    }
}
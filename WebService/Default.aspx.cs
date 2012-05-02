using System;
using System.Web.UI;
using System.Text;

public partial class _Default : Page
{
    private TaggerService mTaggerService 
        = new TaggerService();

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
            string[] triples = mTaggerService.Tag(TextBox.Text.Trim(), /*xmlOutput=*/false).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder response = new StringBuilder("<h2>Označeno besedilo</h2>");
            response.AppendLine("<TABLE border='1'>");
            int i = 0;
            int k = 1;
            while (i < triples.Length)
            {
                response.AppendLine(string.Format("<TR><TH ALIGN='left' VALIGN='middle'>{0}</TH><TH ALIGN='left' VALIGN='middle'>" +
                    "<TABLE border='0'><TR><TH align='left'>beseda</TH></TR><TR><TH align='left'>lema</TH></TR><TR><TH align='left'>oznaka</TH></TR></TABLE>" +
                    "</TH><TD NOWRAP='nowrap' ALIGN='left' VALIGN='baseline'><TABLE border='0'>", k++));
                string wordsHtml = "<TR>";
                string lemmasHtml = "<TR>";
                string tagsHtml = "<TR>";
                const int maxCharCount = 120; // *** hardcoded maxCharCount
                int charCount = 0;
                int j = i;
                for (; j < triples.Length; j++)
                {
                    string[] cols = triples[j].Split('\t');
                    int tokenCharCount = Math.Max(Math.Max(cols[0].Length, cols[1].Length), cols[2].Length);
                    if (j == i || charCount + tokenCharCount <= maxCharCount)
                    {
                        charCount += tokenCharCount;                            
                        wordsHtml += string.Format("<TD NOWRAP='nowrap'><FONT color='black'>{0}</TD>", cols[0]);
                        lemmasHtml += string.Format("<TD NOWRAP='nowrap'><FONT color='blue'>{0}</TD>", cols[1]);
                        tagsHtml += string.Format("<TD NOWRAP='nowrap'><FONT color='red'>{0}</TD>", cols[2]);
                    }
                    else
                    {
                        break;
                    }
                }
                response.Append(wordsHtml);
                response.AppendLine("</TR>");
                response.Append(lemmasHtml);
                response.AppendLine("</TR>");
                response.Append(tagsHtml);
                response.AppendLine("</TR>");
                response.AppendLine("</TABLE></TR>");
                i = j;
            }
            response.AppendLine("</TABLE>");
            TaggedText.Text = response.ToString();
            PageView.ActiveViewIndex = 0;
        }       
        catch (Exception exception)
        {
            ErrorMessage.Text = string.Format("Prišlo je do napake.<br />({0})", exception.Message);
            PageView.ActiveViewIndex = 1;
        }
    }
}
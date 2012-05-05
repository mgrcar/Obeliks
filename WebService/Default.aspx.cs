using System;
using System.Web.UI;
using System.Text;
using System.Web;

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
            StringBuilder response = new StringBuilder("<h2>Označeno besedilo</h2>");
            if (OutputType.SelectedIndex == 0)
            {
                string[] triples = mTaggerService.Tag(TextBox.Text.Trim(), /*xmlOutput=*/false).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);                
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
                            wordsHtml += string.Format("<td nowrap='nowrap'><font color='black'>{0}</td>", cols[0]);
                            lemmasHtml += string.Format("<td nowrap='nowrap'><font color='blue'>{0}</td>", cols[1]);
                            tagsHtml += string.Format("<td nowrap='nowrap'><font color='red'>{0}</td>", cols[2].Replace("<eos>", ""));
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
            ErrorMessage.Text = exception.Message;
            PageView.ActiveViewIndex = 1;
        }
    }
}
/*==========================================================================;
 *
 *  This file is part of Obeliks. See http://obeliks.sourceforge.net/
 *
 *  File:    ObeliksService.cs
 *  Desc:    Obeliks Web service
 *  Created: Apr-2012
 *
 *  Author:  Miha Grcar
 *
 *  License: GNU LGPL (http://www.gnu.org/licenses/lgpl.txt)
 *
 ***************************************************************************/

using System.Web.Services;
using System.Threading;
using PosTagger;

/* .-----------------------------------------------------------------------
   |
   |  Class ObeliksService
   |
   '-----------------------------------------------------------------------
*/
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class ObeliksService : WebService
{
    [WebMethod]
    public bool Ready()
    {
        return Global.mReady;
    }

    [WebMethod]
    public string Tag(string text, bool xmlOutput)
    {
        while (!Global.mReady) { Thread.Sleep(100); }
        Corpus corpus = new Corpus();
        corpus.LoadFromTextSsjTokenizer(text);
        int lemmaCorrect, lemmaCorrectLowercase, lemmaWords;
        Global.mPosTagger.Tag(corpus, out lemmaCorrect, out lemmaCorrectLowercase, out lemmaWords, /*xmlMode=*/false);
        return xmlOutput ? corpus.ToString("XML-MI") : corpus.ToString("TBL");
    }
}

/*==========================================================================;
 *
 *  This file is part of Obeliks. See http://obeliks.sourceforge.net/
 *
 *  File:    Global.cs
 *  Desc:    Obeliks Web application environment
 *  Created: Apr-2012
 *
 *  Author:  Miha Grcar
 *
 *  License: GNU LGPL (http://www.gnu.org/licenses/lgpl.txt)
 *
 ***************************************************************************/

using System;
using System.Web;
using System.Threading;
using PosTagger;

/* .-----------------------------------------------------------------------
   |
   |  Class Global
   |
   '-----------------------------------------------------------------------
*/
public class Global : HttpApplication
{
    public static PartOfSpeechTagger mPosTagger
        = new PartOfSpeechTagger();
    public static HttpServerUtility mServer; 
    public static bool mReady
        = false;

    protected void Application_Start(object sender, EventArgs args)
    {
        string taggerModelFile = Server.MapPath("~\\Models\\TaggerFeb2012.bin");
        string lemmatizerModelFile = Server.MapPath("~\\Models\\LemmatizerFeb2012.bin");
        new Thread(new ThreadStart(delegate() {
            mPosTagger.LoadModels(taggerModelFile, lemmatizerModelFile);
            mReady = true;
        })).Start();
        mServer = Server;
    }

    protected void Session_Start(object sender, EventArgs args)
    {
    }

    protected void Application_BeginRequest(object sender, EventArgs args)
    {
    }

    protected void Application_EndRequest(object sender, EventArgs args)
    {
    }

    protected void Application_AuthenticateRequest(object sender, EventArgs args)
    {
    }

    protected void Application_Error(object sender, EventArgs args)
    {
    }

    protected void Session_End(object sender, EventArgs args)
    {
    }

    protected void Application_End(object sender, EventArgs args)
    {
    }
}
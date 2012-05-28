<%--========================================================================;
 *
 *  This file is part of Obeliks. See http://obeliks.sourceforge.net/
 *
 *  File:    Default.aspx
 *  Desc:    Obeliks Web page markup
 *  Created: Apr-2012
 *
 *  Author:  Miha Grcar
 *
 *  License: GNU LGPL (http://www.gnu.org/licenses/lgpl.txt)
 *
 ************************************************************************--%>
 
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">

<head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <title>Obeliks: Oblikoslovni označevalnik za slovenski jezik</title>
	<style type="text/css">
        button { padding: 0; border: 0; font-size: 14px; background: none; cursor: pointer; }
        button span { display: block; background: url(Img/bg-buttons.png) 0 0; height: 41px; line-height: 41px; }
        button span.r { margin-left: 20px; padding-right: 20px; background-position: right 0; }
        button:hover span.l { background-position: left -42px; }
        button:active span.l { background-position: left -84px; }
        button:hover span.r { background-position: right -42px; }
        button:active span.r { background-position: right -84px; }

		textarea { -webkit-appearance: none; width: 100%; height: 200px; resize: none; font-size: 100%; }  
		.copyright { font-size: 80%; padding-top: 15px; }
		a:link, a:visited, a:active, a:hover { color: #3C64B6; outline: 0; }
    	img { border: 0; }
    	:focus { outline: none; } 
    	::-moz-focus-inner { border: 0; }
    	table { text-align: left; }
    	
    	table.container { border-width: 1px; border-color: silver; border-collapse: collapse; border-style: solid; }        
        table.container td, table.container th { padding: 1px; border-color: silver; }
    	    	
    	h1, h2, p, .p, pre { padding-top: 10px; padding-bottom: 10px; }
    	
    	pre { text-align: left; font-family: courier new, courier; }
    	
    	span.lemma { color: blue; }
    	span.tag { color: red; }

		/*  
		Sticky Footer Solution
		by Steve Hatcher 
		http://stever.ca
		http://www.cssstickyfooter.com
		*/

		* {margin: 0; padding: 0; font-family: helvetica;} 

		/* must declare 0 margins on everything, also for main layout components use padding, not 
		vertical margins (top and bottom) to add spacing, else those margins get added to total height 
		and your footer gets pushed down a bit more, creating vertical scroll bars in the browser */

		html, body {height: 100%; min-width: 800px;}

		#wrap {min-height: 100%;}

		#main {overflow: hidden;
			padding-bottom: 150px;}
       
		#footer {position: relative;
			margin-top: -100px; /* negative value of footer height */
			height: 100px;
			clear: both;} 

		/* Opera Fix */
		body:before { /* thanks to Maleika (Kohoutec) */
			content: "";
			height: 100%;
			float: left;
			width: 0;
			margin-top: -32767px;} /* thank you Erik J - negate effect of float */
	</style>
</head>

<body>
    <div id="wrap">
    <div id="main">
        <center>
			<div style="width: 800px;">
			    <h1>Obeliks:<br />Oblikoslovni označevalnik za slovenski jezik</h1>
				<asp:MultiView ID="PageView" runat="server" ActiveViewIndex="0">        
					<asp:View ID="DefaultView" runat="server">												
						<form id="form" runat="server">
						    <input type="hidden" name="SubmitBtn" value="" />
						    <div style="position: absolute; visibility: hidden;"><asp:Button runat="server" ID="SubmitBtn" OnClick="Submit_Click" Text="" /></div>
						    <h2>Vnesite besedilo</h2>
							<p><asp:TextBox ID="TextBox" runat="server" TextMode="MultiLine">To je besedilo.</asp:TextBox></p>
							<div class="p">Oblika izpisa označenega besedila:
							<asp:RadioButtonList ID="OutputType" runat="server">
                                <asp:ListItem Selected="True" Value="Table"> Tabela</asp:ListItem>
                                <asp:ListItem Value="XML"> XML-TEI</asp:ListItem>
                            </asp:RadioButtonList></div>
							<p><button onclick="javascript: document.forms[0].submit();"><span class="l"><span class="r">Označi besedilo</span></span></button></p>
							<br />				
							<asp:Label ID="TaggedText" runat="server" Text=""></asp:Label>
						</form>            
					</asp:View>
					<asp:View ID="ErrorView" runat="server">
						<h2>Prišlo je do napake</h2>
						<p><asp:Label ID="ErrorMessage" runat="server" Text="Error message goes here."></asp:Label></p>
						<p><button onclick="javascript: document.location = 'Default.aspx';"><span class="l"><span class="r">Nazaj na prvo stran</span></span></button></p>
					</asp:View>
				</asp:MultiView>
			</div>
        </center>
    </div>
    </div>
    <div id="footer">
        <center>
			<div style="width: 800px;">
				<a href="http://mss.gov.si/"><img style="vertical-align: middle;" src="Img/logo1.gif" alt="Ministrstvo za šolstvo in šport" /></a> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; <a href="http://euskladi.si/"><img style="vertical-align: middle;" src="Img/logo2.gif" alt="Evropska unija" /></a></abbr><br />
				<div class="copyright">Operacijo delno financira Evropska unija iz <a href="http://euskladi.si/">Evropskega socialnega sklada</a> ter <a href="http://mss.gov.si/">Ministrstvo za izobraževanje, znanost, kulturo in šport</a>.</div>
			</div>
        </center>
    </div>
</body>

</html>
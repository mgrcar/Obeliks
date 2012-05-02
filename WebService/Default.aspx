<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Obeliks [Oblikoslovni označevalnik za Slovenščino]</title>
</head>
<body>
    <asp:MultiView ID="PageView" runat="server" ActiveViewIndex="0">        
        <asp:View ID="DefaultView" runat="server">
            <h1>Obeliks: Oblikoslovni označevalnik za slovenski jezik</h1>
            <p>Vnesite besedilo:</p>
            <form id="form1" runat="server">
                <asp:TextBox ID="TextBox" runat="server" TextMode="MultiLine"></asp:TextBox>
                <br />
                <br />
                <asp:Button ID="Submit" runat="server" Text="Označi besedilo" onclick="Submit_Click" />
                <br />
                <br />
                <asp:Label ID="TaggedText" runat="server" Text=""></asp:Label>
            </form>            
        </asp:View>
        <asp:View ID="ErrorView" runat="server">
            <h1>Oblikoslovni označevalnik za slovenski jezik (Obeliks)</h1>
            <h2>Prišlo je do napake</h2>
            <asp:Label ID="ErrorMessage" runat="server" Text="Error message goes here."></asp:Label>
        </asp:View>
    </asp:MultiView>    
</body>
</html>

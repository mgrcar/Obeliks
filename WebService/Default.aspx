<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Obeliks [Oblikoslovni označevalnik za Slovenščino]</title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:MultiView ID="PageView" runat="server" ActiveViewIndex="0">        
        <asp:View ID="DefaultView" runat="server">
            <asp:TextBox ID="TextBox" runat="server" Height="232px" TextMode="MultiLine" 
                Width="392px"></asp:TextBox>
            <br />
            <br />
            <asp:Button ID="Submit" runat="server" Text="Označi besedilo" 
                onclick="Submit_Click" />
            <br />
            <br />
            <asp:Label ID="TaggedText" runat="server" Text="Tagged text goes here."></asp:Label>
            <br />
        </asp:View>
        <asp:View ID="ErrorView" runat="server">
            <asp:Label ID="ErrorMessage" runat="server" Text="Error message goes here."></asp:Label>
        </asp:View>
    </asp:MultiView>
    </form>
</body>
</html>

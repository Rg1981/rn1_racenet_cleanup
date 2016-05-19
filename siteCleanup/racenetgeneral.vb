Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Net
Imports System.Net.Mail
Public Class racenetgeneral
    Public Shared Function ToTitleCase(text As String) As String
        Return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower)
    End Function
    Public Shared Function splitstring(ss As String, splitChar As String) As String()
        Return ss.Split(New String() {splitChar}, StringSplitOptions.None)
    End Function
	Public Shared Function smtpmailserver() As Mail.SmtpClient
		Dim smptClient As SmtpClient = New Mail.SmtpClient("smtp.mandrillapp.com", 587)
		smptClient.Credentials = New NetworkCredential("webdev@racenet.com.au", "HTQs5dtZZFIppbYeGN7-dQ")
		Return smptClient
    End Function
End Class

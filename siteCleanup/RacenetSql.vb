Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.SqlClient

Public Class RacenetSql
	Public Shared Function getConnection() As SqlConnection
        'Return New SqlConnection("Data Source=BNE3-0718DQ.server-web.com,4656; Initial Catalog=Racenet; User ID=Racenet; Password=36JNh57HUN; MultipleActiveResultSets=true;")
        Return New SqlConnection("Data Source=10.0.54.131,1433; Initial Catalog=Racenet; User ID=Racenet; Password=36JNh57HUN; MultipleActiveResultSets=true;")
        'Return New SqlConnection("Data Source=202.129.143.39,1433; Initial Catalog=Racenet; User ID=Racenet; Password=36JNh57HUN; MultipleActiveResultSets=true;")
    End Function

	Public Shared Function buildCommand(ByRef conn As SqlConnection) As SqlCommand
		Dim command As SqlCommand = New SqlCommand()
		command.Connection = conn
		command.CommandType = CommandType.StoredProcedure

		Return command
	End Function

	Public Shared Function buildCommand(queryName As String, ByRef conn As SqlConnection) As SqlCommand
		Dim command As SqlCommand = New SqlCommand()
		command.Connection = conn
		command.CommandType = CommandType.StoredProcedure
		command.CommandText = queryName

		Return command
	End Function

	Public Shared Sub closeConnection(ByRef conn As SqlConnection)
		conn.Close()
		conn.Dispose()
		conn = Nothing
	End Sub

	Public Shared Sub disposeCommand(ByRef cmd As SqlCommand)
		cmd.Dispose()
		cmd = Nothing
	End Sub

	Public Shared Sub closeReader(ByRef reader As SqlDataReader)
		reader.Close()
		reader.Dispose()
		reader = Nothing
	End Sub
	Public Shared Function dbIsNull(ByRef ReaderName As SqlDataReader, FieldName As String) As String
		If ReaderName.IsDBNull(ReaderName.GetOrdinal(FieldName)) Then
			Return ""
		Else
			Return ReaderName(FieldName)
		End If
    End Function
    Public Shared Function dbIsNullNum(ByRef ReaderName As SqlDataReader, FieldName As String) As String
        If ReaderName.IsDBNull(ReaderName.GetOrdinal(FieldName)) Then
            Return "0"
        Else
            Return ReaderName(FieldName)
        End If
    End Function
End Class

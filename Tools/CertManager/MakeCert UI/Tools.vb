Imports System.IO
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates

Public Class Tools

    Private Const SEARCHPATH As String = "C:\Program Files (x86)\Windows Kits\8.1\bin\x64;%ProgramW6432%\Microsoft Visual Studio 8\Common7\Tools\Bin;%ProgramW6432%\Microsoft Visual Studio 10\Common7\Tools\Bin;%ProgramW6432%\Microsoft SDKs\Windows\v7.1\Bin\x64;%ProgramW6432%\Microsoft SDKs\Windows\v7.1\Bin;%ProgramFiles%\Microsoft Visual Studio 8\Common7\Tools\Bin;%ProgramFiles%\Microsoft Visual Studio 10\Common7\Tools\Bin"

    Private Shared _makecerttools As String
    Private Shared _pvk2PfxTool As String

    Public Shared ReadOnly Property MakeCertTool As String
        Get
            If String.IsNullOrEmpty(_makecerttools) Then
                For Each toolPath As String In SEARCHPATH.Split(";")
                    toolPath = Environment.ExpandEnvironmentVariables(toolPath)

                    If File.Exists(toolPath + "\makecert.exe") Then
                        _makecerttools = toolPath + "\makecert.exe"
                        Exit For
                    End If
                Next
            End If

            Return """" & _makecerttools & """"
        End Get
    End Property

    Public Shared ReadOnly Property Pvk2PfxTool As String
        Get
            If String.IsNullOrEmpty(_pvk2PfxTool) Then
                For Each toolPath As String In SEARCHPATH.Split(";")
                    toolPath = Environment.ExpandEnvironmentVariables(toolPath)

                    If File.Exists(toolPath + "\pvk2pfx.exe") Then
                        _pvk2PfxTool = toolPath + "\pvk2pfx.exe"
                        Exit For
                    End If
                Next
            End If

            Return """" & _pvk2PfxTool & """"
        End Get
    End Property

    Public Shared Function RootCAs() As List(Of String)
        Dim store As X509Store = New X509Store(StoreName.Root, StoreLocation.LocalMachine)
        store.Open(OpenFlags.ReadOnly Or OpenFlags.OpenExistingOnly)
        Dim returnList As List(Of String) = New List(Of String)

        'Return store.Certificates
        For Each cert As X509Certificate2 In store.Certificates
            If Not String.IsNullOrEmpty(cert.Subject) Then
                returnList.Add(cert.Subject)
            End If
        Next

        Return returnList
    End Function

    Public Shared Function GetTemplates() As List(Of String)
        Dim returnList As List(Of String) = New List(Of String)

        returnList.Add("None")
        returnList.Add("1.3.6.1.5.5.7.3.1")

        Return returnList
    End Function

End Class

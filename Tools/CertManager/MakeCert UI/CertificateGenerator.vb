Imports System.IO
Imports System.Diagnostics

Public Class CertificateGenerator

    Public Shared Function Generate(ByVal props As CertificateProperties) As OutputResult

        ' TODO: validate the options

        Dim output As OutputResult = New OutputResult

        ' generate cer and private key file
        RunExternalTool(Tools.MakeCertTool, props.GetCommandLineArguments, output)

        If props.GenerateType = CertificateProperties.GenerateTypeEnum.PfxFile Then
            RunExternalTool(Tools.Pvk2PfxTool, " -pvk " & _
                            props.CertificatePvkFile & " -spc " & props.CertificateFile & _
                            " -pfx " & props.CertificatePfxFile & " -po " & props.PfxPassword, output)
        End If

        Return output
    End Function

    Private Shared Sub RunExternalTool(ByVal filename As String, ByVal arguments As String, ByVal output As OutputResult)
        ' output
        Using proc As Process = New Process
            proc.StartInfo.UseShellExecute = False
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal
            proc.StartInfo.FileName = filename
            proc.StartInfo.Arguments = arguments
            proc.StartInfo.RedirectStandardInput = True
            proc.StartInfo.RedirectStandardOutput = True
            proc.StartInfo.RedirectStandardError = True

            AddHandler proc.OutputDataReceived, Sub(o As Object, e As DataReceivedEventArgs)
                                                    If e.Data <> "" Then output.Output.AppendLine(e.Data)
                                                End Sub
            AddHandler proc.ErrorDataReceived, Sub(o As Object, e As DataReceivedEventArgs)
                                                   If e.Data <> "" Then output.Error.AppendLine(e.Data)
                                               End Sub

            proc.Start()
            proc.BeginOutputReadLine()
            proc.BeginErrorReadLine()
            proc.WaitForExit()

            output.Exitcode = proc.ExitCode
        End Using
    End Sub
End Class

Public Class OutputResult
    Friend Property Exitcode As Integer = 0
    Friend Property RunException As Exception = Nothing
    Friend Property Output As System.Text.StringBuilder = New System.Text.StringBuilder
    Friend Property [Error] As System.Text.StringBuilder = New System.Text.StringBuilder
End Class

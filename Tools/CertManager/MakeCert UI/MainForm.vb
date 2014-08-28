Public Class MainForm

    Protected Friend Certificate As CertificateProperties

    Private Sub MainForm_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        If (Tools.MakeCertTool = """""") OrElse (Tools.Pvk2PfxTool = """""") Then
            MsgBox("You need to have the makecert.exe and pvk2pfx.exe tool installed.")
            Application.Exit()
        End If

        SetupNewCertificate()

    End Sub

    Private Sub SetupNewCertificate()
        Certificate = CertificateProperties.Open()
        CertificatePropertyGrid.SelectedObject = Certificate
    End Sub

    Private Sub CategorizedToolStripButton_Click(sender As System.Object, e As System.EventArgs) Handles CategorizedToolStripButton.Click
        CertificatePropertyGrid.PropertySort = PropertySort.CategorizedAlphabetical
        CategorizedToolStripButton.Checked = True
        AlphabeticalToolStripButton.Checked = False

    End Sub

    Private Sub AlphabeticalToolStripButton_Click(sender As System.Object, e As System.EventArgs) Handles AlphabeticalToolStripButton.Click
        CertificatePropertyGrid.PropertySort = PropertySort.Alphabetical
        AlphabeticalToolStripButton.Checked = True
        CategorizedToolStripButton.Checked = False
    End Sub

    Private Sub SaveToolStripButton_Click(sender As System.Object, e As System.EventArgs) Handles SaveToolStripButton.Click
        Dim saveDialog As SaveFileDialog = New SaveFileDialog

        With saveDialog
            .FileName = Certificate.FileName
            .Title = "Save certificate"
            .DefaultExt = "cer"
            .OverwritePrompt = True
            .CheckPathExists = True
            .AddExtension = True
            .Filter = "Certificate (*.cer)|*.cer"
            If Certificate.GeneratePrivateKey Then
                ' only add pfx if the private key is generated
                .Filter += "|Personal Information Exchange file (*.pfx)|*.pfx"
            End If
            .SupportMultiDottedExtensions = True

            If .ShowDialog() = Windows.Forms.DialogResult.OK Then
                Certificate.FileName = .FileName

                Dim r As OutputResult = CertificateGenerator.Generate(Certificate)
                If (r.Exitcode <> 0) OrElse (r.Output.ToString.Trim() <> "Succeeded") Then
                    MsgBox(r.Output.ToString & " - " & r.Error.ToString)
                Else
                    StandardToolStripStatusLabel.Text = "Certificate generated..."
                End If
            End If
        End With
        Certificate.Save()
    End Sub

    Private Sub NewCertificateToolStripButton_Click(sender As System.Object, e As System.EventArgs) Handles NewCertificateToolStripButton.Click

        SetupNewCertificate()

    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
    End Sub
End Class

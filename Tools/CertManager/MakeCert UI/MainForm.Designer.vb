<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.StandardMenuStrip = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StandardStatusStrip = New System.Windows.Forms.StatusStrip()
        Me.StandardToolStripStatusLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.NewCertificateToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.SaveToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.CategorizedToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.AlphabeticalToolStripButton = New System.Windows.Forms.ToolStripButton()
        Me.CertificatePropertyGrid = New System.Windows.Forms.PropertyGrid()
        Me.StandardMenuStrip.SuspendLayout()
        Me.StandardStatusStrip.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'StandardMenuStrip
        '
        Me.StandardMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem})
        Me.StandardMenuStrip.Location = New System.Drawing.Point(0, 0)
        Me.StandardMenuStrip.Name = "StandardMenuStrip"
        Me.StandardMenuStrip.Size = New System.Drawing.Size(416, 24)
        Me.StandardMenuStrip.TabIndex = 0
        Me.StandardMenuStrip.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExitToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "&File"
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.ExitToolStripMenuItem.Text = "&Exit"
        '
        'StandardStatusStrip
        '
        Me.StandardStatusStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StandardToolStripStatusLabel})
        Me.StandardStatusStrip.Location = New System.Drawing.Point(0, 449)
        Me.StandardStatusStrip.Name = "StandardStatusStrip"
        Me.StandardStatusStrip.Size = New System.Drawing.Size(416, 22)
        Me.StandardStatusStrip.TabIndex = 2
        Me.StandardStatusStrip.Text = "StatusStrip1"
        '
        'StandardToolStripStatusLabel
        '
        Me.StandardToolStripStatusLabel.Name = "StandardToolStripStatusLabel"
        Me.StandardToolStripStatusLabel.Size = New System.Drawing.Size(39, 17)
        Me.StandardToolStripStatusLabel.Text = "Ready"
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NewCertificateToolStripButton, Me.SaveToolStripButton, Me.ToolStripSeparator1, Me.CategorizedToolStripButton, Me.AlphabeticalToolStripButton})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 24)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(416, 25)
        Me.ToolStrip1.TabIndex = 3
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'NewCertificateToolStripButton
        '
        Me.NewCertificateToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.NewCertificateToolStripButton.Image = CType(resources.GetObject("NewCertificateToolStripButton.Image"), System.Drawing.Image)
        Me.NewCertificateToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.NewCertificateToolStripButton.Name = "NewCertificateToolStripButton"
        Me.NewCertificateToolStripButton.Size = New System.Drawing.Size(23, 22)
        Me.NewCertificateToolStripButton.Text = "New certificate"
        '
        'SaveToolStripButton
        '
        Me.SaveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.SaveToolStripButton.Image = CType(resources.GetObject("SaveToolStripButton.Image"), System.Drawing.Image)
        Me.SaveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.SaveToolStripButton.Name = "SaveToolStripButton"
        Me.SaveToolStripButton.Size = New System.Drawing.Size(23, 22)
        Me.SaveToolStripButton.Text = "Save Certificate"
        Me.SaveToolStripButton.ToolTipText = "Generate and Save Certificate"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'CategorizedToolStripButton
        '
        Me.CategorizedToolStripButton.Checked = True
        Me.CategorizedToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CategorizedToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.CategorizedToolStripButton.Image = CType(resources.GetObject("CategorizedToolStripButton.Image"), System.Drawing.Image)
        Me.CategorizedToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.CategorizedToolStripButton.Name = "CategorizedToolStripButton"
        Me.CategorizedToolStripButton.Size = New System.Drawing.Size(23, 22)
        Me.CategorizedToolStripButton.Text = "Categorized"
        '
        'AlphabeticalToolStripButton
        '
        Me.AlphabeticalToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.AlphabeticalToolStripButton.Image = CType(resources.GetObject("AlphabeticalToolStripButton.Image"), System.Drawing.Image)
        Me.AlphabeticalToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.AlphabeticalToolStripButton.Name = "AlphabeticalToolStripButton"
        Me.AlphabeticalToolStripButton.Size = New System.Drawing.Size(23, 22)
        Me.AlphabeticalToolStripButton.Text = "Alphabetical"
        '
        'CertificatePropertyGrid
        '
        Me.CertificatePropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.CertificatePropertyGrid.Location = New System.Drawing.Point(0, 49)
        Me.CertificatePropertyGrid.Name = "CertificatePropertyGrid"
        Me.CertificatePropertyGrid.Size = New System.Drawing.Size(416, 400)
        Me.CertificatePropertyGrid.TabIndex = 4
        Me.CertificatePropertyGrid.ToolbarVisible = False
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(416, 471)
        Me.Controls.Add(Me.CertificatePropertyGrid)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.StandardStatusStrip)
        Me.Controls.Add(Me.StandardMenuStrip)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.StandardMenuStrip
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(600, 680)
        Me.MinimumSize = New System.Drawing.Size(380, 410)
        Me.Name = "MainForm"
        Me.Text = "MakeCert UI"
        Me.StandardMenuStrip.ResumeLayout(False)
        Me.StandardMenuStrip.PerformLayout()
        Me.StandardStatusStrip.ResumeLayout(False)
        Me.StandardStatusStrip.PerformLayout()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents StandardMenuStrip As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StandardStatusStrip As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents CategorizedToolStripButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents AlphabeticalToolStripButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents CertificatePropertyGrid As System.Windows.Forms.PropertyGrid
    Friend WithEvents NewCertificateToolStripButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents SaveToolStripButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents StandardToolStripStatusLabel As System.Windows.Forms.ToolStripStatusLabel

End Class

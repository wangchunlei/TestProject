Imports System.Text
Imports System.ComponentModel
Imports System.Security.Cryptography.X509Certificates

<Serializable()>
Public Class CertificateProperties

#Region "Member variables"
    <NonSerialized()>
    Private _x500DistinguishedNamed As String = "CN="
    <NonSerialized()>
    Private _filename As String = String.Empty
    <NonSerialized()>
    Private _propertyChanges As Boolean = False
#End Region

#Region "Constructor2"
    Private Sub New()

    End Sub
#End Region

#Region "Public PropertieS"
    <System.Xml.Serialization.XmlIgnore()>
    <Category("Certificate"), DefaultValue("CN="), Description("Distinguished name of the certificate.")>
    Public Property X500DistinguishedNamed As String
        Get
            If Not _x500DistinguishedNamed.ToLower().StartsWith("cn=") Then
                Return "CN=" & _x500DistinguishedNamed
            Else
                Return _x500DistinguishedNamed
            End If
        End Get
        Set(value As String)
            _x500DistinguishedNamed = value
        End Set
    End Property

    <Category("Certificate"), Description(""), DefaultValue(GetType(KeySizeEnum), "Bit1024")>
    Public Property KeySize As KeySizeEnum = KeySizeEnum.Bit1024

    <Category("Personal Information Exchange"), DefaultValue("1234")>
    Public Property PfxPassword As String = "1234"

    <Category("Certificate"), Description("")>
    Public ReadOnly Property ValidFrom As DateTime
        Get
            Return DateTime.Now
        End Get
    End Property

    <Category("Certificate"), Description("")>
    Public ReadOnly Property ValidTo As DateTime
        Get
            Return DateTime.Now.AddYears(2)
        End Get
    End Property

    <Category("Certificate"), DefaultValue(GetType(AlgorithmEnum), "sha1")>
    Public Property Algorithm As AlgorithmEnum = AlgorithmEnum.sha1

    <Category("Certificate"), DefaultValue(True)>
    Public Property GeneratePrivateKey As Boolean = True

    <Category("Key Exchange"), Description(""), DefaultValue(True)>
    Public Property ExportablePrivateKey As Boolean = True

    <Category("Key Exchange"), Description(""), DefaultValue(True)>
    Public Property Exchangable As Boolean = True

    <Category("Certificate Authority"), DefaultValue(True)>
    Public Property SelfSign As Boolean = True

    <Category("Certificate Authority"), Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property CaCertificateFile As String

    <Category("Certificate Authority"), Editor(GetType(System.Windows.Forms.Design.FileNameEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property CaCertificatePvkFile As String

    <Category("Template"), DefaultValue("None"), TypeConverter(GetType(CertificateTypeDropdown))>
    Public Property CertificateType As String = "None"

    <Browsable(False)>
    <System.Xml.Serialization.XmlIgnore()>
    Public Property FileName As String
        Get
            If String.IsNullOrEmpty(_filename) Then
                _filename = X500DistinguishedNamed.Replace("CN=", "") & ".cer"
            End If
            Return _filename
        End Get
        Set(value As String)
            _filename = value
        End Set
    End Property

    <Browsable(False)>
    <System.Xml.Serialization.XmlIgnore()>
    Public ReadOnly Property CertificateFile As String
        Get
            Return GetFilePart(Me.FileName) & ".cer"
        End Get
    End Property

    <Browsable(False)>
    <System.Xml.Serialization.XmlIgnore()>
    Public ReadOnly Property CertificatePvkFile As String
        Get
            Return GetFilePart(Me.FileName) & ".pvk"
        End Get
    End Property

    <Browsable(False)>
    <System.Xml.Serialization.XmlIgnore()>
    Public ReadOnly Property CertificatePfxFile As String
        Get
            Return GetFilePart(Me.FileName) & ".pfx"
        End Get
    End Property

    <Browsable(False)>
    <System.Xml.Serialization.XmlIgnore()>
    Public ReadOnly Property PropertyChanges As Boolean
        Get
            Return _propertyChanges
        End Get
    End Property

    <Browsable(False)>
    <System.Xml.Serialization.XmlIgnore()>
    Public ReadOnly Property GenerateType As GenerateTypeEnum
        Get
            If Me.FileName.ToLower.EndsWith(".cer") Then
                Return GenerateTypeEnum.CerFile
            ElseIf Me.FileName.ToLower.EndsWith(".pfx") Then
                Return GenerateTypeEnum.PfxFile
            Else
                Throw New ApplicationException("Unknown filetype")
            End If
        End Get
    End Property
#End Region

#Region "Methods"
    Protected Friend Function GetCommandLineArguments() As String
        Dim commandLine As StringBuilder = New StringBuilder

        If Me.ExportablePrivateKey Then
            commandLine.Append(" -pe")
        End If
        commandLine.Append(" -a " & Me.Algorithm.ToString)
        commandLine.Append(" -n """ & Me.X500DistinguishedNamed & """")
        If Me.Exchangable Then
            commandLine.Append(" -sky exchange")
        End If
        If Me.GeneratePrivateKey Then
            commandLine.Append(" -sv " & Me.CertificatePvkFile)
        End If
        If Not Me.SelfSign Then
            commandLine.Append(" -iv """ & Me.CaCertificatePvkFile & """ -ic """ & Me.CaCertificateFile & """")
        End If
        commandLine.Append(" """ & Me.CertificateFile & """")

        Return commandLine.ToString
    End Function

    Private Shared Function GetFilePart(ByVal filename As String) As String
        Dim path As String = filename
        Return path.Substring(0, path.LastIndexOf("."))
    End Function

    Private Shared Function AppPath() As String
        Dim path As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        If path.EndsWith("\") Then
            Return path
        Else
            Return path & "\"
        End If
    End Function

    Public Sub Save()
        Dim xmlSerializer As System.Xml.Serialization.XmlSerializer = New System.Xml.Serialization.XmlSerializer(GetType(CertificateProperties))
        Using file As New IO.FileStream(AppPath() & "settings.dat", IO.FileMode.Create, IO.FileAccess.ReadWrite)
            xmlSerializer.Serialize(file, Me)
        End Using
    End Sub

    Public Shared Function Open() As CertificateProperties
        Dim value As CertificateProperties

        If IO.File.Exists(AppPath() & "settings.dat") Then
            Dim xmlSerializer As System.Xml.Serialization.XmlSerializer = New System.Xml.Serialization.XmlSerializer(GetType(CertificateProperties))

            Using file As New IO.FileStream(AppPath() & "settings.dat", IO.FileMode.Open, IO.FileAccess.Read)
                value = xmlSerializer.Deserialize(file)
            End Using
        Else
            value = New CertificateProperties
        End If

        Return value
    End Function
#End Region

#Region "Enums"
    Public Enum KeySizeEnum As Integer
        <Description("1024")>
        Bit1024 = 1024
        Bit2048 = 2048
        Bit4096 = 4096
    End Enum

    Public Enum AlgorithmEnum As Integer
        md5 = 1
        sha1 = 2
        sha256 = 3
        sha384 = 4
        sha512 = 5
    End Enum

    Public Enum GenerateTypeEnum As Integer
        CerFile = 0
        PfxFile = 1
    End Enum
#End Region

End Class

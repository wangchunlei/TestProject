Public Class RootCaDropdown
    Inherits ListTypeConverter

    Protected Friend Overrides Sub SetStandardValues(ByRef values As System.Collections.Generic.List(Of String))
        values = Tools.RootCAs
    End Sub
End Class

Public Class CertificateTypeDropdown
    Inherits ListTypeConverter

    Protected Friend Overrides Sub SetStandardValues(ByRef values As System.Collections.Generic.List(Of String))
        values = Tools.GetTemplates
    End Sub
End Class
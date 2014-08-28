Imports System.ComponentModel

Public MustInherit Class ListTypeConverter
    Inherits TypeConverter

    Private innerList As List(Of String) = New List(Of String)

    Public Sub New()

    End Sub

    Public Overrides Function GetStandardValuesSupported(context As System.ComponentModel.ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Public Overrides Function GetStandardValues(context As System.ComponentModel.ITypeDescriptorContext) As System.ComponentModel.TypeConverter.StandardValuesCollection
        innerList.Clear()
        SetStandardValues(innerList)

        Return (GetValues())
    End Function

    Protected Friend MustOverride Sub SetStandardValues(ByRef values As List(Of String))

    Public Overrides Function GetStandardValuesExclusive(context As System.ComponentModel.ITypeDescriptorContext) As Boolean
        Return True
    End Function

    Protected Sub SetList(ByVal list As List(Of String))
        innerList = list
    End Sub

    Private Function GetValues() As StandardValuesCollection
        Return New StandardValuesCollection(innerList)
    End Function
End Class

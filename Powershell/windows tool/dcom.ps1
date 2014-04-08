$user = "NetWork Service"
$dcom = Get-WmiObject -class "Win32_DCOMApplicationSetting" -Filter 'Description="Microsoft Word 97 - 2003 文档"' -EnableAllPrivileges
 
$sd = $dcom.GetLaunchSecurityDescriptor().Descriptor
$nsAce = $des.Dacl | Where {$_.Trustee.Name -eq $user}
if ($nsAce) {
    $nsAce.AccessMask = 31
}
else {
    $trustee = ([wmiclass] 'Win32_Trustee').CreateInstance()
    $trustee.Name = $user
    $ace = ([wmiclass] 'Win32_ACE').CreateInstance()
    $ace.AccessMask = 31
    $ace.AceFlags = 0
    $ace.AceType = 0
    $ace.Trustee = $trustee
    $des.Dacl += $ace
}
$dcom.SetLaunchSecurityDescriptor($sd)

#$ad = $dcom.GetAccessSecurityDescriptor().Descriptor



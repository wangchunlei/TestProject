[CmdletBinding()]
Param(
[string]$hostname="sd07",
[string]$user="*"
)

$session = New-PSSession $hostname
Invoke-Command -Session $session -ScriptBlock{
    Param($hname,$username)
    Write-Host "开始安装特性..."
    Install-WindowsFeature –Name WindowsPowerShellWebAccess -IncludeManagementTools -Restart

    Write-Host "开始安装应用..."
    Install-PswaWebApplication -UseTestCertificate
    
    Write-Host "开始安装规则..."
    Add-PswaAuthorizationRule –UserName $username -ComputerName $hname -ConfigurationName microsoft.powershell -Force
} -ArgumentList $hostname,$user
Remove-PSSession $session
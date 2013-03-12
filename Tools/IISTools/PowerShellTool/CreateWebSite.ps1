$session = New-PSSession -ComputerName localhost -Name SessionXyz

Invoke-Command{
		Import-Module 'E:\GitHub\TestProject\Tools\IISTools\PowerShellTool'
	} -Session $session
	
Write-Host "Just a place to take a break"

$message = Invoke-Command{
		Write-Debug -Message "We should see this come out in the other debugger."
	} -Session $session
	
$message

Remove-PSSession $session
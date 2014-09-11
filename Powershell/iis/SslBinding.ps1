Import-Module WebAdministration
New-SelfSignedCertificate -DnsName $Env:Computername, $Env:Computername -CertStoreLocation Cert:\LocalMachine\My
New-WebBinding -Name "Default Web Site" -IPAddress "*" -Port 443 -Protocol https
Get-ChildItem Cert:\LocalMachine\My | where{$_.Subject -match "CN\=$Env:Computername"} | select  -First 1 | New-Item IIS:\SslBindings\0.0.0.0!443
function test-linkspeed{
    param([string]$computer=".")

    Get-WmiObject -Namespace root\wmi -Class MSNdis_LinkSpeed -ComputerName $computer | 
    foreach{
        $nic = Get-WmiObject -Class Win32_NetworkAdapter -ComputerName $computer -Filter "Name = '$($_.InstanceName)'"

        New-Object -TypeName PSObject -Property @{
            Name = $_.InstanceName
            NdisLinkSpeed = $_.NdisLinkSpeed
            TestNdis = $_NdisLinkSpeed /10000
            Speed = $nic.Speed
            Test = $nic.Speed /1000000
        } | select Name, NdisLinkSpeed, TestNdis, Test, Speed
    }
}
test-linkspeed
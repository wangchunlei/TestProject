param (
    [string] $oldUser,
    [string] $newUser
)
Write-Host $oldUser $newUser
$logs = svn log --search $oldUser
foreach($log in $logs)
{
    try
    {
        if($log.Contains("$oldUser")){
            $r=$log.Split(" ")[0];
            $r = $r.Substring(1,$r.Length-1);
            if($r){
                svn propset --revprop -r $r svn:author $newUser
            }
        }
    }
    catch [System.Net.WebException],[System.Exception]
    {
        Write-Host "∆‰À˚“Ï≥£"
    }
    #svn propset --revprop -r $f svn:author yanhaibin
}
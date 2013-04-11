function update {
    param (
        [switch] $Enabled,
		[string[]] $folders = "Views"
    )
	
#		foreach($viewItems in (Get-Project).ProjectItems.Item("Views").ProjectItems){
#			foreach($item in $viewItems.ProjectItems){
#				$item.Properties.Item("BuildAction").Value = [int]3
#			}
#		}
#		$path = Split-Path ((Get-Project).FullName)
	
	$curProject = (Get-Project)
	foreach($folder in $folders){
		if($Enabled){
			updateItem $curProject "$folder" 3
		}
		else{
			updateItem $curProject "$folder" 0
		}
	}
#	foreach($folder in $folders)
#	{
##			$items=Get-ChildItem -path $path+$folder -Recurse
##			foreach($item in $items)
##			{
##				
##			}
##			Write-host "成功修改了文件的编译属性为嵌入Embeded Resource"
#		if($Enabled -eq "true"){
#			updateItem((Get-Project),$folder,3)
#		}else
#		{
#			updateItem((Get-Project),$folder,0)
#		}
#	}
	
#	}
#	else{
#		foreach($viewItems in (Get-Project).ProjectItems.Item("Views").ProjectItems){
#			foreach($item in $viewItems.ProjectItems){
#				$item.Properties.Item("BuildAction").Value = [int]0
#			}
#		}
#		Write-host "成功修改了文件的编译属性为None"
#	}
}
	function updateItem($project,[string]$folder,[int]$value){
		Write-Host $project.FullName
		if($folder -ne "null"){
			$items = $project.ProjectItems.Item("$folder").ProjectItems
		}else{
			$items = $project
		}

		foreach($viewItems in $items){
			Write-Host $viewItems.Name
			if($viewItems.ProjectItems -ne 0){
				
				updateItem $viewItems.ProjectItems "null" $value
			}else
			{
				$viewItems.Properties.Item("BuildAction").Value = $value
				if($value -eq 3){	
					Write-host "成功修改了文件[$($viewItems.Name)]的编译属性为嵌入Embeded Resource"
				}else{
					Write-host "成功修改了文件[$($viewItems.Name)]的编译属性为非嵌入Embeded Resource"
				}
			}
		}
	}

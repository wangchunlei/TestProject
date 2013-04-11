function update {
    param (
        [string] $Enabled
    )
	if($Enabled -eq "true"){
		foreach($viewItems in (Get-Project).ProjectItems.Item("Views").ProjectItems){
			foreach($item in $viewItems.ProjectItems){
				$item.Properties.Item("BuildAction").Value = [int]3
			}
		}
	}
}


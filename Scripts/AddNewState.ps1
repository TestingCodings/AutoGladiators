
param([string]$stateName)
$stateEnumPath = "../SharedLibs/StateMachineLib/StateMachine.cs"
Add-Content -Path $stateEnumPath -Value "        $stateName,"
Write-Host "New state '$stateName' added successfully!"

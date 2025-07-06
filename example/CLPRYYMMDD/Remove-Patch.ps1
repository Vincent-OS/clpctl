#!/usr/bin/pwsh
# Patch Creator: Florian. M
# Date: 2025-04-05

$patchFiles=@(
	# Define your files here
	"tmp/example-patch/new-file.txt"
	"tmp/example-patch/modified-file.txt"
	"tmp/example-patch/removed-file.txt"
)
$ogPatchDir="/opt/CLP/$(Split-Path -Leaf (Get-Location))"
$backupDir="${ogPatchDir}/backup"

Start-Transcript -Path "/var/log/CLP.log" -Append

foreach ($file in $patchFiles) {
	$backupFile = Join-Path -Path $backupDir -ChildPath (Split-Path -Leaf $file)
	if (Test-Path $backupFile) {
		Write-Host "[INFO] Restoring file: $backupFile"
		Copy-Item -Path -Verbose $backupFile -Destination $file
	}
	else {
		Write-Warning "[WARNING] Backup file not found: $backupFile"
	}
}
Write-Host "[INFO] All files restored successfully."

Stop-Transcript
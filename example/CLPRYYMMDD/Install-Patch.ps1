#!/usr/bin/pwsh
# Patch Creator: Florian. M
# Date: 2025-04-05

$patchFiles=(
	# Define your files here
	"tmp/example-patch/new-file.txt",
	"tmp/example-patch/modified-file.txt",
	"tmp/example-patch/removed-file.txt"
)
$ogPatchDir="/opt/CLP/$(Split-Path -Leaf (Get-Location))"
$backupDir="${ogPatchDir}/backup"

Start-Transcript -Path "/var/log/CLP.log" -Append

# Because we are never sure if the system is stable, we create a backup
# of the files we are going to modify.
function Confirm-BackupDirectory {
	mkdir $backupDir
	Write-Host "[INFO] Backup directory created at $backupDir"
}

function Backup-File {
	param (
		[string]$file
	)

	if (Test-Path -Path $file) {
		Copy-Item -Path $file -Destination $backupDir
		Write-Host "[INFO] Successfully backed up: $file"
	}
	else {
		Write-Warning "[WARNING] File: $file not found. Skipping backup."
	}
}

function Restore-Action {
	$revertScript = Join-Path -Path (Get-Location) -ChildPath "Remove-Patch.ps1"
	if (Test-Path -Path $revertScript) {
		Write-Host "[INFO] Reverting changes..."
		& $revertScript
	}
	else {
		Write-Error "[ERROR] Remove-Patch.ps1 is missing or not executable. Aborting."
		exit 2
	}
}

function Install-Patch {
	Confirm-BackupDirectory
	foreach ($file in $patchFiles) {
		Backup-File -File $file
		switch ($file) {
			"tmp/example-patch/new-file.txt" {
				Write-Host "[INFO] Installing new file: $file"
				Copy-Item -Path $file -Destination "/opt/CLP/new-file.txt"
			}
			"tmp/example-patch/modified-file.txt" {
				Write-Host "[INFO] Modifying file: $file"
				Copy-Item -Path $file -Destination "/opt/CLP/modified-file.txt" -Force
			}
			"tmp/example-patch/removed-file.txt" {
				Write-Host "[INFO] Removing file: $file"
				Remove-Item -Path "/opt/CLP/removed-file.txt" -Force
			}
			default {
				Write-Error "[FATAL] An unknown error occurred while applying the patch."
				Write-Error "[FATAL] Reverting all changes."
				./Remove-Patch.ps1
			}
		}
	}
	Write-Host "[INFO] All files installed successfully. Processed files:"
	foreach ($file in $patchFiles) {
		Write-Host "  - $file"
	}
	Stop-Transcript
	exit 0
}

Install-Patch
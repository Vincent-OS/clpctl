#!/bin/bash
# Patch Creator: Florian. M
# Date: 2025-04-05
(
    files=(
        # Define your files here
        "tmp/example-patch/new-file.txt"
        "tmp/example-patch/modified-file.txt"
        "tmp/example-patch/removed-file.txt"
    )
    ogPatchDir="/opt/CLP/$(basename $(pwd))"
    backupDir="${ogPatchDir}/backup"

    # Because we are never sure if the system is stable, we create a backup
    # of the files we are going to modify.
    ensureBackupDir() {
        mkdir -p "$backupDir"
        echo "[INFO] Backup directory created at $backupDir"
    }

    backupFile() {
        local file="$1"
        if [ -e "$file" ]; then
            cp "$file" "$backupDir"
            echo "[INFO] Successfully backed up: $file"
        else
            echo "[WARNING] File: $file not found. Skipping backup."
        fi
    }

    revertAction() {
        if [ -x "./revert.sh" ]; then
            echo "[INFO] Reverting changes..."
            ./revert.sh
        else
            echo "[ERROR] revert.sh is missing or not executable. Aborting."
            exit 2
        fi
    }

    patchApply() {
        ensureBackupDir
        for file in "${files[@]}"; do
            backupFile "$file"
            # After backing up, we can apply the patch
            # We use a switch case to handle different file operations
            # according to the list of files
            case "$file" in
                "${files[0]}")
                    # We create a new file
                    echo "[INFO] Creating new file: $file"
                    if [ -e "$ogPatchDir/${files[0]}" ]; then
                        cp "$ogPatchDir/${files[0]}" "/tmp/example-patch/"
                    else
                        echo "[WARNING] Source file not found: $ogPatchDir/${files[0]}"
                    fi
                    ;;
                "${files[1]}")
                    # We modify an existing file
                    echo "[INFO] Modifying file: $file"
                    if [ -e "$ogPatchDir/${files[1]}" ]; then
                        diff -u "$ogPatchDir/${files[1]}" "/tmp/example-patch/${files[1]}" | patch -p0
                        # Check if the patch was successful
                        if [ $? -ne 0 ]; then
                            echo "[ERROR] Failed to apply patch to ${files[1]}"
                            echo "[INFO] Reverting changes..."
                            revertAction
                        fi
                    else
                        echo "[ERROR] Source file not found: /${files[1]}"
                        echo "Aborting patch application."
                        exit 17
                    fi
                    ;;
                "${files[2]}")
                    # We remove an existing file
                    rm "/${files[2]}"
                    ;;
                *)
                    echo "[FATAL] An unknown error occurred while applying the patch."
                    echo "[FATAL] Reverting all changes."
                    # Execute the revert.sh script
                    revertAction
                    ;;
            esac
        done
        echo "[INFO] Patch applied successfully."
        exit 0
    }
    patchApply
) 2>&1 | tee -a /var/log/CLP.log
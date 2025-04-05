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

    revertApply() {
        # We replace all the backup files on the original folders localizations
        for file in "${files[@]}"; do
            if [ -e "$backupDir/$(basename "$file")" ]; then
                cp "$backupDir/$(basename "$file")" "$(dirname "$file")/"
                echo "[INFO] Reverted: $file"
            else
                echo "[WARNING] Backup file not found for: $file"
            fi
        done
        echo "[INFO] Reversion process completed."
    }

    revertApply
) 2>&1 | tee -a /var/log/CLP.log
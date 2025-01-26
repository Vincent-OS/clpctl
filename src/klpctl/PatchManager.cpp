#include <iostream>
#include <filesystem>
#include "include/PatchManager.h"
#include "include/FileExtrator.h"
#include "include/CommandRunner.h"

namespace fs = std::filesystem;

void PatchManager::install(const std::string& filePath) {
    if (!fs::exists(filePath)) {
        std::cerr << "Error: File not found: " << filePath << "\n";
        return;
    }

    std::cout << "Installing patch: " << filePath << "\n";

    std::string outputDir = "/usr/share/klp/" + fs::path(filePath).stem().string();
    fs::create_directories(outputDir);

    // Extrat file
    FileExtractor extractor;
    extractor.extract(filePath, outputDir);

    // Exécuter le script apply.sh
    CommandRunner runner;
    runner.execute(outputDir + "/apply.sh");

    std::cout << "Patch installed successfully.\n";
}

void PatchManager::uninstall(const std::string& patchName) {
    std::string patchDir = "/usr/share/klp/" + patchName;
    if (!fs::exists(patchDir)) {
        std::cerr << "Error: Patch not found: " << patchName << "\n";
        return;
    }

    std::cout << "Uninstalling patch: " << patchName << "\n";

    CommandRunner runner;
    std::string rollbackScript = patchDir + "/rollback.sh";
    if (fs::exists(rollbackScript)) {
        runner.execute(rollbackScript);
    }

    fs::remove_all(patchDir);
    std::cout << "Patch uninstalled successfully.\n";
}

void PatchManager::list() {
    std::string patchDir = "/usr/share/klp/";
    if (!fs::exists(patchDir)) {
        std::cout << "No patches installed.\n";
        return;
    }

    std::cout << "Installed patches:\n";
    for (const auto& entry : fs::directory_iterator(patchDir)) {
        if (fs::is_directory(entry)) {
            std::cout << "  - " << entry.path().filename().string() << "\n";
        }
    }
}
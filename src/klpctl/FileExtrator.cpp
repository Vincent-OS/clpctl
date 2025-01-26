#include "include/FileExtrator.h"
#include <iostream>
#include <cstdlib>

void FileExtractor::extract(const std::string& archivePath, const std::string& outputPath) {
    std::cout << "Extracting archive: " << archivePath << " to " << outputPath << "\n";
    std::string command = "tar -xvf " + archivePath + " -C " + outputPath;
    int result = std::system(command.c_str());
    if (result != 0) {
        std::cerr << "Error: Extraction failed.\n";
    }
}
#pragma once
#ifndef FILEEXTRACTOR_H
#define FILEEXTRACTOR_H

#include <string>

class FileExtractor {
	public:
		void extract(const std::string& archivePath, const std::string& outputPath);
};

#endif // !FILEEXTRACTOR_H

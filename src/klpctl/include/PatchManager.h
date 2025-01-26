#pragma once
#ifndef PATCHMANAGER_H
#define PATCHMANAGER_H

#include <string>

class PatchManager {
	public:
		void install(const std::string& file);
		void uninstall(const std::string& file);
		void list();
};
#endif // !PATCHMANAGER_H

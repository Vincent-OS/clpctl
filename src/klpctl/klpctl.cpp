#include <iostream>
#include <string>
#include "include/PatchManager.h"

int main(int argc, char* argv[]) {
	if (argc < 2) {
		std::cerr << "Usage: klpctl <command> [args]";
		std::cerr << "Commands:\n";
		std::cerr << "  install  <file>      Install a .klp patch file\n";
		std::cerr << "  uninstall <file>     Uninstall a .klp patch file\n";
		std::cerr << "  list                 List installed patches\n";
		return 1;
	}

	std::string command = argv[1];

	PatchManager patchManager;

	if (command == "install" && argc == 3) {
		patchManager.install(argv[2]);
	}
	else if (command == "uninstall" && argc == 3) {
		patchManager.uninstall(argv[2]);
	}
	else if (command == "list") {
		patchManager.list();
	}
	else {
		std::cerr << "Invalid command or missing arguments.\n";
	}
	return 0;
}
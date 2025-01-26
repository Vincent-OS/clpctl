#include "include/CommandRunner.h"
#include <iostream>
#include <cstdlib>

void CommandRunner::execute(const std::string& command) {
    std::cout << "Executing command: " << command << "\n";
    int result = std::system(command.c_str());
    if (result != 0) {
        std::cerr << "Error: Command execution failed.\n";
    }
}

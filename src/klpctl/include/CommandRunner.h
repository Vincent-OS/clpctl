#pragma once
#ifndef COMMANDRUNNER_H
#define COMMANDRUNNER_H

#include <string>

class CommandRunner {
    public:
        void execute(const std::string& command);
};

#endif // !COMMANDRUNNER_H
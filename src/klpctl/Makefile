CXX = g++
CXXFLAGS = -std=c++17 -Wall -Iinclude
LDFLAGS = 
SRCDIR = src
INCDIR = include
BUILDDIR = build
BINDIR = bin
TARGET = $(BINDIR)/klp

SOURCES = $(wildcard $(SRCDIR)/*.cpp)
OBJECTS = $(patsubst $(SRCDIR)/%.cpp, $(BUILDDIR)/%.o, $(SOURCES))

all: $(TARGET)

$(TARGET): $(OBJECTS)
	mkdir -p $(BINDIR)
	$(CXX) $(CXXFLAGS) $^ -o $@ $(LDFLAGS)

$(BUILDDIR)/%.o: $(SRCDIR)/%.cpp
	mkdir -p $(BUILDDIR)
	$(CXX) $(CXXFLAGS) -c $< -o $@

clean:
	rm -rf $(BUILDDIR) $(BINDIR)

help:
	@echo "Commandes disponibles:"
	@echo "  make         - Compile le programme"
	@echo "  make clean   - Supprime les fichiers générés"
	@echo "  make help    - Affiche cette aide"

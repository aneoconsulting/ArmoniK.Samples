
.PHONY: all

CXXFLAGS += -fPIC -DBUILD

SOURCES= ComputeNativeLib.cpp
OUTPUTFILE= libComputeNative.so
OBJDIR:= obj
SRCDIR:= src

SOURCES     := $(shell find $(SRCDIR) -type f -name *.cpp)
OBJECTS     := $(patsubst $(SRCDIR)/%,$(OBJDIR)/%,$(SOURCES:.cpp=.o))

all: prepare $(OUTPUTFILE)
	echo "Build Shared library"

clean:
	rm -rf obj bin 2>&1 > /dev/null || true

prepare:
	mkdir -p bin
	mkdir -p obj

$(OUTPUTFILE): $(OBJECTS)
	$(CXX) -shared -fPIC $(LDFLAGS) -o bin/$@ $^

$(OBJDIR)/%.o: $(SRCDIR)/%.cpp
	$(CXX) $(CPPFLAGS) $(CXXFLAGS) -c -o $@ $<

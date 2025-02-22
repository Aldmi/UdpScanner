cmake_minimum_required(VERSION 3.28)
project(cpp_tag_debian12)

set(CMAKE_CXX_STANDARD 20)

add_executable(cpp_tag_debian12 main.cpp
        src/Contracts/TagRequest.h
        src/Shared/Shared.h
        src/Contracts/TagResponse.h
        src/Payload/TagPayload.h
        src/Application/TagPing.h)

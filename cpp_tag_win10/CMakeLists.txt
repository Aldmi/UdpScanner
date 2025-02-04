cmake_minimum_required(VERSION 3.17)
project(udp_tag)

set(CMAKE_CXX_STANDARD 20)

add_executable(udp_tag
        main.cpp
        src/Tag/TagPing.h
        src/Payloads/TagPayload.h
        src/Payloads/ScannerPayload.h)

if(WIN32)
    target_link_libraries(udp_tag ws2_32)
endif(WIN32)
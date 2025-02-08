cmake_minimum_required(VERSION 3.17)
project(udp_tag)

set(CMAKE_CXX_STANDARD 20)

add_executable(udp_tag
        main.cpp
        src/Tag/TagPing.h
        src/Contracts/TagResponse.h
        src/Contracts/TagRequest.h
        src/Shared/Shared.h
        src/Payload/TagPayload.h)

if(WIN32)
    target_link_libraries(udp_tag ws2_32)
    target_link_libraries(udp_tag iphlpapi)
endif(WIN32)
#include <iostream>
#include "src/Tag/TagPing.h"

int main()
{
    std::cout << "Starting" << std::endl;

    //Прочитать настройки из .env---------
    int listenPort= 11000;
    //------------------------------------

    string errorMessage;
    auto udpPing = TagPing(listenPort);

    bool cancelFlag = false;
    auto res= udpPing.StartWork(cancelFlag);

    return 0;
}

#include <iostream>
#include "src/Tag/TagPing.h"

int main()
{
    std::cout << "Starting" << std::endl;

    //Прочитать настройки из .env---------
    int listenPort= 11000;
    string tagName= "Device1";
    //------------------------------------

    //Получить информацию про устройство
    auto tagPayload= TagPayload();
    auto payload= tagPayload.printMACAddress();
    std::cout << payload << std::endl;
    //------------------------------------

    //Запустить задачу обработки запросов от сканера
    auto udpPing = TagPing(listenPort, tagName, payload);
    bool cancelFlag = false;
    auto res= udpPing.StartWork(cancelFlag);

    return 0;
}

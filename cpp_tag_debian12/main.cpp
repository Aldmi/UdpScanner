#include <iostream>
#include "src/Payload/TagPayload.h"
#include "src/Application/TagPing.h"

const string APP_NAME= "Udp_Tag";
const string VER= "1.0.0";

int main()
{
    cout << APP_NAME <<" : " << VER << endl;

    //Прочитать настройки из .env---------
    int listenPort= 11000;
    string tagName= "Device1";
    //------------------------------------

    //Получить информацию про устройство
    auto tagPayload= TagPayload();
    tagPayload.collectInformation();
    tagPayload.printMACAddress();
    //------------------------------------

    //Запустить задачу обработки запросов от сканера
    auto udpPing = TagPing(listenPort, tagName, tagPayload);
    bool cancelFlag = false;
    auto res= udpPing.StartWork(cancelFlag);

    return 0;
}

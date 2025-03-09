#include <iostream>
#include "src/Payload/TagPayload.h"
#include "src/Application/TagPing.h"
#include "src/Settings/Settings.h"

const string APP_NAME= "Udp_Tag";
const string VER= "1.0.0";

int main()
{
    cout << "Version:" << APP_NAME <<" : " << VER << endl;

    //Получить настройки----------------------------------------------------------------
    if(Settings::CreateSettingsFrom_Env() < 0)
        return -1;

    //Получить информацию про устройство
    auto tagPayload= TagPayload();
    tagPayload.collectInformation();
    tagPayload.printMACAddress();
    //------------------------------------

    //Запустить задачу обработки запросов от сканера
    auto udpPing = TagPing(Settings::listenPort, Settings::tagName, tagPayload);
    bool cancelFlag = false;
    auto res= udpPing.StartWork(cancelFlag);

    return 0;
}

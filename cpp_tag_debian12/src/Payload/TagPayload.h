#ifndef UDP_TAG_TAGPAYLOAD_H
#define UDP_TAG_TAGPAYLOAD_H

#include <iostream>
#include <fstream>
#include <sstream>
#include <string>
#include <iomanip>
#include <vector>
#include <map>
#include <filesystem>

using namespace std;
namespace fs = std::filesystem;


class TagPayload
{
private:
    map<string, string> macAddressMap;

    /*
    На Linux можно прочитать MAC-адрес из файла в /sys/class/net/
    interface - имя интерфейса (eth0)
    */
    int readAllMacAddress()
    {
        string directoryPath = "/sys/class/net/";
        // Проверяем, существует ли каталог
        if (!fs::exists(directoryPath))
        {
            std::cerr << "Каталог не существует: " << directoryPath << std::endl;
            return -1;
        }

        // Итерируем по содержимому каталога
        for (const auto& entry: fs::directory_iterator(directoryPath))
        {
            if (entry.is_directory())
            {
                auto interfacePath= entry.path().filename().string();
                string filePath= directoryPath + interfacePath + "/address"; //Путь до файла с mac адресом
                if(fs::is_regular_file(filePath))
                {
                    auto macAddress= readMACAddressFromFile(filePath);
                    macAddressMap[interfacePath]=macAddress;
                }
            }
        }
        return 0;
    }

    static string readMACAddressFromFile(const string& filePath)
    {
        ifstream file(filePath);
        string macAddress;
        if (file)
        {
            getline(file, macAddress);
        }
        return macAddress;
    }


public:
    int collectInformation()
    {
        int result=0;
        result= readAllMacAddress();
        if(result < 0)
            return result;

        return result;
    }


    void printMACAddress()
    {
        if(macAddressMap.empty())
        {
            std::cout << "Информация о системе не собрана" << ":" << std::endl;
            return;
        }

        std::cout << "Информация" << ":" << std::endl;
        for (const auto& [interface, macAddress] : macAddressMap)
        {
            std::cout << interface << ":" << "\t" << macAddress << std::endl;
        }
    }
};


#endif //UDP_TAG_TAGPAYLOAD_H

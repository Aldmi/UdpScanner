#ifndef UDP_TAG_TAGPAYLOAD_H
#define UDP_TAG_TAGPAYLOAD_H

#include <iostream>
//#include <winsock2.h>
//#include <iphlpapi.h>
#include <iomanip>

using namespace std;

class TagPayload
{

public:
//    string printMACAddress()
//    {
//        ostringstream oss;
//        ULONG bufferSize = 0;
//        GetAdaptersAddresses(AF_UNSPEC, 0, nullptr, nullptr, &bufferSize);
//        IP_ADAPTER_ADDRESSES* adapterAddresses = (IP_ADAPTER_ADDRESSES*)malloc(bufferSize);
//        if (GetAdaptersAddresses(AF_UNSPEC, 0, nullptr, adapterAddresses, &bufferSize) == ERROR_SUCCESS)
//        {
//            oss << "["<< endl;
//            for (IP_ADAPTER_ADDRESSES* adapter = adapterAddresses; adapter != nullptr; adapter = adapter->Next)
//            {
//                if (adapter->PhysicalAddressLength > 0)
//                {
//                    string adapterName = PWCHARToString(adapter->FriendlyName);
//                    oss << "Adapter: " << adapterName << std::endl;
//                    oss << "MAC Address: ";
//                    for (DWORD i = 0; i < adapter->PhysicalAddressLength; i++)
//                    {
//                        oss << hex << setw(2) << setfill('0') << (int)adapter->PhysicalAddress[i];
//                        if (i < adapter->PhysicalAddressLength - 1)
//                        {
//                            oss << ":";
//                        }
//                    }
//                    oss << dec << endl;
//                }
//            }
//            oss << "]";
//        }
//
//        free(adapterAddresses);
//        return oss.str();
//    }
//
//
//
//
//    std::string PWCHARToString(PWCHAR wideStr)
//    {
//        if (!wideStr) {
//            return ""; // Возвращаем пустую строку, если входной указатель равен nullptr
//        }
//
//        // Определяем длину строки после преобразования
//        int length = WideCharToMultiByte(CP_UTF8, 0, wideStr, -1, nullptr, 0, nullptr, nullptr);
//        if (length == 0) {
//            return ""; // Ошибка преобразования
//        }
//
//        // Выделяем буфер для многобайтовой строки
//        std::string narrowStr(length, 0);
//
//        // Выполняем преобразование
//        WideCharToMultiByte(CP_UTF8, 0, wideStr, -1, &narrowStr[0], length, nullptr, nullptr);
//
//        // Убираем завершающий нулевой символ
//        narrowStr.pop_back();
//
//        return narrowStr;
//    }


};


#endif //UDP_TAG_TAGPAYLOAD_H

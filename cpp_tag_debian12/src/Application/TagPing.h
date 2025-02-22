#ifndef CPP_TAG_DEBIAN12_TAGPING_H
#define CPP_TAG_DEBIAN12_TAGPING_H

#include <iostream>
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>
#include <errno.h>
#include "../Payload/TagPayload.h"
#include "../Contracts/TagRequest.h"
#include "../Contracts/TagResponse.h"

using namespace std;

typedef int SOCKET;
const int BUFFER_SIZE = 1024;
const int failedCounterMax = 5;

enum class ListenPingResult
{
    Cancelled,
    Error
};

enum class WorkState
{
    None,
    InitSocketLib,
    CreateUdpSocket,
    BindAddress,
    ListenPing,
    Error
};


class TagPing
{
private:
    SOCKET _sockfd;
    int _listenPort;
    string _tagName;
    TagPayload _tagPayload;
    WorkState _workState = WorkState::None;
    string _workErrorMessage;

    void Print()
    {
        string workStateStr= "";
        switch (_workState)
        {
            case WorkState::None:
                workStateStr = "None";
                break;
            case WorkState::InitSocketLib:
                workStateStr = "InitSocketLib";
                break;
            case WorkState::CreateUdpSocket:
                workStateStr = "CreateUdpSocket";
                break;
            case WorkState::BindAddress:
                workStateStr = "BindAddress";
                break;
            case WorkState::ListenPing:
                workStateStr = "ListenPing";
                break;
            case WorkState::Error:
                workStateStr = "Error";
                break;
        }

        cout << "State= " << workStateStr;
        if(_workState == WorkState::Error)
        {
            cout << "[" << _workErrorMessage << "]";
        }
        cout << std::endl;
    }

    void getErrorDescription(const char* error)
    {
        // Получаем описание ошибки
        char error_msg[256];
        snprintf(error_msg, sizeof(error_msg), "%s:  %s", error, strerror(errno));
        _workErrorMessage = error_msg;
    }


public:
    TagPing(int listenPort, string tagName, TagPayload& tagPayload)
    {
        _listenPort = listenPort;
        _tagName = tagName;
        _tagPayload = tagPayload;
    }


    //Запуск основной задачи
    bool StartWork(bool& cancelFlag)
    {
        while (!cancelFlag)
        {
            Print();
            if(_workState == WorkState::Error)
            {
                Dispose();
                sleep(1);
                cout << std::endl << "Dispose success" << std::endl;
            }

            _workState = CreateUdpSocket();
            Print();
            if(_workState == WorkState::Error)
                continue;

            _workState = BindAddress();
            Print();
            if(_workState == WorkState::Error)
                continue;

            _workState = WorkState::ListenPing;
            Print();
            auto res= LitenPing(cancelFlag);
            if(res == ListenPingResult::Error)//если ошибки сокета, то закрыть сетевое подключение
            {
                _workState = WorkState::Error;
            }
        }
        Dispose();
        return true;//Остановили задачу.
    }


    /* Создать сокет UDP
     * AF_INET- определяет семейство адресов (IPv4)
     * SOCK_DGRAM - указывает тип сокета (датаграмма, которая используется для UDP)
     * IPPROTO_UDP - протокол (UDP).
    */
    WorkState CreateUdpSocket()
    {
        // Создаем UDP-сокет
        _sockfd = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
        if (_sockfd < 0)
        {
            getErrorDescription("Create socket failed");
            return WorkState::Error;
        }

        // Разрешаем сокету принимать широковещательные пакеты
        int broadcastEnable = 1;
        if (setsockopt(_sockfd, SOL_SOCKET, SO_BROADCAST, &broadcastEnable, sizeof(broadcastEnable)))
        {
            getErrorDescription("Error configuring socket for broadcast");
            return WorkState::Error;
        }

        return WorkState::CreateUdpSocket;
    }


    /*
     * Настроить сокет на прослушивание входящих сообщений от сканера
     * Привязка к INADDR_ANY позволяет серверу принимать пакеты, отправленные на любой из IP-адресов машины
     * Если вы хотите принимать только локальные подключения, привяжите его к INADDR_LOOPBACK «127.0.0.1».
     */
    WorkState BindAddress() //listenPort=11000
    {
        // Настраиваем адрес для приема широковещательных пакетов
        struct sockaddr_in serverAddr{};
        memset(&serverAddr, 0, sizeof(serverAddr));
        serverAddr.sin_family = AF_INET;
        serverAddr.sin_port = htons(_listenPort);
        serverAddr.sin_addr.s_addr = htonl(INADDR_ANY);; // Принимаем пакеты на все интерфейсы
        // Привязываем сокет к адресу
        if (bind(_sockfd, (struct sockaddr*)&serverAddr, sizeof(serverAddr)) < 0)
        {
            getErrorDescription("Error while binding socket");
            return WorkState::Error;
        }
        return WorkState::BindAddress;
    }


    /*
     *  Слушает UDP socket и отвечает на ping запрос от сканера
     */
    ListenPingResult LitenPing(bool& cancelFlag)
    {
        std::cout << "The server is running and listening for broadcast packets on the port. " << _listenPort << std::endl;
        cout << std::endl << "+++++++++++++++++++++++++++++++++++++++++++"<< std::endl;
        char buffer[BUFFER_SIZE];
        struct sockaddr_in clientAddr{}; //при получении данных, заполнится адресом сканера.
        socklen_t clientAddrLen = sizeof(clientAddr);

        int failedCounter = 0;
        while (!cancelFlag)
        {
            try
            {
                size_t recvLen= ReceveTagRequest(buffer, clientAddr, clientAddrLen); //TODO: Добавить заголовок пакета, чтобы идентифицировать именно запросы от сканера.
                if(recvLen <= 0)
                {
                    //Ошибка получения данных от сканера
                    getErrorDescription("Error receiving data");
                    cout << _workErrorMessage << std::endl;
                    if(failedCounter++ > failedCounterMax)
                    {
                        return ListenPingResult::Error;
                    }
                    continue;
                }

                //Читаем данные от сканера.
                auto tagRequest= TagRequest::CreateFromBuffer(buffer, recvLen);
                std::cout << "TagRequest Form " << inet_ntoa(clientAddr.sin_addr) << ": " << tagRequest.toString() << std::endl;

                //создать ответ сканеру.
                string payloadInfo= _tagPayload.getInformation();
                auto tagResponse= TagResponse(_tagName, payloadInfo);
                cout << "TagResponse " << tagResponse.toString() << std::endl;

                //Отправить ответ сканеру.
                size_t sendLen= SendTagResponse(clientAddr, clientAddrLen, tagRequest.ReceiverPort, tagResponse);
                if(sendLen <= 0)
                {
                    //Ошибка Отправки ответа сканеру
                    getErrorDescription("Error sending data");
                    cout << _workErrorMessage << std::endl;
                    if(failedCounter++ > failedCounterMax)
                    {
                        return ListenPingResult::Error;
                    }
                    continue;
                }

                printf("successfully send Bytes '%d' >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n", sendLen);
                std::cout << std::endl;
            }
            catch (const std::exception& e)
            {
                std::ostringstream oss;
                oss << "Exception: " << e.what();
                _workErrorMessage = oss.str();
                return ListenPingResult::Error;
            }
        }
        return ListenPingResult::Cancelled;
    }


    ssize_t ReceveTagRequest(char* buffer, sockaddr_in& clientAddr, socklen_t& clientAddrLen)
    {
        // Принимаем широковещательный пакет
        auto recvLen = recvfrom(_sockfd, buffer, BUFFER_SIZE, 0, (struct sockaddr*)&clientAddr, &clientAddrLen);
        return recvLen;
    }


    // Отправить Payload
    ssize_t SendTagResponse(sockaddr_in& clientAddr, socklen_t& clientAddrLen, int receiverPort, TagResponse& tagResponse)
    {
        //выставить порт в который принимает сканер ответы от тега.
        clientAddr.sin_port = htons(receiverPort);
        auto response= tagResponse.GetStringResponse(); //"Device1_10-20-30-40-5F-FF_1739006312"
        auto sendLen = sendto(_sockfd, response.c_str(), response.size(), 0, (struct sockaddr*)&clientAddr, clientAddrLen);
        return sendLen;
    }


    void Dispose()
    {
        close(_sockfd);
    }
};



#endif //CPP_TAG_DEBIAN12_TAGPING_H

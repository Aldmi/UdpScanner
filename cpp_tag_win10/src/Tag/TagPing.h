#ifndef UDP_TAG_TAGPING_H
#define UDP_TAG_TAGPING_H

#include <WinSock2.h>
#include <Ws2tcpip.h>
#include <unistd.h>
#include <sstream>

using namespace std;

const int BUF_SIZE = 1024;
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
    LitenPing,
    Error
};

class TagPing
{
private:
    SOCKET serverSocket;
    int bytes_received;
    char serverBuf[BUF_SIZE];
    int _listenPort;
    WorkState workState = WorkState::None;
    string workErrorMessage;

    void Print()
    {
        string workStateStr= "";
        switch (workState)
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
            case WorkState::LitenPing:
                workStateStr = "LitenPing";
                break;
            case WorkState::Error:
                workStateStr = "Error";
                break;
        }

        cout << "State= " << workStateStr;
        if(workState == WorkState::Error)
        {
            cout << "[" << workErrorMessage << "]";
        }
        cout << std::endl;
    }

public:
    TagPing(int listenPort)
    {
        _listenPort = listenPort;
    }


    //Запуск основной задачи
    bool StartWork(bool& cancelFlag)
    {
        while (!cancelFlag)
        {
            Print();
            if(workState == WorkState::Error)
            {
                Dispose();
                sleep(1);
                cout << std::endl << "Dispose success" << std::endl ;
            }

            workState = InitSocketLib();
            Print();
            if(workState == WorkState::Error)
            {
                return false;
            }

            workState = CreateUdpSocket();
            Print();
            if(workState == WorkState::Error)
                continue;

            workState = BindAddress(_listenPort);
            Print();
            if(workState == WorkState::Error)
                continue;

            workState = WorkState::LitenPing;
            Print();
            auto res= LitenPing(cancelFlag);
            if(res == ListenPingResult::Error)//если ошибки сокета, то закрыть сетевое подключение
            {
                workState = WorkState::Error;
            }
        }

        return true;//Остановили задачу.
    }



//инициализации библиотеки Winsock в приложении Windows
    WorkState InitSocketLib()
    {
        WSADATA wsaData;
        int res = WSAStartup(MAKEWORD(2, 2), &wsaData);
        if (res != NO_ERROR)
        {
            std::ostringstream oss;
            oss << "WSAStartup failed with error: " << res;
            workErrorMessage = oss.str();
            return WorkState::Error;
        }
        return WorkState::InitSocketLib;
    }


    /* Создать сокет UDP
     * AF_INET- определяет семейство адресов (IPv4)
     * SOCK_DGRAM - указывает тип сокета (датаграмма, которая используется для UDP)
     * IPPROTO_UDP - протокол (UDP).
    */
    WorkState CreateUdpSocket()
    {
        serverSocket = INVALID_SOCKET;
        serverSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
        if (serverSocket == INVALID_SOCKET)
        {
            std::ostringstream oss;
            oss << "socket failed with error: " << serverSocket;
            workErrorMessage = oss.str();
            return WorkState::Error;
        }
        return WorkState::CreateUdpSocket;
    }


    /*
     * Настроить сокет на прослушивание входящих сообщений от сканера
     * Привязка к INADDR_ANY позволяет серверу принимать пакеты, отправленные на любой из IP-адресов машины
     * Если вы хотите принимать только локальные подключения, привяжите его к INADDR_LOOPBACK «127.0.0.1».
     */
    WorkState BindAddress(int listenPort) //listenPort=11000
    {
        struct sockaddr_in serverAddr;
        serverAddr.sin_family = AF_INET;
        serverAddr.sin_port = htons(listenPort);
        serverAddr.sin_addr.s_addr = htonl(INADDR_ANY);

        if (bind(serverSocket, (SOCKADDR *) & serverAddr, sizeof (serverAddr)))
        {
            std::ostringstream oss;
            oss << "bind failed with error: " << WSAGetLastError();
            workErrorMessage = oss.str();
            return WorkState::Error;
        }
        return WorkState::BindAddress;
    }


    /*
     *  Слушает UDP socket и отвечает на ping запрос от сканера
     */
    ListenPingResult LitenPing(bool& cancelFlag)
    {
        int failedCounter = 0;
        while (!cancelFlag)
        {
            try
            {
                struct sockaddr_in SenderAddr;
                int senderAddrSize = sizeof(SenderAddr);

                auto success= WaitScannerData(SenderAddr, senderAddrSize);
                if(!success)
                {
                    //Ошибка получения данных от сканера
                    if(failedCounter++ > failedCounterMax)
                    {
                        return ListenPingResult::Error;
                    }
                    continue;
                }

                ScannerDataHandle(serverBuf, bytes_received);

                CreateTagPayload();

                success= SendTagPayload(SenderAddr);
                if(!success)
                {
                    //Ошибка Отправки ответа сканеру
                    if(failedCounter++ > failedCounterMax)
                    {
                        return ListenPingResult::Error;
                    }
                    continue;
                }
                std::cout << std::endl;
            }
            catch (const std::exception& e)
            {
                std::ostringstream oss;
                oss << "Exception: " << e.what();
                workErrorMessage = oss.str();
                return ListenPingResult::Error;
            }
        }
        return ListenPingResult::Cancelled;
    }


    bool WaitScannerData(sockaddr_in& senderAddr, int& senderAddrSize)
    {
        printf("Waiting datagrams on Scanner...\n");
        bytes_received = recvfrom(serverSocket, serverBuf, BUF_SIZE, 0 /* no flags*/, (SOCKADDR *) &senderAddr,&senderAddrSize);  //bocking ожидание запроса от клиента
        if (bytes_received == SOCKET_ERROR)
        {
            std::ostringstream oss;
            oss << "recvfrom failed with error: " << WSAGetLastError();
            workErrorMessage = oss.str();
            return false;
        }
        return true;
    }


     // Обработка ping запроса от сканера.
     void ScannerDataHandle(char* data, size_t size) //TODO: вернуть ScannerPayload
     {
         printf("Receiving data on '%s\n'", data);
     }


    // Создать TagPayload
    void CreateTagPayload() //TODO: вернуть TagPayload
    {

    }


    // Отправить Payload
    bool SendTagPayload(sockaddr_in& senderAddr)
    {
        //Данные с tagPayload
        std::string tagPayload = "Device1_10-20-30-40-5F-FF_02.02.2025 15:15:16"; //
        auto listenPort = 11001;
        int sendBufLen = (int) (tagPayload.length());

        senderAddr.sin_port = htons(listenPort);
        int sendResult = sendto(serverSocket, tagPayload.data(), sendBufLen, 0, (SOCKADDR *) &senderAddr, sizeof(senderAddr));
        if (sendResult == SOCKET_ERROR)
        {
            std::ostringstream oss;
            oss << "Sending back response got an error: " << WSAGetLastError();
            workErrorMessage = oss.str();
            return false;
        }

        printf("Sending back response Bytes %d\n", sendResult); //Кол-во отправленых байт
        return true;
    }


    void Dispose()
    {
        closesocket(serverSocket);
        WSACleanup();
    }
};

















#endif //UDP_TAG_TAGPING_H

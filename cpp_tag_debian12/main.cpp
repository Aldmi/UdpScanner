#include <iostream>
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>

const int BROADCAST_PORT = 11000; // Порт для широковещательной рассылки
const int BUFFER_SIZE = 1024;     // Размер буфера для приема данных


int main()
{
    // Создаем UDP-сокет
    int sockfd = socket(AF_INET, SOCK_DGRAM, 0);
    if (sockfd < 0) {
        std::cerr << "Ошибка при создании сокета" << std::endl;
        return 1;
    }

    // Разрешаем сокету принимать широковещательные пакеты
    int broadcastEnable = 1;
    if (setsockopt(sockfd, SOL_SOCKET, SO_BROADCAST, &broadcastEnable, sizeof(broadcastEnable)))
    {
        std::cerr << "Ошибка при настройке сокета для широковещательной рассылки" << std::endl;
        close(sockfd);
        return 1;
    }

    // Настраиваем адрес для приема широковещательных пакетов
    struct sockaddr_in serverAddr{};
    memset(&serverAddr, 0, sizeof(serverAddr));
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(BROADCAST_PORT);
    serverAddr.sin_addr.s_addr = INADDR_ANY; // Принимаем пакеты на все интерфейсы
    // Привязываем сокет к адресу
    if (bind(sockfd, (struct sockaddr*)&serverAddr, sizeof(serverAddr)) < 0)
    {
        std::cerr << "Ошибка при привязке сокета" << std::endl;
        close(sockfd);
        return 1;
    }

    std::cout << "Сервер запущен и ожидает широковещательные пакеты на порту " << BROADCAST_PORT << std::endl;

    char buffer[BUFFER_SIZE];
    struct sockaddr_in clientAddr;
    socklen_t clientAddrLen = sizeof(clientAddr);

    while (true)
    {
        // Принимаем широковещательный пакет
        ssize_t recvLen = recvfrom(sockfd, buffer, BUFFER_SIZE, 0, (struct sockaddr*)&clientAddr, &clientAddrLen);
        if (recvLen < 0)
        {
            std::cerr << "Ошибка при приеме данных" << std::endl;
            continue;
        }

        // Выводим полученное сообщение
        buffer[recvLen] = '\0'; // Добавляем завершающий нулевой символ
        std::cout << "Получено от " << inet_ntoa(clientAddr.sin_addr) << ": " << buffer << std::endl;

        // Отправляем ответ
        clientAddr.sin_port = htons(11001);
        std::string response = "Device1_10-20-30-40-5F-FF_1739006312";
        if (sendto(sockfd, response.c_str(), response.size(), 0, (struct sockaddr*)&clientAddr, clientAddrLen) < 0)
        {
            std::cerr << "Ошибка при отправке ответа" << std::endl;
        }
    }

    // Закрываем сокет (эта строка никогда не выполнится в данном примере)
    close(sockfd);
    return 0;
}

#ifndef UDP_TAG_SHARED_H
#define UDP_TAG_SHARED_H

#include <iostream>
#include <sstream>
#include <vector>
#include <string>
#include <chrono>


using namespace std;
using namespace std::chrono;
using Byte = unsigned char;


/// <summary>
/// Разделяет строку на подстроки, по delimiter
/// </summary>
vector<string> split(const string& str, char delimiter)
{
    vector<string> tokens;
    stringstream ss(str); // Создаем поток из строки
    string token;

    // Разделяем строку по delimiter
    while (getline(ss, token, delimiter)) {
        tokens.push_back(token); // Добавляем каждую часть в вектор
    }

    return tokens;
}


/// <summary>
/// 4 байта в int
/// </summary>
int buffToInteger(Byte* buffer)
{
    return *reinterpret_cast<int*>(buffer);
}

/// <summary>
/// время UnixTime к строке
/// </summary>
string UnixTimeToString(int64_t ut, const char* format) //"%d-%m-%Y %H:%M:%S (GMT %z)"
{
    std::time_t temp = ut;
    std::tm* t = std::gmtime(&temp);
    std::stringstream ss; // or if you're going to print, just input directly into the output stream
    ss << std::put_time(t, format);
    return ss.str();
}


//unix time
int64_t unixTimeNowUtc()
{
    int64_t timestamp = std::chrono::duration_cast<std::chrono::milliseconds>
            (
                    std::chrono::system_clock::now().time_since_epoch()
            ).count();

    return timestamp/1000;
}


#endif //UDP_TAG_SHARED_H

#ifndef UDP_TAG_TAGRESPONSE_H
#define UDP_TAG_TAGRESPONSE_H

#include <sstream>
#include "../Shared/Shared.h"

using namespace std;

///ответ сканеру.
class TagResponse
{
public:
    string Name;
    string Payload;
    int64_t CreatedAtUnixTime;

    TagResponse(string name, string payload)
    {
        Name = name;
        Payload = payload;
        CreatedAtUnixTime = unixTimeNowUtc();
    }

    string GetStringResponse()
    {
        std::ostringstream oss;
        oss << Name << "_" << Payload << "_" << CreatedAtUnixTime;
        return oss.str();
    }

    string toString()
    {
        std::ostringstream oss;
        oss
        << " Name= " << Name
        << " Payload= " << Payload
        << " CreatedAt= " << UnixTimeToString(CreatedAtUnixTime, "%d-%m-%Y %H:%M:%S");
        return oss.str();
    }
};

#endif //UDP_TAG_TAGRESPONSE_H

#ifndef UDP_TAG_TAGREQUEST_H
#define UDP_TAG_TAGREQUEST_H

#include <sstream>
#include "../Shared/Shared.h"

using namespace std;


///запрос от сканера.
class TagRequest
{
public:
    int ReceiverPort;
    int64_t CreatedAtUnixTime;

    TagRequest(int listenPortNumber, int64_t createdAtUnixTime)
    {
        ReceiverPort = listenPortNumber;
        CreatedAtUnixTime = createdAtUnixTime;
    }


    static TagRequest CreateFromBuffer(const char* data)
    {
        auto parts= split(data, '_');
        if(parts.size() != 2)
        {
            throw std::invalid_argument( "[TagRequest.CreateFromBuffer] parts in buffer != 2" );
        }

        auto portNumber= std::stoi(parts[0]);
        int64_t ut= std::stol(parts[1]);
        TagRequest scannerPayload(portNumber, ut);
        return scannerPayload;
    }


    string Print()
    {
        std::ostringstream oss;
        oss << "ReceiverPort= " << ReceiverPort << " CreatedAt= " << UnixTimeToString(CreatedAtUnixTime, "%d-%m-%Y %H:%M:%S");
        return oss.str();
    }


};


#endif //UDP_TAG_TAGREQUEST_H

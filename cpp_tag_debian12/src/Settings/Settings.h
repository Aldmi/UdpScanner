#ifndef CPP_TAG_DEBIAN12_SETTINGS_H
#define CPP_TAG_DEBIAN12_SETTINGS_H

#include "../Shared/Shared.h"

using namespace std;

struct Settings {
    inline static string tagName;
    inline static int listenPort;


    inline static int CreateSettingsFrom_Env()
    {
        try
        {
            if(getenv("UDP_TAG_NAME") != nullptr) {
                Settings::tagName= getenv("UDP_TAG_NAME");
            }
            if(getenv("UDP_TAG_LISTEN_PORT") != nullptr) {
                int numberLastElements = stoi(getenv("UDP_TAG_LISTEN_PORT"));
                Settings::listenPort= numberLastElements;
            }

//            tcp_addr="10.35.33.95:9220";
//            kafka_addr="10.0.0.97:9092";

            cout << "UDP_TAG_LISTEN_PORT: " << listenPort << endl;
            cout << "UDP_TAG_NAME: " << tagName << endl;
        }
        catch (const std::exception& ex)
        {
            cerr <<"Exception .env parser " << "'" << ex.what() << "'" <<endl;
            return -1;
        }
        return 0;
    }


};

#endif //CPP_TAG_DEBIAN12_SETTINGS_H

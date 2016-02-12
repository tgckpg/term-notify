# checking out the sources
```bash
git clone ...
git submodule init
git submodule update
```

# Compiling term-notify
1. Associate your app with Store first ( this is because you need a auth token from WNS, whick requires your app to be associated. But not neccessarily be published )
2. You need to setup your own service_uri in ChannelInfo/ChannelInfo.cs, that means you need to setup your own server.
  * The service used by term-notify server can be found [here](https://github.com/tgckpg/term-notify-server). But you can always write your own server.


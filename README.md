### checking out the sources
```bash
git clone ...
git submodule init
git submodule update
```

### Compiling term-notify
It is recommended to host your own services. However if you just want to improve the term-notify client, but don't want go through the host-your-own-service thing. The `Default ( Astropenguin )` settings of ChannelInfo can be found [here](https://github.com/tgckpg/term-notify/blob/507b6084b1421ea6473f2b67bf07513fd087c1d1/ChannelInfo/ChannelInfo.cs).

* Please note that default server `notify.astropenguin.net` is a dynamic ip address and free to use. By free which means is not very reliable for serious use.

### Hosting your own service

#### Associate your app with Store first
This is required by Windows because you need an auth token from WNS, which requires your app to be associated. ( But not neccessarily to be published )

The default service used by term-notify can be found [here](https://github.com/tgckpg/term-notify-server). But you can always write your own server.

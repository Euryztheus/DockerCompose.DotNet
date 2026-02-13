using DockerComposeLiteAPI;

string arkfile = """
version: "3.3"
services:
  asa-server-1:
    container_name: asa-server-1
    hostname: asa-server-1
    entrypoint: "/usr/bin/start_server"
    user: gameserver
    image: "mschnitzer/asa-linux-server:latest"
    tty: true
    environment:
      - ASA_START_PARAMS=TheIsland_WP?listen?Port=7777?RCONPort=27020?RCONEnabled=True -WinLiveMaxPlayers=5 -NoBattlEye -mods=1434188,928793,1188679
      - ENABLE_DEBUG=0
      #-clusterid=default - ClusterDirOverride="/home/gameserver/cluster-shared"
    ports:
      # Game port for player connections through the server browser
      - 10.0.0.2:7777:7777/udp
      # RCON port for remote server administration
      - 10.0.0.2:27020:27020/tcp
    depends_on:
      - set-permissions-1
    volumes:
      - steam-1:/home/gameserver/Steam:rw
      - steamcmd-1:/home/gameserver/steamcmd:rw
      - server-files-1:/home/gameserver/server-files:rw
      - cluster-shared:/home/gameserver/cluster-shared:rw
      - /etc/localtime:/etc/localtime:ro
    networks:
      - asa-network
    sysctls:
      - net.ipv6.conf.all.disable_ipv6=1
      - net.ipv6.conf.default.disable_ipv6=1
  set-permissions-1:
    entrypoint: "/bin/bash -c 'chown -R 25000:25000 /steam ; chown -R 25000:25000 /steamcmd ; chown -R 25000:25000 /server-files ; chown -R 25000:25000 /cluster-shared'"
    user: root
    image: "opensuse/leap"
    volumes:
      - steam-1:/steam:rw
      - steamcmd-1:/steamcmd:rw
      - server-files-1:/server-files:rw
      - cluster-shared:/cluster-shared:rw
#  asa-server-2:
#    container_name: asa-server-2
#    hostname: asa-server-2
#    entrypoint: "/usr/bin/start_server"
#    user: gameserver
#    image: "mschnitzer/asa-linux-server:latest"
#    tty: true
#    environment:
#      - ASA_START_PARAMS=ScorchedEarth_WP?listen?Port=7778?RCONPort=27021?RCONEnabled=True -WinLiveMaxPlayers=50 -clusterid=default -ClusterDirOverride="/home/gameserver/cluster-shared"
#    ports:
#      # Game port for player connections through the server browser
#      - 0.0.0.0:7778:7778/udp
#      # RCON port for remote server administration
#      - 0.0.0.0:27021:27021/tcp
#    depends_on:
#      - set-permissions-2
#    volumes:
#      - steam-2:/home/gameserver/Steam:rw
#      - steamcmd-2:/home/gameserver/steamcmd:rw
#      - server-files-2:/home/gameserver/server-files:rw
#      - cluster-shared:/home/gameserver/cluster-shared:rw
#      - /etc/localtime:/etc/localtime:ro
#    networks:
#      asa-network:
#  set-permissions-2:
#    entrypoint: "/bin/bash -c 'chown -R 25000:25000 /steam ; chown -R 25000:25000 /steamcmd ; chown -R 25000:25000 /server-files ; chown -R 25000:25000 /cluster-shared'"
#    user: root
#    image: "opensuse/leap"
#    volumes:
#      - steam-2:/steam:rw
#      - steamcmd-2:/steamcmd:rw
#      - server-files-2:/server-files:rw
#      - cluster-shared:/cluster-shared:rw
volumes:
  cluster-shared:
  steam-1:
  steamcmd-1:
  server-files-1:
#  steam-2:
#  steamcmd-2:
#  server-files-2:
networks:
  asa-network:
    driver: bridge
    attachable: true
    driver_opts:
      com.docker.network.bridge.name: 'asanet'
      com.docker.network.driver.mtu: '1420'
    ipam:
      config:
        - subnet: 172.30.0.0/16

""";

string testfile = """
version: "3.8"

services:
  app:
    image: busybox:1.36
    container_name: dcl_app
    command: ["sh", "-c", "echo ok > /data/ok.txt; sleep 3600"]
    volumes:
      - dcl_data:/data:rw
    networks:
      - dcl_net
    ports:
      - 0.0.0.0:7777:7777/udp
      - 0.0.0.0:27020:27020/tcp

volumes:
  dcl_data:
#    external: true

networks:
  dcl_net:
    driver: bridge
""";

var lib = new ComposeLite(testfile, "composelite_test");
lib.ParseComposeFile();
await lib.ComposeUp();
Console.WriteLine();

//await lib.ComposeDown();
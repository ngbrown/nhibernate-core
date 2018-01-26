FROM debian:jessie

ENV PREFIX=/usr/local/firebird
ENV DEBIAN_FRONTEND noninteractive
ENV FBURL=http://downloads.sourceforge.net/project/firebird/firebird/3.0.2-Release/Firebird-3.0.2.32703-0.tar.bz2
ENV DBPATH=/databases

ADD build.sh ./build.sh

RUN chmod +x ./build.sh && \
    sync && \
    ./build.sh && \
    rm -f ./build.sh

VOLUME ["/databases", "/var/firebird/run", "/var/firebird/etc", "/var/firebird/log", "/var/firebird/system", "/tmp/firebird"]

EXPOSE 3050/tcp

ADD docker-entrypoint.sh ${PREFIX}/docker-entrypoint.sh
RUN chmod +x ${PREFIX}/docker-entrypoint.sh

ADD docker-entrypoint-async.sh ${PREFIX}/docker-entrypoint-async.sh
RUN chmod +x ${PREFIX}/docker-entrypoint-async.sh

ENTRYPOINT ${PREFIX}/docker-entrypoint-async.sh ${PREFIX}/bin/fbguard

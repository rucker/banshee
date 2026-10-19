# Reproducible rebuild of the December 2018 Banshee development environment.
#
# Reconstructed from rucker/docker-images (branch `banshee`, ubuntu/{baseimage,dev,banshee})
# and the layer history of the rucker/banshee:latest image. The original used
# floating refs (ubuntu:latest, gst `master`, latest meson/Mono) and built the
# dependencies at container runtime via banshee-setup; this version pins each
# one to what actually ran in 2018 and does all builds at image build time.
#
#   docker build -t rucker/banshee:2018 docker/
#   docker/run.sh

FROM ubuntu:18.04

ARG user=rick
ENV DEBIAN_FRONTEND=noninteractive \
    LANG=en_US.UTF-8

# --- ubuntu/baseimage ---
RUN apt-get update -qq && \
    apt-get install -y locales tzdata && \
    echo "America/New_York" > /etc/timezone && \
    ln -fs /usr/share/zoneinfo/America/New_York /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    sed -i 's,# en_US.UTF-8 UTF-8,en_US.UTF-8 UTF-8,' /etc/locale.gen && \
    locale-gen

# --- ubuntu/dev ---
RUN apt-get install -y ca-certificates curl dirmngr git gnupg sudo vim && \
    useradd $user --create-home -G sudo && \
    passwd -d $user

# --- ubuntu/banshee ---
RUN apt-get install -y libgconf2-dev libgstreamer1.0-dev libgstreamer-plugins-base1.0-dev libsqlite3-dev

# Mono 5.16.0.220 (the version in the 2018 image), pinned via the Mono repo snapshot
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic/snapshots/5.16.0.220 main" \
        > /etc/apt/sources.list.d/mono-official-stable.list && \
    apt-get update && \
    apt-get install -y mono-devel msbuild

RUN apt-get install -y gnome-sharp2 gtk-sharp3 libdbus-glib2.0-cil-dev libgtk-3-dev libgtk2.0-cil-dev \
        libgtk2.0-dev libmono-addins-cil-dev libtaglib-cil-dev libdbus-glib-1-dev \
        autoconf automake autopoint gettext libtool pkg-config

# GStreamer dependencies (meson/ninja/flex only served the omitted gstreamer-sharp build)
RUN apt-get install -y libgstreamer-plugins-good1.0-dev libgstreamer-plugins-bad1.0-dev libxml2-dev \
        gstreamer1.0-plugins-good gstreamer1.0-plugins-ugly gstreamer1.0-libav gstreamer1.0-alsa \
        gstreamer1.0-pulseaudio

# --- banshee-setup, moved to build time ---
WORKDIR /build

# banshee-setup also built gst-editing-services and gstreamer-sharp (1.14.0) for the managed
# GStreamer backend, but gstreamer-sharp 1.14.0's meson build installs no assembly or .pc file,
# so Banshee's configure never found it. The 2018 build that ran used the native C backend
# (libbanshee); see the configure flags below. Those steps are omitted here.

# rucker/dbus-sharp: "Fix compilation errors" (re-enable Mono.Unix usings)
RUN git clone https://github.com/rucker/dbus-sharp.git && \
    cd dbus-sharp && \
    git checkout ab1ad66880f2af95068c6fe05ed4c5e3a9ee80a2 && \
    ./autogen.sh && \
    msbuild src/dbus-sharp.csproj '/p:WarningLevel=0;Configuration=Release' && \
    gacutil -i src/dbus-sharp.dll

# Banshee: arfbtwn's feature/build branch as forked to rucker/banshee (c104af51d), Hyena c72e520
USER $user
WORKDIR /home/$user
RUN git clone https://github.com/rucker/banshee.git && \
    cd banshee && \
    git checkout c104af51d0dffe1ce360922c4832538f8bfd03da && \
    git submodule update --init && \
    # gtk-sharp3 2.99.3 lacks Atk.ValueChangedHandler and Atk.Range
    sed -i '413s,^,//,' src/Hyena/Hyena.Gui/Hyena.Widgets/RatingEntry.cs && \
    sed -i '433,436{s,^,//,}' src/Hyena/Hyena.Gui/Hyena.Widgets/RatingEntry.cs && \
    autoreconf -ivf && \
    ./configure --disable-tests --disable-docs --disable-mass-storage --disable-mtp --disable-appledevice \
        --disable-karma --disable-amazonmp3 --disable-amazonmp3-store --disable-audiobook --disable-booscript \
        --disable-emusic --disable-emusic-store --disable-internetarchive --disable-lastfm \
        --disable-lastfmstreaming --disable-opticaldisc --disable-torrent --disable-ubuntuone \
        --disable-wikipedia --disable-youtube --disable-halie --disable-beroe --disable-mediapanel \
        --disable-muinshee --enable-gnome --enable-gst-native --disable-gst-sharp && \
    make -j"$(nproc)"

# Created here so the banshee-config volume mounted by run.sh is initialized owned by $user
# (Docker would otherwise create the mount point, and ~/.config, as root).
RUN mkdir -p /home/$user/.config/banshee-1

WORKDIR /home/$user/banshee
CMD ["make", "run"]

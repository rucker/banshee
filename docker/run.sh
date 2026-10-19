#!/bin/bash
# Launch the dockerized Banshee on the host's X server (XWayland works too).
# Adapted from the `banshee:run` task in rucker/docker-images ubuntu/banshee/Rakefile.
# Banshee's config and library database live in the `banshee-config` volume rather than
# the host's ~/.config/banshee-1. The music folder is mounted read-write so Banshee can
# write tag/metadata edits back to the files.

# Audio goes through the host's PulseAudio socket (served by pipewire-pulse on current Ubuntu)
# rather than the original --device /dev/snd, which competes with PipeWire for the hardware.

IMAGE=${IMAGE:-rucker/banshee:2018}
MUSIC_DIR=${MUSIC_DIR:-${HOME}/Music}
PULSE_SOCKET=${XDG_RUNTIME_DIR:-/run/user/$(id -u)}/pulse/native

xhost +si:localuser:"$(whoami)" > /dev/null 2>&1

exec docker run -it --rm \
    -e DISPLAY="${DISPLAY}" \
    -v /tmp/.X11-unix:/tmp/.X11-unix \
    -e PULSE_SERVER=unix:/run/pulse/native \
    -v "${PULSE_SOCKET}":/run/pulse/native \
    -v "${MUSIC_DIR}":/home/rick/Music \
    -v banshee-config:/home/rick/.config/banshee-1 \
    "${IMAGE}" "$@"

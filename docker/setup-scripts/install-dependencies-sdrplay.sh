#!/bin/bash

echo "Installing SDRplay dependencies"

apt-get install -y --no-install-recommends libusb-1.0-0 sudo udev

(cd /tmp/sdrplay && /tmp/sdrplay/install_lib.sh)

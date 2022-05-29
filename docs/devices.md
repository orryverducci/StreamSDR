# Device Specific Notes

StreamSDR supports a range of software defined radios.

As StreamSDR uses the rtl_tcp server protocol, which was originally designed for use with only RTL-SDR devices, StreamSDR attempts to emulate an RTL-SDR device when using other SDR devices. As such some radios may not support features supported by RTL-SDR devices, and some more advanced features provided by other radios may not be supported by StreamSDR.

## RTL-SDR

To use RTL-SDR devices the appropriate drivers will need to be installed on the host system. We recommend reading the [Quick Start Guide](https://www.rtl-sdr.com/rtl-sdr-quick-start-guide/) provided by the RTL-SDR blog for instructions on how to set up a RTL-SDR device.

All features provided by RTL-SDR devices are fully supported by StreamSDR.

## SDRplay

To use SDRplay devices the SDRplay API needs to be installed on the host system. This can be downloaded from the [SDRplay website](https://www.sdrplay.com/softwarehome/). StreamSDR requires version 3 of the SDRplay API.

Due to the limitations of the rtl_tcp protocol the bit depth of the samples provided by the radio will be limited to 8-bit.

SDRplay radios use two gain controls, the LNA gain and the tuner gain, which vary depending on the radio band the tuner is tuned to. As the rtl_tcp protocol doesn't support this these controls have been mapped to 29 levels of gain which automatically adjust as the tuner changes between the radio bands.

Sample rates between 2 MHz and 3.2 MHz are directly supported by StreamSDR. Higher sample rates supported by SDRplay radios are supported by StreamSDR, but most rtl_tcp client applications will only allow samples rates up 3.2 MHz to be selected. Select sample rates below 2 MHz are supported by StreamSDR through the use of decimation on the radio to achieve the desired sample rate. The supported sample rates below 2 MHz are:

* 1 MHz
* 500 kHz
* 250 kHz
* 125 kHz

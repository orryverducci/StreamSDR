# Installing StreamSDR

## Windows

### Installing and Updating

StreamSDR can be installed or updated using the MSI installers provided on StreamSDR's [releases page](https://github.com/orryverducci/StreamSDR/releases). Two installers are provided for the Windows platform, one for 64-bit Intel/AMD (x64) systems and another for 64-bit ARM (ARM64) systems.

To install download the latest version of the appropriate installer for your system, open the installer from the File Explorer, and then follow the on screen instructions.

StreamSDR supports the following versions of Windows:

* Windows 10
* Windows 11
* Windows Server 2016
* Windows Server 2019
* Windows Server 2022

### Uninstalling

You can uninstall StreamSDR from within Windows' Settings application. You can find the page to uninstall apps by selecting the Apps option, and then Apps & features from the menu. Then select StreamSDR from the list of installed applications and click the Uninstall button that appears.

## macOS

### Installing and Updating

StreamSDR can be installed or updated using the installer package provided on StreamSDR's [releases page](https://github.com/orryverducci/StreamSDR/releases).

To install download the latest version of the installer package, open the package within Finder, and then follow the on screen instructions.

The intaller package supports both Intel and Apple Silicon (aka M1) systems.

### Uninstalling

To uninstall StreamSDR open the Terminal application from Launchpad and run the following command:

```console
sudo rm /usr/local/bin/streamsdr
```

You will be prompted for your password which you will need to enter to complete the uninstallation.

## Docker

StreamSDR provides a Docker image that can be used to run the app within a container. Although Docker supports multiple operating systems, StreamSDR only supports being run in a container on Linux systems due to the need to access hardware (i.e. SDR devices) connected to the system.

To run StreamSDR in Docker:

1. Follow the [official guide to install Docker](https://docs.docker.com/engine/install/) if it is not already installed.
2. Download the latest container image by running the following command in your terminal:

        docker pull orryverducci/streamsdr

3. Create and run a container in one of the following ways:

    * **Docker command line interface:**

        Run the following command in your terminal:

            docker run -d --name streamsdr --publish 1234:1234 --privileged orryverducci/streamsdr

        This command runs the container with privileged capabilities, which allows full access to the hardware connected to the host system. If you don't wish to enable this you can remove `--privileged` from the command, but you will instead need to use the `--device` option to specifically allow access to each SDR device you wish to use.
    
    * **Docker Compose:**

        Create a `docker-compose.yml` file containing the following:
    
            version: "3.9"
            services:
                streamsdr:
                    image: orryverducci/streamsdr
                    ports:
                        - "1234:1234"
                    privileged: true

        You can then start the service from the compose file by running the following command in the terminal:

            docker-compose up

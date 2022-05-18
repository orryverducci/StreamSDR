# Using StreamSDR

## Launching StreamSDR

StreamSDR can be launched from the system command line interface, or on Windows systems via the shortcuts in the Start Menu.

### Windows Start Menu

Multiple shortcuts to launch StreamSDR can be found in the Windows Start Menu. There is one shortcut for each type of radio supported by StreamSDR that will launch the application with the default settings for that radio.

### Command Line Interface

StreamSDR can be run within the Terminal on Linux and macOS systems, and within the Command Prompt, PowerShell or Windows Terminal on Windows systems. You can start the application by typing the `streamsdr` command in to the command line.

Additional arguments can be typed in after the `streamsdr` command to set the available application or radio settings. Arguments are set by typing the setting name prefixed with two dashes followed by the parameter for the setting. For example you can set the type of radio to be used to a RTL-SDR by typing in the ‘radio’ setting with the ‘rtlsdr’ parameter as below:

```console
streamsdr --radio rtlsdr
```

## Available Settings

* **radio** — The type of radio to use. Defaults to rtlsdr. The following options are available:
    * **rtlsdr** — RTL-SDR devices
    * **sdrplay** — SDRplay devices
* **serial** — The serial number of radio to use. Defaults to the first device returned by the system.
* **debug** — A mode that provides additional logging to diagnose issues. Disabled by default, set to true to enable.

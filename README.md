# VISCA

## License

Provided under MIT license

## Overview

This repo is for VISCA camera plugin.

## Device Specific

### RS-232 Communications

| Setting      | Value                       |
|--------------|-----------------------------|
| Baud rate    | 9600 or 38,400 (selectable) |
| Data bits    | 8                           |
| Stop bits    | 1                           |
| Parity       | None                        |
| Flow control | none                        |

### VISCA Over IP (UDP) **NOT COMPLETE**

| Setting      | Value |
|--------------|-------|
| Default IP   |       |
| Default Port | 52381 |
| Username     |       |
| Password     |       |

## Configuration

An example configuration is provided to assist in implementation

### Device Configuation

The device configuration provides an example of the Vaddio OneLink device, control method, presets and other device properties.

```
{	
	"key": "camera-1",
	"name": "VISCA Camera",
	"type": "visca",
	"group": "pluginDevices",
	"properties": {
		"control": {
			"method": "tcpIp",
			"tcpSshProperties": {
				"address": "127.0.0.1",
				"port": 23,
				"username": "admin",
				"password": "password",
				"autoReconnect": true,
				"autoReconnectIntervalMs": 10000
			}
		},
	},
	"pollTimeMs": 30000,
	"warningTimeoutMs": 180000,
	"errorTimeoutMs": 300000,
	"address": 1,
	"panSpeed": 12,
	"tiltSpeed": 10,
	"zoomSpeed": 3,
	"focusSpeed": 4,
	"privacyOnPreset": 15,
	"privacyOffPreset": 1,
	"presets": {
		"1": {
			"enabled": true,
			"name": "Preset 1"
		},
		"2": {
			"enabled": true,
			"name": "Preset 2"
		}
	}
}
```


### Bridge Configuration

It is important to note the Vaddio OneLink Plugin is built on the Essentials Plugin Template and uses the **eiscApiAdvanced** type.  The following configuration is an example of the Bridge configuration.

```
{
	"key": "plugin-bridge-1",
	"uid": 11,
	"name": "Communication Bridge",
	"group": "api",
	"type": "eiscApi",
	"properties": {
		"control": {
		"tcpSshProperties": {
			"address": "127.0.0.2",
			"port": 0
		},
		"ipid": "1A"
		},
		"devices": [
			{
				"deviceKey": "camera-1",
				"joinStart": 1
			}
		]
	}
}
```

## SiMPL Bridge Joins

### Digitals
| dig-o                                    | I/O     | dig-i               |
|------------------------------------------|---------|---------------------|
| Tilt Up                                  | 1       |                     |
| TiltDown                                 | 2       |                     |
| Pan Left                                 | 3       |                     |
| Pan Right                                | 4       |                     |
| Zoom In                                  | 5       |                     |
| Zoom Out                                 | 6       |                     |
| Power On                                 | 7       | Power On Feedback   |
| Power Off                                | 8       | Power Off Feedback  |
| Is Online                                | 9       |                     |
| Home                                     | 10      |                     |
| Preset Recall (Press)/Preset Save (Hold) | 11 - 26 |                     |
| Auto Focus                               | 30      | Auto Focus Feedback |
| Preset Save (Press)                      | 31 - 46 |                     |
| Privacy On                               | 48      |                     |
| Privacy Off                              | 49      |                     |

## Analogs
| an_o        | I/O | an_i                 |
|-------------|-----|----------------------|
| Tilt Speed  | 1   | Tilt Speed Feedback  |
| Pan Speed   | 2   | Pan Speed Feedback   |
| Zoom Speed  | 3   | Zoom Speed Feedback  |
| Focus Speed | 4   | Focus Speed Feedback |
|             | 11  | Preset Count         |
|             | 50  | Status               |

## Serials
| serial-o | I/O     | serial-i     |
|----------|---------|--------------|
|          | 1       | Device Name  |
|          | 11 - 26 | Preset Names |
# VISCA

## License

Provided under MIT license

## Overview

This repo is for VISCA camera plugin.

## Device Specific

### Plugin Valid Communication methods

```c#
Comm
Tcpip
Udp
```

### RS-232 Communications

| Setting      | Value                       |
| ------------ | --------------------------- |
| Baud rate    | 9600 or 38,400 (selectable) |
| Data bits    | 8                           |
| Stop bits    | 1                           |
| Parity       | None                        |
| Flow control | none                        |

### VISCA-over-IP via TCP/IP Communications

| Setting      | Value |
| ------------ | ----- |
| Default IP   |       |
| Default Port | 5500  |
| Username     |       |
| Password     |       |

### VISCA-over-IP via UDP Communications

| Setting      | Value |
| ------------ | ----- |
| Default IP   |       |
| Default Port | 52381 |
| Username     |       |
| Password     |       |

#### Device Configuration

```json
{	
	"key": "camera-1",
	"name": "VISCA Camera",
	"type": "visca",
	"group": "pluginDevices",
	"properties": {
		"control": {},
		"pollTimeMs": 30000,
		"address": 1,
		"panSpeed": 12,
		"tiltSpeed": 10,
		"zoomSpeed": 3,
		"focusSpeed": 4,
		"privacyOnPreset": 15,
		"privacyOffPreset": 1,
		"presets": [
			{
				"id": 15,
				"name": "Privacy On"
			},
			{
				"id": 2,
				"name": "Preset 2"
			}
		]
	}
}
```

### Bridge Configuration

The following configuration is an example of the Bridge configuration.

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
| dig-o                                     | I/O     | dig-i                 |
| ----------------------------------------- | ------- | --------------------- |
| Tilt Up                                   | 1       |                       |
| TiltDown                                  | 2       |                       |
| Pan Left                                  | 3       |                       |
| Pan Right                                 | 4       |                       |
| Zoom In                                   | 5       |                       |
| Zoom Out                                  | 6       |                       |
| Power On                                  | 7       | Power On Feedback     |
| Power Off                                 | 8       |                       |
|                                           | 9       | Is Online Feedback    |
| Home                                      | 10      |                       |
| Preset Select (Press)/Preset Store (Hold) | 11 - 26 |                       |
| Focus Near                                | 28      |                       |
| Focus Far                                 | 29      |                       |
|                                           | 30      | Preset Store Feedback |
| Preset Store (Press)                      | 31 - 46 |                       |
| Privacy On                                | 48      |                       |
| Privacy Off                               | 49      |                       |
| Trigger Auto Focus                        | 50      |                       |

## Analogs
| an_o                    | I/O | an_i                       |
| ----------------------- | --- | -------------------------- |
| Pan Speed               | 1   | Pan Speed Feedback         |
| Tilt Speed              | 2   | Tilt Speed Feedback        |
| Zoom Speed              | 3   | Zoom Speed Feedback        |
| Focus Speed             | 4   | Focus Speed Feedback       |
| Preset Select by Number | 11  | Number of Presets Feedback |
| Preset Store by Number  | 12  |                            |
|                         | 50  | Status                     |

## Serials
| serial-o           | I/O     | serial-i     |
| ------------------ | ------- | ------------ |
|                    | 1       | Device Name  |
|                    | 11 - 26 | Preset Names |
| Device Comms (WIP) | 50      |              |